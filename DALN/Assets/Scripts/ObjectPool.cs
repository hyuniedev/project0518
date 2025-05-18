using System;
using System.Collections.Generic;
using UnityEngine;

public enum EObjectPoolType
{
    Group,
    Bullet,
    Soldier,
    Sound,
}
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private GameObject soundPrefab;
    [SerializeField] private GameObject groupPrefab;
    
    private static ObjectPool _instance;

    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("ObjectPool").AddComponent<ObjectPool>();
            }
            return _instance;
        }
    }
    
    private Queue<GameObject> _groupQueue = new Queue<GameObject>();
    private Queue<GameObject> _soldiersQueue = new Queue<GameObject>();
    private Queue<GameObject> _poolSound = new Queue<GameObject>();
    private Queue<GameObject> _poolBullets = new Queue<GameObject>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        if (_instance != this)
        {
            Destroy(this);
        }
        
        DontDestroyOnLoad(this);
    }

    public void Enqueue(EObjectPoolType type, GameObject obj)
    {
        obj.SetActive(false);
        switch (type)
        {
            case EObjectPoolType.Group:
                _groupQueue.Enqueue(obj);
                break;
            case EObjectPoolType.Soldier:
                _soldiersQueue.Enqueue(obj);
                break;
            case EObjectPoolType.Sound:
                _poolSound.Enqueue(obj);
                break;
            case EObjectPoolType.Bullet:
                _poolBullets.Enqueue(obj);
                break;
        }
    }

    public GameObject Dequeue(EObjectPoolType type)
    {
        GameObject obj;
        switch (type)
        {
            case EObjectPoolType.Group:
                obj = _groupQueue.Count>0?_groupQueue.Dequeue():Instantiate(groupPrefab);
                break;
            case EObjectPoolType.Soldier:
                obj = _soldiersQueue.Count>0?_soldiersQueue.Dequeue():Instantiate(soldierPrefab);
                break;
            case EObjectPoolType.Sound:
                obj = _poolSound.Count>0?_poolSound.Dequeue():Instantiate(soundPrefab);
                break;
            case EObjectPoolType.Bullet:
                obj = _poolBullets.Count>0?_poolBullets.Dequeue():Instantiate(soundPrefab);
                break;
            default:
                obj = new GameObject();
                break;
        }
        obj.SetActive(true);
        return obj;
    }
}