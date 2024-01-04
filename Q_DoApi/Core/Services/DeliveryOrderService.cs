using System.Linq.Expressions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrderApiFun.Core.Services;
using OrderDbLib;
using OrderDbLib.Entities;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos;
using OrderHelperLib.Dtos.DeliveryOrders;
using Q_DoApi.Core.Utls;
using Q_DoApi.DtoMapping;
using Utls;
using WebUtlLib;
using WebUtlLib.Services;

namespace Q_DoApi.Core.Services
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

        public async ValueTask<ResultOf<DeliveryOrder>> Do_CreateAsync(string userId, DeliverOrderModel orderDto, ILogger log)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                log.Event($"User not found! id = {userId}");
                return ResultOf.Fail<DeliveryOrder>("User not found!");
            }
            if (!MyPhone.VerifyPhoneNumber(orderDto.SenderInfo.PhoneNumber))
            {
                log.Event($"Invalid sender phone number. {orderDto.SenderInfo.PhoneNumber}");
                return ResultOf.Fail<DeliveryOrder>("Invalid sender phone number.");
            }

            if (!MyPhone.VerifyPhoneNumber(orderDto.ReceiverInfo.PhoneNumber))
            {
                log.Event($"Invalid receiver phone number. {orderDto.ReceiverInfo.PhoneNumber}");
                return ResultOf.Fail<DeliveryOrder>("Invalid receiver phone number.");
            }

            var result = await Do_GetPrice(orderDto, log);
            if (!result.IsSuccess)
                return ResultOf.Fail<DeliveryOrder>(result.Message);
            var dto = result.Data;
            orderDto.PaymentInfo = new PaymentInfoDto
            {
                Fee = dto.PaymentInfo.Fee,
                Charge = dto.PaymentInfo.Fee,
            };
            var newDo = await CreateNewDo(user, orderDto, log);
            return ResultOf.Success(newDo);
        }

        private async Task<DeliveryOrder> CreateNewDo(User user, DeliverOrderModel dto, ILogger log)
        {
            var newOrder = dto.Adapt<DeliveryOrder>(EntityMapper.Config);
            newOrder.User = user;
            newOrder.UserId = user.Id;
            newOrder.SenderInfo = new SenderInfo
            {
                User = user,
                UserId = user.Id,
                Name = dto.SenderInfo.Name,
                PhoneNumber = dto.SenderInfo.PhoneNumber,
                NormalizedPhoneNumber = MyPhone.NormalizePhoneNumber(dto.SenderInfo.PhoneNumber),
            };
            var receiver = await UserManager.FindByIdAsync(newOrder.ReceiverInfo.UserId);
            if (receiver != null)
            {
                newOrder.ReceiverInfo = CreateOrderReceiverInfo(receiver.Name, receiver.PhoneNumber);
                newOrder.ReceiverInfo.User = receiver;
                newOrder.ReceiverInfo.UserId = user.Id;
            }
            else
            {
                newOrder.ReceiverInfo =
                    CreateOrderReceiverInfo(dto.ReceiverInfo.Name, dto.ReceiverInfo.PhoneNumber);
            }

            newOrder.AddStateHistory(DoStateMap.Created[0]);
            Db.DeliveryOrders.Add(newOrder);
            await Db.SaveChangesAsync();

            log.Event($"DeliveryOrder created with ID: {newOrder.Id}");
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

        public async ValueTask<PageList<DeliveryOrder>> User_DoPage_GetAsync(string? userId, int pageSize, int pageIndex,
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
                .ToListAsync();
            return new PageList<DeliveryOrder>(index, pageSize, count, list);
        }

        public async ValueTask<PageList<DeliveryOrder>> DoPage_GetAsync(
            int pageSize, int index, ILogger log,
            Expression<Func<DeliveryOrder, bool>>? filter = null,
            params (Expression<Func<DeliveryOrder, object>> sortExpression, SortDirection sortDirection)[] sorts)
        {
            log.LogInformation($"GetDeliveryOrders: pageSize={pageSize}, index={index}");
            index = Math.Max(0, index);
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
                .Skip(pageSize * index)
                .Take(pageSize)
                .ToListAsync();

            return PageList.Instance(index, pageSize, count, array);
        }

        public Task<DeliveryOrder?> Do_FirstAsync(Expression<Func<DeliveryOrder, bool>> filter,
            params (Expression<Func<DeliveryOrder, object>> sortExpression, SortDirection sortDirection)[] sorts)
        {
            var query = Db.DeliveryOrders
                .Include(d => d.User)
                .Where(o=>!o.IsDeleted)
                .Where(filter);
            return AddInOrderedSorts(sorts, query).FirstOrDefaultAsync();
        }

        public async ValueTask<ResultOf<DeliveryOrder>> Do_SubState_Update(DeliveryOrder order, TransitionRoles role,
            string subState, ILogger log)
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
        private async Task UpdateOrderStateAsync(DeliveryOrder order, string subState, ILogger log)
        {
            var state = DoStateMap.GetState(subState);
            if (state == null) throw new ArgumentException($"Invalid subState: {subState}");
            order.Status = state.Status;
            order.SubState = subState;
            order.AddStateHistory(state);
            await Db.SaveChangesAsync();
            log.Event($"Order[{order.Id}] state updated to {order.Status}");
        }

        public async ValueTask<ResultOf<DeliveryOrder>> Do_Rider_AssignAsync(long deliveryOrderId, Rider rider)
        {
            var deliveryOrder = await Do_FirstAsync(o => o.Id == deliveryOrderId);
            if (deliveryOrder == null) return ResultOf.Fail<DeliveryOrder>("Order not found!");
            if (deliveryOrder.Rider != null) return ResultOf.Fail<DeliveryOrder>("Order already assigned!");
            deliveryOrder.RiderId = rider.Id;
            deliveryOrder.Rider = rider;

            deliveryOrder.Status = DoSubState.AssignState.ConvertToDoStatusInt();
            deliveryOrder.SubState = DoSubState.AssignState;
            deliveryOrder.AddStateHistory(DoSubState.AssignState, $"{rider.Name} - {rider.Phone}");
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

        public async ValueTask<ResultOf<Lingau>> DoPay_ByLingau(string userId, long deliveryOrderId, ILogger log)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                log.Event($"User not found! id = {userId}");
                return ResultOf.Fail<Lingau>("User not found!");
            }

            var order = await Do_FirstAsync(o => o.Id == deliveryOrderId);
            if (order == null)
            {
                log.Event($"Order not found! id = {deliveryOrderId}");
                return ResultOf.Fail<Lingau>("Order not found!");
            }

            var result = await LingauManager.UpdateLingauBalanceAsync(userId, order.PaymentInfo.Charge, log, false);
            if (!result.IsSuccess)
            {
                log.Event($"UpdateLingauBalanceAsync failed! {result.Message}");
                return ResultOf.Fail<Lingau>(result.Message);
            }
            log.Event(
                $"User[{userId}].Lingau.Credit = {user.Lingau.Credit}, Order[{order.Id}].PaymentInfo.Charge = {order.PaymentInfo.Charge}");
            order.PaymentInfo.IsReceived = true;
            order.PaymentInfo.Method = PaymentMethods.UserCredit.ToDisplayString();
            order.PaymentInfo.Reference = userId;
            log.Event($"Order[{order.Id}].Payment Received! Method = {order.PaymentInfo.Method}, Reference = {userId}");
            await Db.SaveChangesAsync();
            return ResultOf.Success(result.Data);
        }

        public async ValueTask<ResultOf<DeliveryOrder>> DoPay_RiderCollect(string userId, long deliveryOrderId,
            ILogger log)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                log.Event($"User not found! id = {userId}");
                return ResultOf.Fail<DeliveryOrder>("User not found!");
            }

            var order = await Do_FirstAsync(o => o.Id == deliveryOrderId);
            if (order == null)
            {
                log.Event($"Order not found! id = {deliveryOrderId}");
                return ResultOf.Fail<DeliveryOrder>("Order not found!");
            }

            order.PaymentInfo.Method = PaymentMethods.RiderCollection.ToDisplayString();
            log.Event($"Order[{order.Id}].PaymentInfo.Method = {order.PaymentInfo.Method}");
            await Db.SaveChangesAsync();
            return ResultOf.Success(order);
        }

        public async ValueTask<ResultOf<DeliverOrderModel>> Do_GetPrice(DeliverOrderModel dto, ILogger log)
        {
            log.Event($"{nameof(Do_GetPrice)}()");
            var item = dto.ItemInfo;
            var vol = GetVolume(item);
            var weight = item.Weight;
            var deliver = dto.DeliveryInfo;
            var startLat = deliver.StartLocation.Latitude;
            var startLng = deliver.StartLocation.Longitude;
            var endLat = deliver.EndLocation.Latitude;
            var endLng = deliver.EndLocation.Longitude;
            var km = await GetGoogleMatrixDistance(startLat, startLng, endLat, endLng);
            if (km < 0) return ResultOf.Fail<DeliverOrderModel>("Failed to get distance.");
            var price = GetPrice((float)km, vol, weight);
            dto.PaymentInfo.Fee = (float)price;
            dto.DeliveryInfo.Distance = (float)km;
            return ResultOf.Success(dto);

            double GetPrice(float km, float size, float kg)
            {
                var vol = size > kg ? size : kg;
                var price = GetKmMultiplier(km) * GetVolMultiplier(vol);
                return price;
            }

            float ToCm(float m) => m * 100;

            async Task<double> GetGoogleMatrixDistance(double startLat, double startLng, double endLat,
                double endLng)
            {
                try
                {
                    var apiKey = Config.GetGoogleApiKey();
                    var requestUrl =
                        $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={startLat},{startLng}&destinations={endLat},{endLng}&key={apiKey}";
                    log.Event($"Requesting ... {requestUrl}");
                    using var client = new HttpClient();
                    var response = await client.GetAsync(requestUrl);
                    var content = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        log.Event($"Failed: {response.StatusCode}\n{content}");
                        return -1;
                    }
                    JObject jsonResponse = JObject.Parse(content);
                    // 解析 JSON 数据以获取距离（米）
                    var obj = jsonResponse["rows"][0]["elements"][0];
                    var meters = (double)obj["distance"]["value"];
                    log.Event($"Meters = {meters}");
                    // 这里您可以根据距离、体积和重量计算价格
                    // 示例：return distance / 1000; // 将距离从米转换为公里
                    return meters / 1000;
                }
                catch (Exception e)
                {
                    log.Event("result failed:\n" + e);
                    return -1;
                }
            }

            float GetVolume(ItemInfoDto info) => ToCm(info.Width) * ToCm(info.Height) * ToCm(info.Length) / 50000;

            double GetKmMultiplier(float km)
            {
                if (km < 1) return 1;
                return km / 3 * 0.5;
            }

            double GetVolMultiplier(float v)
            {
                if (v <= 1)
                    return 5;
                return 5 * v;
            }
        }

    }
}
