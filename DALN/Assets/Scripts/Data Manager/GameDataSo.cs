using System.Collections.Generic;

namespace Data_Manager
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "GameData", menuName = "Game Data/GameData")]
    public class GameDataSo : ScriptableObject
    {
        public int initCountSoldierPerTeam = 5;
        public int initHealth = 100;
        
        public int initDamage = 20;
        public int feeInitIncreaseDamage = 5;
        public int rangeFeePerIncreaseDamage = 5;
        public int maxTimeOfIncreaseDamage = 5;
        public int valuePerIncreaseDamage = 5;
        
        public int initArmor = 0;
        public int feeInitIncreaseArmor = 5;
        public int rangeFeePerIncreaseArmor = 5;
        public int maxTimeOfIncreaseArmor = 5;
        public int valuePerIncreaseArmor = 5;
        
        public float initSpawnCostTime = 1;
        public int feeInitIncreaseSpawnCostTime = 20;
        public int rangeFeePerIncreaseSpawnCostTime = 5;
        public int maxTimeOfIncreaseSpawnCostTime = 5;
        public float valuePerChangeSpawnCostTime = -0.1f;
        
        public int feeCreateSoldierTeam = 30;
        public int maxPlayersPerLobby = 3;

        public List<Texture> soliderTextures = new List<Texture>();

        public Dictionary<int, Vector3> TeamInitialPosition = new Dictionary<int, Vector3>()
        {
            { 1, new Vector3(53.5f, 9.5f, 72.5f) },
            { 2, new Vector3(-85.5f, 2, 22.5f) },
            { 3, new Vector3(17.5f, 1.5f, -84.3f) }
        };
    }

}