using Mapster;

namespace WebUtlLib;

public static class MapsterExtension
{
    public static IList<TDto> Adapt<TSource, TDto>(this IList<TSource> list) => list.Adapt<IList<TDto>>();

    public static PageList<TDto> AdaptPageList<TSource, TDto>(this PageList<TSource> source) where TDto : class where TSource : class =>
        PageList.Instance(source.PageIndex, source.PageSize, source.ItemCount, source.List.Adapt<List<TDto>>());
}