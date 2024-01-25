namespace Tests.Shared;

public static class Signals
{
    public static IEnumerable<double> Impulse()
    {
        yield return 1;
        while (true) yield return 0;
    }
    
    public static IEnumerable<double> Step()
    {
        yield return 0;
        while (true) yield return 1;
    }
}