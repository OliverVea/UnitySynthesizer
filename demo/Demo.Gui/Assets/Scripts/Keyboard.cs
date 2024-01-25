#nullable enable

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Demo.Gui
{
    public class Keyboard : MonoBehaviour
    {
        private KeyBinding[] _keyBindings = Array.Empty<KeyBinding>();
    
        private const KeyCode OctaveUpKey = KeyCode.UpArrow;
        private const KeyCode OctaveDownKey = KeyCode.DownArrow;
    
        private const KeyCode OctaveUpModifierKey = KeyCode.LeftShift;
        private const KeyCode OctaveDownModifierKey = KeyCode.LeftControl;
    
        private TMP_Text _octaveText = null!;

        private static readonly Color32 NormalColor = new(255, 255, 255, 255);
        private static readonly Color32 PressedColor = new(0, 122, 204, 255);
    
        private int _octave = 5;

        private readonly Dictionary<KeyCode, int> _keyVoiceLookup = new();

        private void Awake()
        {
            _keyBindings = new KeyBinding[]
            {
                new() { Key = KeyCode.A, GameObject = GameObject.Find("A"), Note = Note.C },
                new() { Key = KeyCode.W, GameObject = GameObject.Find("W"), Note = Note.CSharp },
                new() { Key = KeyCode.S, GameObject = GameObject.Find("S"), Note = Note.D },
                new() { Key = KeyCode.E, GameObject = GameObject.Find("E"), Note = Note.DSharp },
                new() { Key = KeyCode.D, GameObject = GameObject.Find("D"), Note = Note.E },
                new() { Key = KeyCode.F, GameObject = GameObject.Find("F"), Note = Note.F },
                new() { Key = KeyCode.T, GameObject = GameObject.Find("T"), Note = Note.FSharp },
                new() { Key = KeyCode.G, GameObject = GameObject.Find("G"), Note = Note.G },
                new() { Key = KeyCode.Y, GameObject = GameObject.Find("Y"), Note = Note.GSharp },
                new() { Key = KeyCode.H, GameObject = GameObject.Find("H"), Note = Note.A },
                new() { Key = KeyCode.U, GameObject = GameObject.Find("U"), Note = Note.ASharp },
                new() { Key = KeyCode.J, GameObject = GameObject.Find("J"), Note = Note.B },
                new() { Key = KeyCode.K, GameObject = GameObject.Find("K"), Note = Note.C, OctaveShift = 1},
                new() { Key = KeyCode.O, GameObject = GameObject.Find("O"), Note = Note.CSharp, OctaveShift = 1},
                new() { Key = KeyCode.L, GameObject = GameObject.Find("L"), Note = Note.D, OctaveShift = 1},
            };

            _octaveText = GameObject.Find("OctaveText").GetComponentInChildren<TMP_Text>();
        }

        private void Update()
        {
            foreach (var keyBinding in _keyBindings) HandleKeyBinding(keyBinding);
        
            if (Input.GetKeyDown(OctaveUpKey)) SetOctave(_octave + 1);
            if (Input.GetKeyDown(OctaveDownKey)) SetOctave(_octave - 1);
        }
    
        public void SetOctave(int octave)
        {
            _octave = octave;
            _octaveText.text = _octave.ToString();
        }

        public int GetOctave()
        {
            var octave = _octave;
        
            if (Input.GetKey(OctaveUpModifierKey)) octave += 1;
            if (Input.GetKey(OctaveDownModifierKey)) octave -= 1;
        
            return octave;
        }

        private void HandleKeyBinding(KeyBinding keyBinding)
        {
            if (Input.GetKeyDown(keyBinding.Key)) HandleKeyDown(keyBinding);
            if (Input.GetKeyUp(keyBinding.Key)) HandleKeyUp(keyBinding);
        }

        private void HandleKeyUp(KeyBinding keyBinding)
        {
            keyBinding.Text.faceColor = NormalColor;
            keyBinding.Outline.enabled = false;

            if (!_keyVoiceLookup.Remove(keyBinding.Key, out var voiceIndex)) return;
            VoiceManager.StopVoice(voiceIndex);
        }

        private void HandleKeyDown(KeyBinding keyBinding)
        {
            keyBinding.Text.faceColor = PressedColor;
            keyBinding.Outline.enabled = true;

            var octave = GetOctave() + keyBinding.OctaveShift;
            var frequency = keyBinding.Note.GetFrequency(octave);

            var voiceIndex = VoiceManager.PlayVoice(frequency);
            if (!voiceIndex.HasValue) return;

            _keyVoiceLookup[keyBinding.Key] = voiceIndex.Value;
        }


        [Serializable]
        private class KeyBinding
        {
            public KeyCode Key { get; set; }
            public GameObject GameObject { get; set; }
            public Note Note { get; set; }
            public int OctaveShift { get; set; }
            

            private Lazy<TMP_Text> _text;
            public TMP_Text Text => _text.Value;
        
            private Lazy<Outline> _outline;
            public Outline Outline => _outline.Value;

            public KeyBinding()
            {
                _text = new Lazy<TMP_Text>(() => GameObject?.GetComponentInChildren<TMP_Text>()!);
                _outline = new Lazy<Outline>(() => GameObject?.GetComponentInChildren<Outline>()!);
            }
        }
    }
}