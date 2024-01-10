namespace Synthesizer.Core.Extensions;

public static class StringExtensions
{
    public static byte[] AsByteArray(this string str)
    {
        return str.Select(c => (byte)c).ToArray();
    }
}