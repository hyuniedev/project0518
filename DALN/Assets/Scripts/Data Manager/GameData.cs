using DesignPattern;
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
    }

}