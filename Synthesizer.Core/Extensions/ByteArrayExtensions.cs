namespace Synthesizer.Core.Extensions;

public static class ByteArrayExtensions
{
    public static string AsString(this byte[] bytes)
    {
        var chars = bytes.Select(b => (char)b).ToArray();
        return new string(chars);
    }
}