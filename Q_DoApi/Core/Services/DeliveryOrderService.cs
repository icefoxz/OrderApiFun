using System.Linq.Expressions;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderDbLib;
using OrderDbLib.Entities;
using Utls;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using Q_DoApi.Core.Utls;
using Q_DoApi.DtoMapping;
using WebUtlLib;

namespace OrderApiFun.Core.Services
{
    public class DoService
    {
        private OrderDbContext Db { get; }
        private UserManager<User> UserManager { get; }
        private LingauManager LingauManager { get; }
        private RiderManager RiderManager { get; }
        public DoService(OrderDbContext db, UserManager<User> userManager, RiderManager riderManager, LingauManager lingauManager)
        {
            Db = db;
            UserManager = userManager;
            RiderManager = riderManager;
            LingauManager = lingauManager;
        }

        public async Task<DeliveryOrder> CreateDeliveryOrderAsync(string userId, DeliverOrderModel dto, ILogger log)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null) throw new NullReferenceException("user is null!");
            var newOrder = dto.Adapt<DeliveryOrder>(EntityMapper.Config);

            newOrder.User = user;
            newOrder.UserId = userId;
            newOrder.SenderInfo = new SenderInfo
            {
                User = user,
                UserId = userId,
                Name = dto.SenderInfo.Name,
                PhoneNumber = dto.SenderInfo.PhoneNumber,
                NormalizedPhoneNumber = MyPhone.NormalizePhoneNumber(dto.SenderInfo.PhoneNumber),
            };
            var receiver = await UserManager.FindByIdAsync(newOrder.ReceiverInfo.UserId);
            if (receiver != null)
            {
                newOrder.ReceiverInfo = CreateOrderReceiverInfo(receiver.Name, receiver.PhoneNumber);
                newOrder.ReceiverInfo.User = receiver;
                newOrder.ReceiverInfo.UserId = userId;
            }
            else
            {
                newOrder.ReceiverInfo =
                    CreateOrderReceiverInfo(dto.ReceiverInfo.Name, dto.ReceiverInfo.PhoneNumber);
            }

            Db.DeliveryOrders.Add(newOrder);
            await Db.SaveChangesAsync();

            log.LogInformation($"DeliveryOrder created with ID: {newOrder.Id}");
            return newOrder;

            ReceiverInfo CreateOrderReceiverInfo(string receiverName, string phoneNumber)
            {
                return new ReceiverInfo
                {
                    Name = receiverName,
                    PhoneNumber = phoneNumber,
                    NormalizedPhoneNumber = MyPhone.NormalizePhoneNumber(phoneNumber)
                };
            }
        }

        public async Task<PageList<DeliveryOrder>> User_GetDeliveryOrdersAsync(string? userId, int pageSize, int pageIndex,
            Expression<Func<DeliveryOrder, bool>>? filter, ILogger log)
        {
            log.Event($"GetDeliveryOrders: userId={userId}, pageSize={pageSize}, page={pageIndex}");
            var index = Math.Max(0, pageIndex);
            var count = await OrderCountAsync(userId, filter);
            var query = Db.DeliveryOrders
                .Include(d => d.Rider)
                .Include(d => d.ReceiverInfo.User)
                .Include(d => d.SenderInfo.User)
                .Include(d => d.User)
                .ThenInclude(u => u.Lingau)
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            var list = await query.OrderByDescending(o => o.CreatedAt)
                .Skip(pageSize * index)
                .Take(pageSize)
                .ToArrayAsync();
            return new PageList<DeliveryOrder>(index, pageSize, count, list);
        }

        public async Task<PageList<DeliveryOrder>> GetPageList(
            int pageSize, int index, ILogger log,
            Expression<Func<DeliveryOrder, bool>>? filter = null,
            params (Expression<Func<DeliveryOrder, object>> sortExpression, SortDirection sortDirection)[] sorts)
        {
            log.LogInformation($"GetDeliveryOrders: pageSize={pageSize}, index={index}");
            index = Math.Max(1, index);
            var query = Db.DeliveryOrders
                .Include(d => d.Rider)
                .Include(d => d.ReceiverInfo.User)
                .Include(d => d.SenderInfo.User)
                .Include(d => d.User)
                .ThenInclude(u => u.Lingau)
                .Where(o => !o.IsDeleted);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var count = await query.CountAsync();

            query = AddInOrderedSorts(sorts, query);

            var array = await query
                .Skip(pageSize * (index - 1))
                .Take(pageSize)
                .ToArrayAsync();

            return PageList.Instance(index, pageSize, count, array);
        }

        public async Task<DeliveryOrder?> GetFirstAsync(Expression<Func<DeliveryOrder, bool>> filter,
            params (Expression<Func<DeliveryOrder, object>> sortExpression, SortDirection sortDirection)[] sorts)
        {
            var query = Db.DeliveryOrders
                .Include(d => d.User)
                .Where(o=>!o.IsDeleted)
                .Where(filter);
            return await AddInOrderedSorts(sorts, query).FirstOrDefaultAsync();
        }

        public async Task PayDeliveryOrderByLingauAsync(string userId, DeliveryOrder order, ILogger log)
        {
            log.LogInformation($"PayDeliveryOrderByLingau: userId={userId}, orderId={order.Id}");
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");
            var lingau = await LingauManager.GetLingauAsync(userId);
            var price = order.PaymentInfo.Charge;
            if (price < 0) throw new InvalidOperationException($"Invalid price: {price} from order.{order.Id}");
            if (lingau.Credit < price)
            {
                throw new InvalidOperationException("Insufficient balance");
            }
            await LingauManager.UpdateLingauBalanceAsync(userId, price, log);
        }

        public async Task<ResultOf<DeliveryOrder>> SubState_Update(DeliveryOrder order, TransitionRoles role, int subState, ILogger log)
        {
            log.Event($"Order.{order.Id}, {order.SubState} => {subState}, subState = {subState}");
            if (!DoStateMap.IsAssignableSubState(role, order.SubState, subState))
                return ResultOf.Fail(order, "State not assignable.");
            await UpdateOrderStateAsync(order, subState, log);
            return ResultOf.Success(order, string.Empty);
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task UpdateOrderStateAsync(DeliveryOrder order, int subState, ILogger log)
        {
            order.Status = subState.ConvertToDoStatusInt();
            order.SubState = subState;
            await Db.SaveChangesAsync();
            log.Event($"Order[{order.Id}] state updated to {order.Status}");
        }

        public async Task<ResultOf<DeliveryOrder>> AssignRiderAsync(long deliveryOrderId, Rider rider)
        {
            var deliveryOrder = await GetFirstAsync(o => o.Id == deliveryOrderId);
            if (deliveryOrder == null) return ResultOf.Fail<DeliveryOrder>("Order not found!");
            if (deliveryOrder.RiderId != null) return ResultOf.Fail<DeliveryOrder>("Order already assigned!");
            deliveryOrder.RiderId = rider.Id;
            deliveryOrder.Rider = rider;

            deliveryOrder.Status = DoSubState.AssignState.ConvertToDoStatusInt();
            deliveryOrder.SubState = DoSubState.AssignState;
            await Db.SaveChangesAsync();
            return ResultOf.Success(deliveryOrder);
        }
        #region private_methods
        private async Task<int> OrderCountAsync(string? userId, Expression<Func<DeliveryOrder, bool>>? filter)
        {
            var query = Db.DeliveryOrders.AsNoTracking();
            if (filter != null)
                query = query.Where(filter);
            return await query.CountAsync(o => o.UserId == userId && !o.IsDeleted);
        }
        // 应用提供的排序规则
        private static IOrderedQueryable<DeliveryOrder> AddInOrderedSorts(
            (Expression<Func<DeliveryOrder, object>> sortExpression, SortDirection sortDirection)[] sorts,
            IQueryable<DeliveryOrder> query)
        {
            IOrderedQueryable<DeliveryOrder> orderedQuery;
            if (sorts is { Length: > 0 })
            {
                var firstSort = sorts[0];
                orderedQuery = firstSort.sortDirection == SortDirection.Ascending
                    ? query.OrderBy(firstSort.sortExpression)
                    : query.OrderByDescending(firstSort.sortExpression);

                foreach (var sort in sorts.Skip(1))
                {
                    orderedQuery = sort.sortDirection == SortDirection.Ascending
                        ? orderedQuery.ThenBy(sort.sortExpression)
                        : orderedQuery.ThenByDescending(sort.sortExpression);
                }
            }
            else
            {
                orderedQuery = query.OrderByDescending(o => o.CreatedAt);
            }

            // 总是以 CreatedAt 作为最后一个排序规则
            orderedQuery = orderedQuery.ThenByDescending(o => o.CreatedAt);
            return orderedQuery;
        }
        #endregion
    }
}
