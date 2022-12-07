namespace Cocotte.Utils;

public static class EnumerableExtensions
{
    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
        Func<TSource, TSource, TSource> conflictResolver)
    {
        var comparer = Comparer<TKey>.Default;

        var min = source.First();
        var minKey = keySelector(min);

        foreach (var element in source.Skip(1))
        {
            var key = keySelector(element);

            if (comparer.Compare(key, minKey) < 0)
            {
                min = element;
                minKey = key;
            }
            else if (comparer.Compare(key, minKey) == 0)
            {
                min = conflictResolver(min, element);
                minKey = keySelector(min);
            }
        }

        return min;
    }
}