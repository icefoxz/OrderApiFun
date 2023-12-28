using System.Net;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderApiFun.Core.Services;
using OrderDbLib.Entities;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using Q_DoApi.Core.Extensions;
using Q_DoApi.Core.Services;
using Utls;
using WebUtlLib;

namespace FunctionApp1.Funcs
{
    public class RiderDoFunc
    {
        private LingauManager LingauManager { get; }
        private DoService DoService { get; }
        private RiderManager RiderManager { get; }
        private UserManager<User> UserManager { get; }

        public RiderDoFunc(DoService doService,
            UserManager<User> userManager,
            RiderManager riderManager,
            LingauManager lingauManager)
        {
            DoService = doService;
            UserManager = userManager;
            RiderManager = riderManager;
            LingauManager = lingauManager;
        }

        [Function(nameof(Rider_Get_Unassigned))]
        public async Task<HttpResponseData> Rider_Get_Unassigned(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var (funcName, bag, log) = await req.GetBagWithLogAsync(context);

            var riderId = context.GetRiderId();
            // Get the 'limit' and 'page' values from the DataBag
            var pageSize = 10;
            var index = 0;
            try
            {
                pageSize = bag.Get<int>(0);
                index = bag.Get<int>(1);
            }
            catch (Exception _)
            {
                return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
            }

            var rider = await RiderManager.FindByIdAsync(riderId);
            if (rider == null) return await req.WriteStringAsync("Rider not found!");

            var pg = await DoService.DoPage_GetAsync(pageSize, index, log,
                d => d.Rider == null && d.Status == 0);
            var dtoPg = pg.AdaptPageList<DeliveryOrder, DeliverOrderModel>();
            return await req.WriteBagAsync(funcName, dtoPg);
        }

        [Function(nameof(Rider_Get_Assigned))]
        public async Task<HttpResponseData> Rider_Get_Assigned(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var (funcName, bag, log) = await req.GetBagWithLogAsync(context);

            var riderId = context.GetRiderId();
            // Get the 'limit' and 'page' values from the DataBag
            var pageSize = 10;
            var index = 0;
            try
            {
                pageSize = bag.Get<int>(0);
                index = bag.Get<int>(1);
            }
            catch (Exception _)
            {
                return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
            }
            var rider = await RiderManager.FindByIdAsync(riderId);
            if (rider == null) return await req.WriteStringAsync("Rider not found!");

            var pg = await DoService.DoPage_GetAsync(pageSize, index, log,
                d => d.RiderId == riderId && d.Status > 0);
            var dtoPg = pg.AdaptPageList<DeliveryOrder, DeliverOrderModel>();
            return await req.WriteBagAsync(funcName, dtoPg);
        }

        [Function(nameof(Rider_Get_Histories))]
        public async Task<HttpResponseData> Rider_Get_Histories(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var (funcName, bag, log) = await req.GetBagWithLogAsync(context);

            var riderId = context.GetRiderId();
            // Get the 'limit' and 'page' values from the DataBag
            var pageSize = 10;
            var index = 0;
            try
            {
                pageSize = bag.Get<int>(0);
                index = bag.Get<int>(1);
            }
            catch (Exception _)
            {
                return await req.WriteStringAsync(HttpStatusCode.BadRequest, "Invalid request body.");
            }
            var rider = await RiderManager.FindByIdAsync(riderId);
            if (rider == null) return await req.WriteStringAsync("Rider not found!");

            var pg = await DoService.DoPage_GetAsync(pageSize, index, log,
                d => d.RiderId == riderId && d.Status < 0);
            var dtoPg = pg.AdaptPageList<DeliveryOrder, DeliverOrderModel>();
            return await req.WriteBagAsync(funcName, dtoPg);
        }

        [Function(nameof(Rider_Get_SubStates))]
        public async Task<HttpResponseData> Rider_Get_SubStates(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var (funcName, bag, log) = await req.GetBagWithLogAsync(context);
            return await req.WriteBagAsync(funcName, new object[] { DoStateMap.GetAllSubStates().ToArray() });
        }

        [Function(nameof(Rider_AvailableStates))]
        public async Task<HttpResponseData> Rider_AvailableStates(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var (funcName, bag, log) = await req.GetBagWithLogAsync(context);
            var subState = bag.Get<int>(0);
            var subStates = DoStateMap.GetPossibleStates(TransitionRoles.Rider, subState);
            return await req.WriteBagAsync(funcName, new object[] { subStates.Select(s => s.StateId).ToArray() });
        }

        [Function(nameof(Rider_Do_Assign))]
        public async Task<HttpResponseData> Rider_Do_Assign(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var (funcName, bag, log) = await req.GetBagWithLogAsync(context);
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                var riderId = context.GetRiderId();
                var orderId = bag.Get<long>(0);

                var rider = await RiderManager.FindByIdAsync(riderId);
                if (rider == null) return await req.WriteStringAsync("Rider not found!");
                var result = await DoService.Do_Rider_AssignAsync(orderId, rider);
                if (!result.IsSuccess)
                    return await req.WriteStringAsync(result.Message);

                SignalRCall.Update_Do_Call(orderId, log);
                return await req.WriteBagAsync(funcName, result.Data.Adapt<DeliverOrderModel>());
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error assigning delivery man.");
                return await req.WriteStringAsync("Error assigning delivery man.");
            }
        }

        [Function(nameof(Rider_Do_StateUpdate))]
        public async Task<HttpResponseData> Rider_Do_StateUpdate(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var (funcName, bag, log) = await req.GetBagWithLogAsync(context);
            var riderId = context.GetRiderId();
            var result = await Rider_Do_StateUpdate(bag, riderId, log);
            if (!result.IsSuccess) return await req.WriteStringAsync(result.Message);
            SignalRCall.Update_Do_Call(result.Data.Id, log);
            var dto = result.Data.Adapt<DeliverOrderModel>();
            return await req.WriteBagAsync(funcName, dto);
        }

        [Function(nameof(Rider_Do_Cancel))]
        public async Task<HttpResponseData> Rider_Do_Cancel(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var riderId = context.GetRiderId();
            var (functionName, bag, log) = await req.GetBagWithLogAsync(context);
            var result = await Rider_Do_StateUpdate(bag, riderId, log);
            if (!result.IsSuccess) return await req.WriteStringAsync(result.Message);

            SignalRCall.Update_Do_Call(result.Data.Id, log);
            return await req.WriteBagAsync(functionName, result.Data.Adapt<DeliverOrderModel>());
        }

        /// <summary>
        /// 更新订单状态 bag[doId,subState]
        /// </summary>
        private async Task<ResultOf<DeliveryOrder>> Rider_Do_StateUpdate(DataBag bag, long? riderId, ILogger log)
        {
            var deliveryOrderId = bag.Get<int>(0);
            var subState = bag.Get<int>(1);
            var order = await DoService.Do_FirstAsync(o => o.Id == deliveryOrderId && o.RiderId == riderId);
            if (order == null)
                return ResultOf.Fail<DeliveryOrder>("Order not found!");
            return await DoService.Do_SubState_Update(order, TransitionRoles.Rider, subState, log);
        }
    }
}