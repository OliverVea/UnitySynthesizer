namespace Synthesizer.Core;

public class CircularBuffer<T>(int size)
{
    private readonly T[] _buffer = new T[size];
    private int _currentIndex;

    public void Write(T value)
    {
        _buffer[_currentIndex] = value;
        _currentIndex = (_currentIndex + 1) % size;
    }

    public T Read(int offset)
    {
        var readIndex = (_currentIndex - 1 - offset + size) % size;
        return _buffer[readIndex];
    }
}