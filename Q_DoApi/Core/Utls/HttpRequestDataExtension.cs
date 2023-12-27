using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderHelperLib;
using WebUtlLib;

namespace Utls;

public static class HttpRequestDataExtension
{
    public static async Task<(string functionName, DataBag bag, ILogger log)> GetBagWithLogAsync(
        this HttpRequestData req,
        FunctionContext context,
        [CallerMemberName] string functionName = null)
    {
        var log = context.GetLogger(functionName);
        log.Event($"Function: {functionName} HttpTrigger");
        var body = await req.ReadAsStringAsync();
        log.Event($"body:\n{body}");
        var bag = DataBag.Deserialize(body);
        if (bag != null) return (functionName, bag, log);
        log.Event("bag is null");
        throw new NullReferenceException("bag is null");
    }

    public static async Task<DataBag?> GetBagAsync(this HttpRequestData req)
    {
        var body = await req.ReadAsStringAsync();
        return DataBag.Deserialize(body);
    }

    public static async Task<HttpResponseData> WriteBagAsync(this HttpRequestData req, string functionName, params object[] args)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);
        return await res.WriteBagAsync(functionName, args);
    }

    public static async Task<HttpResponseData> WriteStringAsync(this HttpRequestData req, HttpStatusCode code,
        string message)
    {
        var res = req.CreateResponse(code);
        await res.WriteStringAsync(message);
        return res;
    }

    public static async Task<HttpResponseData> WriteStringAsync(this HttpRequestData req, string message) =>
        await req.WriteStringAsync(HttpStatusCode.OK, message);
}

public static class HttpResponseDataExtension
{
    public static async Task<HttpResponseData> WriteBagAsync(this HttpResponseData res, string functionName, params object[] args)
    {
        await res.WriteStringAsync(DataBag.SerializeWithName(functionName, args));
        return res;
    }
}