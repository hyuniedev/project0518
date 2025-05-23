using System;
using System.Collections.Generic;
using UnityEngine;

internal class Team
{
    private List<Soldier> _soliders = new List<Soldier>();
    private Action<Vector3> OnTeamMove;
    private Action<bool> OnVisibleOutline;
    private Team _opponentTeam;
    public void AddSoldier(Soldier soldier)
    {
        OnVisibleOutline += soldier.VisibleOutline;
        soldier.OnMouseTarget += VisibleOutlineAllSoldiers;
        soldier.OnDeath += RemoveSoldier;
        OnTeamMove += soldier.RequestMoveTo;
        _soliders.Add(soldier);
    }

    private void VisibleOutlineAllSoldiers(bool visible)
    {
        OnVisibleOutline?.Invoke(visible);
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

    public void UpdateOpponentTeam(Team team)
    {
        _opponentTeam = team;
    }
}