using System;
using System.Collections.Generic;
using UnityEngine;

namespace Object
{
    internal class Team
    {
        private List<Soldier> _soliders = new List<Soldier>();
        private Action<Vector3> OnTeamMove;
        private Action<bool> OnVisibleOutline;
        public Action<Team> OnAllSoldiersOnTeamDeath;

        public void AddSoldier(Soldier soldier)
        {
            OnVisibleOutline += soldier.VisibleOutline;
            soldier.OnMouseTarget += VisibleOutlineAllSoldiers;
            soldier.OnDeath += RemoveSoldier;
            soldier.OnTargetOpponent += SetOpponentTeam;
            // soldier.SetStoppingDistance(0.5f*_soliders.Count);
            OnTeamMove += soldier.RequestMoveTo;
            _soliders.Add(soldier);
        }

        private void VisibleOutlineAllSoldiers(bool visible)
        {
            OnVisibleOutline?.Invoke(visible);
        }

        public Transform GetTransformFirstSoldier()
        {
            return _soliders[0].transform;
        }

        public int GetNumSoldiers()
        {
            return _soliders.Count;
        }

        private void RemoveSoldier(Soldier soldier)
        {
            soldier.OnDeath -= RemoveSoldier;
            OnVisibleOutline -= soldier.VisibleOutline;
            soldier.OnMouseTarget -= VisibleOutlineAllSoldiers;
            OnTeamMove -= soldier.RequestMoveTo;
            _soliders.Remove(soldier);
            OnAllSoldiersOnTeamDeath?.Invoke(this);
        }

        public void TeamMoveTo(Vector3 newPosition)
        {
            OnTeamMove?.Invoke(newPosition);
        }

        public bool ContainsSoldier(Soldier soldier)
        {
            return _soliders.Contains(soldier);
        }

        private void SetOpponentTeam(ulong opponentId)
        {
            foreach (var soldier in _soliders)
            {
                soldier.SetOpponentServerRpc(opponentId);
            }
        }
    }
}