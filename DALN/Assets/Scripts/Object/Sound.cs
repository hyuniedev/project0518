using System;
using UnityEngine;

namespace Object
{
    [Serializable]
    public class Sound
    {
        public string name;
        public SoundType type;
        public AudioClip clip;
    }

    public enum SoundType
    {
        Music,
        Sfx,
    }
}