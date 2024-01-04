namespace Synthesizer.Core.Abstractions;

public static class DictionaryExtensions
{
    public static TValue SetDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        if (dictionary.TryGetValue(key, out var value)) return value;
        dictionary.Add(key, defaultValue);
        return defaultValue;
    }
}