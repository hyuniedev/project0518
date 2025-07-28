using System;
using Data_Manager;
using DesignPattern;
using Object;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Controller
{
    public class SettingController : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button continueButton;
        
        private void Start()
        {
            musicSlider.onValueChanged.AddListener(MusicVolumeChanged);   
            sfxSlider.onValueChanged.AddListener(SFXVolumeChanged);
            exitButton?.onClick.AddListener(ExitButton);
            continueButton?.onClick.AddListener(ContinueButton);
            musicSlider.value = GameData.Instance.GetVolume(SoundType.Music) / 100f;
            sfxSlider.value = GameData.Instance.GetVolume(SoundType.Sfx) / 100f;
        }

        private void MusicVolumeChanged(float vol)
        {
            ActionEvent.OnChangeVolume?.Invoke(SoundType.Music, (int)(vol * 100));
        }

        private void SFXVolumeChanged(float vol)
        {
            ActionEvent.OnChangeVolume?.Invoke(SoundType.Sfx, (int)(vol * 100));
        }

        private async void ExitButton()
        {
            await LobbyController.Instance.LeaveLobby(AuthenticationService.Instance.PlayerId);
            SceneManager.LoadScene("HomeScene", LoadSceneMode.Single);
        }

        private void ContinueButton()
        {
            this.gameObject.SetActive(false);
        }
    }
}