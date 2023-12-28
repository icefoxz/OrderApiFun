using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Q_DoApi.Core.Services;

namespace FunctionApp1.Funcs;

public class ImageFunc
{
    private IBlobService _blobService { get; }
    public ImageFunc(IBlobService blobService)
    {
        _blobService = blobService;
    }

    [Function(nameof(Anonymous_UploadImage))]
    public async Task<HttpResponseData> Anonymous_UploadImage(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger(nameof(Anonymous_UploadImage));

        //读取文件数据和文件类型（例如：jpg, png, gif）

        //传递文件数据和文件类型到BlobService进行上传
        string fileId;
        try
        {
            fileId = await _blobService.UploadFileAsync(req.Body);
        }
        catch (ArgumentException e)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync(e.Message);
            return badRequestResponse;
        }

        //创建响应，返回上传文件的Id
        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteStringAsync(fileId);
        return response;
    }
}