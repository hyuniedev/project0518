using System;
using UnityEngine;

namespace Controller
{
    public class ActionEvent
    {
        public static Action OnJoinLobby;
        public static Action OnLeaveLobby;
        public static Action<Vector3> OnMove;
    }    
}
