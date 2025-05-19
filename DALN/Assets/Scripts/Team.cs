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
        OnTeamMove += soldier.MoveServerRpc;
        _soliders.Add(soldier);
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
}