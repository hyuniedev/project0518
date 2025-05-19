using System;
using System.Collections.Generic;
using UnityEngine;

public class Group
{
    private List<Soldier> _soldiers = new List<Soldier>();
    private Action<bool> OnMouseEvent;

    public int GetCountSoldiers()
    {
        return _soldiers.Count;
    }

    public List<Soldier> GetSoldiers()
    {
        return _soldiers;
    }

    public void AddSoldiers(List<Soldier> newSoldiers)
    {
        foreach (var soldier in newSoldiers)
        {
            OnMouseEvent += soldier.VisibleOutline;
            soldier.OnDeath += RemoveSoldier;
            soldier.Group = this;
            _soldiers.Add(soldier);
        }
    }

    // Remove Soldier on Death State
    private void RemoveSoldier(Soldier soldier)
    {
        if (_soldiers.Contains(soldier))
        {
            soldier.OnDeath -= RemoveSoldier;
            _soldiers.Remove(soldier);
        }
        if (_soldiers.Count == 0)
        {
            // ObjectPool.Instance.Enqueue(EObjectPoolType.Group, gameObject);
        }
    }

    public void OnMouseHoverGroup(bool visible)
    {
        OnMouseEvent?.Invoke(visible);
    }
    
    public void GroupMoveTo(Vector3 pos)
    {
        foreach (var sol in _soldiers)
        {
            sol.MoveTo(pos);
        }
    }
}