using System.Collections.Generic;
using UnityEngine;

namespace Data_Manager
{
    public class GameData
    {
        public static int InitHeath = 100;
        public static int InitDamage = 20;
        public static int InitArmor = 0;
        public static int InitCountSoldierPerPlayer = 5;
        public static int FeeIncreaseDamage = 5;
        public static int FeeIncreaseArmor = 5;
        public static int MaxPlayersPerLobby = 3;
        public static Dictionary<int, Vector3> TeamInitialPosition = new Dictionary<int, Vector3>()
        {
            { 1, new Vector3(53.5f, 9.5f, 72.5f) },
            { 2, new Vector3(-85.5f, 2, 22.5f) },
            { 3, new Vector3(17.5f, 1.5f, -84.3f) }
        };
    }
}