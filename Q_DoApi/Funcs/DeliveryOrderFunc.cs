using System.Net;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderDbLib.Entities;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using OrderHelperLib.Dtos.DeliveryOrders;
using OrderHelperLib.Dtos.Lingaus;
using Q_DoApi.DtoMapping;
using Utls;

namespace Do_Api.Funcs
{
    public class DeliveryOrderFunc
    {
        private LingauManager LingauManager { get; }
        private DeliveryOrderService DoService { get; }
        private RiderManager DmManager { get; }
        private UserManager<User> UserManager { get; }

        public DeliveryOrderFunc(DeliveryOrderService doService, UserManager<User> userManager, RiderManager dmManager, LingauManager lingauManager)
        {
            DoService = doService;
            UserManager = userManager;
            DmManager = dmManager;
            LingauManager = lingauManager;
        }

        [Function(nameof(User_CreateDeliveryOrder))]
        public async Task<HttpResponseData> User_CreateDeliveryOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger(nameof(User_CreateDeliveryOrder));
            var bag = await req.GetBagAsync();
            //test Instance:
            //var dto = InstanceTestDeliverDto();
            //log.LogWarning(Json.Serialize(dto));
            //throw new NotImplementedException();
            log.LogInformation("C# HTTP trigger function processed a request.");
            var userId = context.Items[Auth.UserId].ToString();
            // Deserialize the request body to DeliveryOrder
            DeliverOrderModel? orderDto = null;
            try
            {
                orderDto = bag.Get<DeliverOrderModel>(0);
            }
            catch (Exception e)
            {
                log.LogWarning($"Invalid request body.\n{e}");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body.");
                return badRequestResponse;
            }

            if (!MyPhone.VerifyPhoneNumber(orderDto.SenderInfo.PhoneNumber))
            {
                log.LogWarning("Invalid sender phone number.");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid sender phone number.");
                return badRequestResponse;
            }

            if (!MyPhone.VerifyPhoneNumber(orderDto.ReceiverInfo.PhoneNumber))
            {
                log.LogWarning("Invalid receiver phone number.");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid receiver phone number.");
                return badRequestResponse;
            }

            // Add the new order to the database using the DeliveryOrderService
            var newDo = await DoService.CreateDeliveryOrderAsync(userId, orderDto, log);
            var createdResponse = req.CreateResponse(HttpStatusCode.Created);
            //createdResponse.Headers.Add("Location", $"deliveryorder/{newOrder.Id}");
            await createdResponse.WriteStringAsync(DataBag.Serialize(newDo.Adapt<DeliverOrderModel>()));
            return createdResponse;
        }

        [Function(nameof(User_GetAllDeliveryOrders))]
        public async Task<HttpResponseData> User_GetAllDeliveryOrders(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger(nameof(User_GetAllDeliveryOrders));
            log.LogInformation("C# HTTP trigger function processed a request.");

            var userId = context.Items[Auth.UserId].ToString();

            // Retrieve DataBag from request
            var bag = await req.GetBagAsync();

            // Get the 'limit' and 'page' values from the DataBag
            var limit = 10;
            var page = 0;

            try
            {
                limit = bag.Get<int>(0);
                page = bag.Get<int>(1);
            }
            catch (Exception _)
            {
                // ignored
            }

            // Retrieve paginated DeliveryOrders for the user from the database using the DeliveryOrderService
            var deliveryOrders = await DoService.User_GetDeliveryOrdersAsync(userId, limit, page, log);

            // Convert the DeliveryOrders to a list of DeliveryOrderDto objects
            var deliveryOrdersDto = deliveryOrders.Select(order => order.Adapt<DeliverOrderModel>(EntityMapper.Config)).ToList();

            // Create an HTTP 200 (OK) response and write the DeliveryOrderDto objects as JSON
            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteStringAsync(DataBag.SerializeWithName(nameof(deliveryOrdersDto.GetType), deliveryOrdersDto));
            return okResponse;
        }
        
        [Function(nameof(Rider_GetAllDeliveryOrders))]
        public async Task<HttpResponseData> Rider_GetAllDeliveryOrders(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger(nameof(Rider_GetAllDeliveryOrders));
            log.LogInformation("C# HTTP trigger function processed a request.");

            var riderId = GetRiderId(context);

            // Retrieve DataBag from request
            var bag = await req.GetBagAsync();

            // Get the 'limit' and 'page' values from the DataBag
            var limit = 10;
            var page = 0;

            try
            {
                limit = bag.Get<int>(0);
                page = bag.Get<int>(1);
            }
            catch (Exception _)
            {
                // ignored
            }

            // Retrieve paginated DeliveryOrders for the user from the database using the DeliveryOrderService
            var deliveryOrders = await DoService.Rider_GetDeliveryOrdersAsync(riderId, limit, page, log);

            // Convert the DeliveryOrders to a list of DeliveryOrderDto objects
            var deliveryOrdersDto = deliveryOrders.Select(order => order.Adapt<DeliverOrderModel>()).ToList();

            // Create an HTTP 200 (OK) response and write the DeliveryOrderDto objects as JSON
            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteStringAsync(DataBag.SerializeWithName(nameof(deliveryOrdersDto.GetType), deliveryOrdersDto));
            return okResponse;
        }

        [Function(nameof(User_PayDeliveryOrderByCredit))]
        public async Task<HttpResponseData> User_PayDeliveryOrderByCredit(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger(nameof(User_PayDeliveryOrderByCredit));
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Retrieve the request body
            var userId = context.Items[Auth.UserId].ToString();
            DeliveryOrder? order;
            try
            {
                var bag = await req.GetBagAsync();
                var deliveryOrderId = bag.Get<int>(0);
                order = await DoService.User_GetDeliveryOrderAsync(userId, deliveryOrderId);
            }
            catch (Exception e)
            {
                log.LogWarning($"Invalid request body.\n{e}");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body.");
                return badRequestResponse;
            }

            var userLingau = await LingauManager.GetLingauAsync(userId);
            if (order.PaymentInfo.Charge > userLingau.Credit)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Not enough Lingau.");
                return badRequestResponse;
            }

            await DoService.PayDeliveryOrderByLingauAsync(userId, order, log);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(DataBag.Serialize(userLingau.Adapt<LingauModel>()));
            return response;
        }

        [Function(nameof(Rider_AssignRider))]
        public async Task<HttpResponseData> Rider_AssignRider(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger(nameof(Rider_AssignRider));
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                var deliveryManId = GetRiderId(context);

                var bag = await req.GetBagAsync();
                var oId = bag.Get<string>(0);
                var orderId = int.Parse(oId);

                if (bag == null)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Invalid request payload.");
                    return errorResponse;
                }

                //assign deliveryMan
                var order = await DoService.AssignRiderAsync(orderId, deliveryManId);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(DataBag.Serialize(order.Adapt<DeliverOrderModel>()));
                return response;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error assigning delivery man.");

                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error assigning delivery man.");
                return errorResponse;
            }
        }
        //从context中获取当前的DeliveryManId
        private static int GetRiderId(FunctionContext context)
        {
            if (!int.TryParse(context.Items[Auth.RiderId].ToString(), out var deliveryManId))
                throw new NullReferenceException($"DeliveryMan[{deliveryManId}] not found!");
            return deliveryManId;
        }

        [Function(nameof(Rider_UpdateOrderStatus))]
        public async Task<HttpResponseData> Rider_UpdateOrderStatus(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger(nameof(Rider_UpdateOrderStatus));
            log.LogInformation("C# HTTP trigger function processed a request.");
            var deliveryManId = GetRiderId(context);
            var message = string.Empty;
            var bag = await req.GetBagAsync() ?? throw new NullReferenceException("Invalid databag format");
            var riderId = GetRiderId(context);
            var orderId = bag.Get<int>(0);
            var status = bag.Get<int>(1);
            try
            {
                var deliveryOrder = await DoService.Rider_GetDeliveryOrderAsync(riderId, orderId, log) ??
                                    throw new NullReferenceException($"Order[{orderId}] not found!");
                deliveryOrder.Status = status;
                message = $"Order[{deliveryOrder.Id}].Update({status})";

                await DoService.UpdateOrderStatusByRiderAsync(deliveryManId, orderId, (DeliveryOrderStatus)status);
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(DataBag.Serialize(deliveryOrder.Adapt<DeliverOrderModel>()));
                return response;
            }
            catch (Exception ex)
            {
                message = $"Error {message}!";
                log.LogError(ex, message);

                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error setting order status to {status}.");
                return errorResponse;
            }
        }
    }
}
