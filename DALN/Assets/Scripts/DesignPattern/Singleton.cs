using System;
using Controller;
using UnityEngine;

namespace DesignPattern
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject(nameof(T)).AddComponent<T>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            _instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}