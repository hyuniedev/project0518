using System.Collections;
using System.Collections.Generic;
using Data_Manager;
using DesignPattern;
using Object;
using Unity.Netcode;
using UnityEngine;

namespace Controller
{
    public class AudioController : Singleton<AudioController>
    {
        [SerializeField] private SoundSo soundSo;
        [SerializeField] private AudioSource audioSourcePrefab;
        
        public int SfxVolume { get; set; } = 100;
        public int BgmVolume { get; set; } = 100;
        
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();

        private IEnumerator Enqueue(AudioSource audioSource, float length)
        {
            yield return new WaitForSeconds(length);
            audioSource.gameObject.SetActive(false);
            audioSourcePool.Enqueue(audioSource);
        }

        public void Play(string nameAudio, Vector3 position)
        {
            AudioSource audioSource;
            Debug.Log("Called");
            if (audioSourcePool.Count == 0)
            {
                audioSource = Instantiate(audioSourcePrefab, position, Quaternion.identity);;
            }
            else
            {
                audioSource = audioSourcePool.Dequeue();
                audioSource.gameObject.SetActive(true);
            }
            Debug.Log("Instantiated");
            var sound = GetSound(nameAudio);
            Debug.Log(sound != null);
            audioSource.clip = sound.clip;
            var length = audioSource.clip.length;
            if (sound.type == SoundType.Music)
            {
                audioSource.volume = BgmVolume / 100f;
                audioSource.spatialBlend = 0f;
            }
            else
            {
                audioSource.volume = SfxVolume / 100f;
                StartCoroutine(Enqueue(audioSource, length));
                audioSource.spatialBlend = 1f;
            }
            audioSource.loop = sound.type == SoundType.Music;
            audioSource.Play();
        }
        
        private Sound GetSound(string name)
        {
            return soundSo.sounds.Find(sound => sound.name.Equals(name));
        }
    }
}