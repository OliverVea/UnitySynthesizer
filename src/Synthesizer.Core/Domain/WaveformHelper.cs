using System.ComponentModel.DataAnnotations;

namespace Synthesizer.Core.Domain;

public static class WaveformHelper
{
    private const double TwoPi = 2 * Math.PI;
    
    public static double SampleSine([Range(0, 1)] double normalizedPhase)
    {
        return Math.Sin(TwoPi * normalizedPhase);
    }

    public static double SampleSquare([Range(0, 1)] double normalizedPhase)
    {
        return normalizedPhase < 0.5 ? 1 : 0;
    }

    public static double SampleSawtooth([Range(0, 1)] double normalizedPhase)
    {
        return -2 * normalizedPhase + 1;
    }

    public static double SampleTriangle([Range(0, 1)] double normalizedPhase)
    {
        return normalizedPhase switch
        {
            <= 0.25 => 4 * normalizedPhase,
            <= 0.75 => -4 * normalizedPhase + 2,
            _ => 4 * normalizedPhase - 4
        };
    }
}