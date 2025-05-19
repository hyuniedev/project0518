using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Team : NetworkBehaviour
{
    #region Setup Singleton
    private static Team _instance;

    public static Team Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("Team").AddComponent<Team>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (_instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    #endregion
    private List<Soldier> _freeGroup = new List<Soldier>();
    private List<Group> _groups = new List<Group>();
    public Group TargetGroup { get; set; }
    public Group CurrentGroup { get; set; }

    private int _feeUpdateArmor = 5;
    private int _feeUpdateDamage = 5;
    public int Cost { get; set; }

    private void OnEnable()
    {
        ActionEvent.OnSelectGroup += VerifyTargetingGroup;
    }

    private void OnDisable()
    {
        ActionEvent.OnSelectGroup -= VerifyTargetingGroup;
    }

    private void Start()
    {
        CreateNewSoldier();
        CreateNewSoldier();
        CreateNewGroup(2);
        CurrentGroup = _groups[0];
    }

    private void VerifyTargetingGroup(Group group)
    {
        if (_groups.Contains(group))
        {
            CurrentGroup = group;
        }
        else
        {
            TargetGroup = group;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Mouse Down");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                CurrentGroup.GroupMoveTo(hit.point);
            }
        }
    }

    public void CreateNewGroup(int numSoldier)
    {
        // var newGroup = ObjectPool.Instance.Dequeue(EObjectPoolType.Group).GetComponent<Group>();
        Group newGroup = new Group();
        newGroup.AddSoldiers(_freeGroup.GetRange(0, numSoldier));
        _freeGroup.RemoveRange(0, numSoldier);
        _groups.Add(newGroup);
    }

    private void CreateNewSoldier()
    {
        var newSoldier = ObjectPool.Instance.Dequeue(EObjectPoolType.Soldier).GetComponent<Soldier>();
        Debug.Log(newSoldier.name);
        _freeGroup.Add(newSoldier);
    }

    public void BtnIncreaseDamageOnClick()
    {
        if (Cost > _feeUpdateDamage)
        {
            Cost -= _feeUpdateDamage;
            ActionEvent.OnIncreaseDamage?.Invoke(GameData.DamagePerIncrease);
        }
    }

    public void BtnIncreaseArmorOnClick()
    {
        if (Cost > _feeUpdateArmor)
        {
            Cost -= _feeUpdateArmor;
            ActionEvent.OnIncreaseArmor?.Invoke(GameData.ArmorPerIncrease);
        }
    }
}