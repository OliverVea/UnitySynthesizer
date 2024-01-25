namespace Synthesizer.Core.Domain;

public abstract class IIR(int order, SignalBus signalBus) : SynthesizerModule
{
    private readonly SignalBus _signalBus = signalBus;
    
    private readonly CircularBuffer<double> _x = new(order + 1);
    private readonly CircularBuffer<double> _y = new(order + 1);
    
    private readonly double[] _a = new double[order + 1];
    private readonly double[] _b = new double[order + 1];
    
    /// <summary>If the coefficients should be recalculated.</summary>
    protected abstract bool ShouldUpdateCoefficients();
    
    /// <summary>Gets the coefficients for the filter.</summary>
    /// <remarks>y_0 = a_0 * (sum(x_i * b_i) + sum(y_i * -a_i))</remarks>
    /// <returns>The array of a and then b coefficients.</returns>
    protected abstract void UpdateCoefficients(double[] a, double[] b);
    
    public SignalId? SignalInput { get; set; }
    public SignalId? SignalOutput { get; set; }

    public override void Update(SynthesizerUpdateContext context)
    {
        if (ShouldUpdateCoefficients())
        {
            UpdateCoefficients(_a, _b);
        }
    
        _x.Write(_signalBus.ReadSignalValue(SignalInput));
    
        double a = 0, b = 0;
    
        for (var i = 0; i < order; i++)
        {
            b += _b[i] * _x.Read(i);

            if (i > 0)
            {
                a += -_a[i] * _y.Read(i);
            }
        }

        var output = (a + b) * _a[0];
        _y.Write(output);
        _signalBus.WriteSignalValue(SignalOutput, output);
    }
}