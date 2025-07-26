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
        public SoundType Type { get; set; }
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            ActionEvent.OnChangeVolume += ChangeVolume;
        }

        private void OnDisable()
        {
            ActionEvent.OnChangeVolume -= ChangeVolume;
        }

        private void ChangeVolume(int volume)
        {
            _audioSource.volume = volume / 100f;
        }
        
    }
}