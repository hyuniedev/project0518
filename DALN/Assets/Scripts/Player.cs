using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Group _freeGroup;
    private List<Group> groups;
    public Camera camera;
    private Group targetGroup;
    private void Update()
    {
        CheckPositionMouseClick();
    }

    private void CheckPositionMouseClick()
    {
        
    }

    public void UpdateTargetGroup(Group targetGroup)
    {
        this.targetGroup = targetGroup;
    }
    
    public void NewGroup(Group oldGroup, int numSoldier)
    {
        if (numSoldier < oldGroup.GetCountSoldiers()) return;
        var newGroup = ObjectPool.Instance.Dequeue(EObjectPoolType.Group).GetComponent<Group>();
        newGroup.AddSoldiers(oldGroup.CutSoldiers(numSoldier));
        groups.Add(newGroup);
    }

    public void RemoveGroup(Group group)
    {
        _freeGroup.AddSoldiers(group.CutSoldiers(group.GetCountSoldiers()));
        groups.Remove(group);
    }
}
