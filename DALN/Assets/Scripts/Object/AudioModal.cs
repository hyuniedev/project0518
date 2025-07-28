using System;
using Data_Manager;
using DesignPattern;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Object
{
    public class AudioModal : MonoBehaviour
    {
        private AudioSource _audioSource;
        public SoundType Type = SoundType.Sfx;
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            ActionEvent.OnChangeVolume += ChangeVolume;
            ChangeVolume(Type, GameData.Instance.GetVolume(Type));
        }

        private void OnDisable()
        {
            ActionEvent.OnChangeVolume -= ChangeVolume;
        }

        private void ChangeVolume(SoundType type, int volume)
        {
            if (type != Type) return;
            _audioSource.volume = volume / 100f;
        }
        
    }
}