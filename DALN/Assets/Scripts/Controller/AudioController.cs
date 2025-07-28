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
            if (audioSourcePool.Count == 0)
            {
                audioSource = Instantiate(audioSourcePrefab, position, Quaternion.identity);;
            }
            else
            {
                audioSource = audioSourcePool.Dequeue();
                audioSource.gameObject.SetActive(true);
            }
            var sound = GetSound(nameAudio);
            audioSource.clip = sound.clip;
            var length = audioSource.clip.length;
            if (sound.type == SoundType.Music)
            {
                audioSource.volume = GameData.Instance.GetVolume(SoundType.Music) / 100f;
                audioSource.spatialBlend = 0f;
            }
            else
            {
                audioSource.volume = GameData.Instance.GetVolume(SoundType.Sfx) / 100f;
                StartCoroutine(Enqueue(audioSource, length));
                audioSource.spatialBlend = nameAudio.Equals("Coin")||nameAudio.Equals("PrayCompleted")?0f:1f;
                audioSource.GetComponent<AudioModal>().Type = SoundType.Sfx;
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