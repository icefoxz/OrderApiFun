using System.Runtime.CompilerServices;
using System.Text;
using DoManageWebApp.SignalHub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderDbLib;
using OrderHelperLib;
using OrderHelperLib.Contracts;
using WebUtlLib;

namespace DoManageWebApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerCallController : ControllerBase
    {
        private ILoggerFactory _logFac;
        private ServerCallService _call;
        private OrderCallService _orderCallService;
        private OrderDbContext _db;
        public ServerCallController(ServerCallService call, OrderCallService orderCallService, ILoggerFactory logFac, OrderDbContext db)
        {
            _call = call;
            _orderCallService = orderCallService;
            _logFac = logFac;
            _db = db;
        }

        [HttpPost(nameof(Call_Do_Ver))]
        public async Task<IActionResult> Call_Do_Ver([FromBody] object value)
        {
            var log = GetLog();
            try
            {
                var arg = value.ToString();
                var bag = DataBag.Deserialize(arg);
                var orderId = bag.Get<long>(0);
                await _orderCallService.CallOrderAsync(orderId);
                return Ok();
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                Console.WriteLine(e);
                throw;
            }

        }

        private ILogger GetLog([CallerMemberName] string? method = null)
        {
            var log = _logFac.CreateLogger<ServerCallController>();
            log.Event(method);
            return log;
        }

        [HttpPost(nameof(BroadCastAll))]
        public async Task<IActionResult> BroadCastAll([FromBody] object value)
        {
            var arg = value.ToString();
            var bag = DataBag.Deserialize(arg);
            var method = bag.DataName;
            var data = bag.Get<string>(0);
            await _call.BroadCastAllAsync(SignalREvents.ServerCall, DataBag.SerializeWithName(method, data));
            return Ok();
        }
        
        [HttpPost(nameof(BroadcastRiders))]
        public async Task<IActionResult> BroadcastRiders([FromBody] object value)
        {
            var arg = value.ToString();
            var bag = DataBag.Deserialize(arg);
            var method = bag.DataName;
            var data = bag.Get<string>(0);
            await _call.BroadcastRidersAsync(method, data);
            return Ok();
        }
        
        [HttpPost(nameof(BroadcastUsers))]
        public async Task<IActionResult> BroadcastUsers([FromBody] object value)
        {
            var arg = value.ToString();
            var bag = DataBag.Deserialize(arg);
            var method = bag.DataName;
            var data = bag.Get<string>(0);
            await _call.BroadCastUsersAsync(method, data);
            return Ok();
        }

        //[HttpPost(nameof(TestSendUser))]
        //public async Task<IActionResult> TestSendUser([FromBody] string value)
        //{
        //    var userId = value;
        //    var url = $"https://localhost:7191/api/ServerCall/{nameof(CallUser)}";
        //    var bag = DataBag.SerializeWithName("ServerCall", userId, nameof(TestSendUser));
        //    var isSuccess = await ApiSendAsync(url, bag);
        //    return isSuccess ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        //}
        //[HttpPost(nameof(TestSendAll))]
        //public async Task<IActionResult> TestSendAll()
        //{
        //    var url = $"https://localhost:7191/api/ServerCall/{nameof(BroadCastAll)}";
        //    var bag = DataBag.SerializeWithName("ServerCall", nameof(TestSendAll));
        //    var isSuccess = await ApiSendAsync(url, bag);
        //    return isSuccess ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        //}
        //[HttpPost(nameof(TestSendRiders))]
        //public async Task<IActionResult> TestSendRiders()
        //{
        //    var url = $"https://localhost:7191/api/ServerCall/{nameof(BroadcastRiders)}";
        //    var bag = DataBag.SerializeWithName("ServerCall", nameof(TestSendRiders));
        //    var isSuccess = await ApiSendAsync(url, bag);
        //    return isSuccess ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        //}
        
        //[HttpPost(nameof(TestSendUsers))]
        //public async Task<IActionResult> TestSendUsers()
        //{
        //    var url = $"https://localhost:7191/api/ServerCall/{nameof(BroadcastUsers)}";
        //    var bag = DataBag.SerializeWithName("ServerCall", nameof(TestSendUsers));
        //    var isSuccess = await ApiSendAsync(url, bag);
        //    return isSuccess ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        //}

        private static async Task<bool> ApiSendAsync(string url, string content)
        {
            using var httpclient = new HttpClient();
            //httpclient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            using var message = new HttpRequestMessage(HttpMethod.Post, url);
            message.Content = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await httpclient.SendAsync(message);
            return response.IsSuccessStatusCode;
        }

        [HttpGet(nameof(Test))]
        public async Task<IActionResult> Test()
        {
            var o = await _db.DeliveryOrders.FirstAsync();
            return Ok(o);
        }
    }
}
