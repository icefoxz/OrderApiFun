using Mapster;
using Microsoft.Extensions.Logging;
using OrderHelperLib.Dtos;

namespace WebUtlLib;

public static class MapsterExtension
{
    public static IList<TDto> Adapt<TSource, TDto>(this IList<TSource> list) => list.Adapt<IList<TDto>>();

    public static PageList<TDto> AdaptPageList<TSource, TDto>(this PageList<TSource> source,ILogger log) where TDto : class where TSource : class
    {
        var result = source.AdaptPageList<TSource, TDto>();
        log.Event($"Adapted from {typeof(TSource).Name} to {typeof(TDto).Name}");
        return result;
    }

    private static PageList<TDto> AdaptPageList<TSource, TDto>(this PageList<TSource> source) where TDto : class where TSource : class =>
        PageList.Instance(source.PageIndex, source.PageSize, source.ItemCount, source.List.Adapt<List<TDto>>());
}