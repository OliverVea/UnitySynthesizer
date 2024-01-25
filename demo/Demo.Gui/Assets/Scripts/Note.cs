using System;

namespace Demo.Gui
{
    public enum Note
    {
        C,
        CSharp,
        D,
        DSharp,
        E,
        F,
        FSharp,
        G,
        GSharp,
        A,
        ASharp,
        B
    }

    public static class NoteExtensions
    {
        public static double GetFrequency(this Note note, int octave = 4)
        {
            var baseFrequency = GetBaseFrequency(note);
            var frequency = baseFrequency * Math.Pow(2, octave - 4);

            return frequency;
        }
    
        private static double GetBaseFrequency(Note note)
        {
            return note switch
            {
                Note.C => 261.63,
                Note.CSharp => 277.18,
                Note.D => 293.66,
                Note.DSharp => 311.13,
                Note.E => 329.63,
                Note.F => 349.23,
                Note.FSharp => 369.99,
                Note.G => 392.00,
                Note.GSharp => 415.30,
                Note.A => 440.00,
                Note.ASharp => 466.16,
                Note.B => 493.88,
                _ => throw new ArgumentOutOfRangeException(nameof(note), note, null)
            };
        }
    }
}