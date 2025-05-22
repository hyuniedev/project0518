using System;
using System.Collections.Generic;
using UnityEngine;

internal class Team
{
    private List<Soldier> _soliders = new List<Soldier>();
    private Action<Vector3> OnTeamMove;
    public void AddSoldier(Soldier soldier)
    {
        soldier.OnDeath += RemoveSoldier;
        OnTeamMove += soldier.RequestMoveTo;
        _soliders.Add(soldier);
    }

    public int GetNumSoldiers()
    {
        return _soliders.Count;
    }
    public void RemoveSoldier(Soldier soldier)
    {
        soldier.OnDeath -= RemoveSoldier;
        _soliders.Remove(soldier);
    }
    public void TeamMoveTo(Vector3 newPosition)
    {
        OnTeamMove?.Invoke(newPosition);
    }

    public bool ContainsSoldier(Soldier soldier)
    {
        return _soliders.Contains(soldier);
    }
}