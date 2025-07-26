using System;
using UnityEngine;

namespace DesignPattern
{
    public class ActionEvent
    {
        public static Action OnJoinLobby;
        public static Action OnLeaveLobby;
        public static Action<Vector3> OnMove;
        public static Action<int> OnChangeVolume;
    }    
}
