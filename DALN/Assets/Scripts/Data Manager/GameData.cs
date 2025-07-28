using System;
using DesignPattern;
using Object;
using UnityEngine;

namespace Data_Manager
{
    using UnityEngine;
    using Data_Manager;

    public class GameData : MonoBehaviour
    {
        public GameDataSo gameData;
        
        private static GameData _instance;
        public static GameData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameData>();
                }
                return _instance;
            }
        }

        private void Start()
        {
            ActionEvent.OnChangeVolume += SaveVolume;
        }

        private void OnDisable()
        {
            if(ActionEvent.OnChangeVolume!=null)
                ActionEvent.OnChangeVolume -= SaveVolume;       
        }

        public void SaveSessionData(bool isAnonymous)
        {
            PlayerPrefs.SetInt("IsAnonymous",isAnonymous?1:0);
            PlayerPrefs.Save();
        }
        
        public bool PreviousSessionIsAnonymous()
        {
            return PlayerPrefs.GetInt("IsAnonymous",0) == 1;
        }
        
        public void SaveVolume(SoundType type, int volume)
        {
            PlayerPrefs.SetInt(type.ToString(),volume);
            PlayerPrefs.Save();
        }
        
        public int GetVolume(SoundType type)
        {
            return PlayerPrefs.GetInt(type.ToString(),100);
        }
    }
}