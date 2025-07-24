using System.Collections.Generic;
using Object;
using UnityEngine;

namespace Data_Manager
{
    [ CreateAssetMenu(fileName = "SoundData", menuName = "Game Data/SoundData")]
    public class SoundSo : ScriptableObject
    {
        public List<Sound> sounds;
    }
}