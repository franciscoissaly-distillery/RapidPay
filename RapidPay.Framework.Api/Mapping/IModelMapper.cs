namespace RapidPay.Framework.Api.Mapping
{
    public interface IModelMapper
    {
        TTarget MapTo<TTarget, TSource>(TSource source);
        TTarget MapTo<TTarget, TSource>(TTarget target, TSource source);
        TTarget MapTo<TTarget, TSource>(Func<TSource, TTarget> factory, TSource source);

        IList<TTarget> MapTo<TTarget, TSource>(IEnumerable<TSource> source);
        IList<TTarget> MapTo<TTarget, TSource>(IList<TTarget> target, IEnumerable<TSource> source);
        IList<TTarget> MapTo<TTarget, TSource>(Func<TSource, TTarget> factory, IEnumerable<TSource> source);
        IList<TTarget> MapTo<TTarget, TSource>(Func<TSource, TTarget> factory, IList<TTarget> target, IEnumerable<TSource> source);
    }
}