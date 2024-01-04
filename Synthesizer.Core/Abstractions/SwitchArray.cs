namespace Synthesizer.Core.Abstractions;

/// <summary>
/// Container for two arrays, one for reading and one for writing.
/// The read and write arrays are swapped when calling <see cref="Swap"/>.
/// </summary>
/// <typeparam name="T">The type of the array items.</typeparam>
public class SwitchArray<T>
{
    private readonly List<T>[] _arrays = { new(), new() };

    private bool _switched;
    
    private int ReadIndex => _switched ? 1 : 0;
    private int WriteIndex => _switched ? 0 : 1;

    /// <summary>
    /// Gets the number of items in the array.
    /// </summary>
    /// <returns>The number of items in the array.</returns>
    public int Count => _arrays[0].Count;

    public T Read(int index) => _arrays[ReadIndex][index];

    public void Write(int index, T value) => _arrays[WriteIndex][index] = value;
    
    public void Set(int index, T item)
    {
        _arrays[WriteIndex][index] = item;
        _arrays[ReadIndex][index] = item;
    }

    /// <summary>
    /// Adds an item to both the read and write arrays.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        _arrays[ReadIndex].Add(item);
        _arrays[WriteIndex].Add(item);
    }

    /// <summary>
    /// Swaps the read and write arrays.
    /// </summary>
    public void Swap()
    {
        _switched = !_switched;
    }
}