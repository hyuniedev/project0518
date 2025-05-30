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
        private Team _opponentTeam;
        public Action<Team> OnAllSoldiersOnTeamDeath;

        public void AddSoldier(Soldier soldier)
        {
            OnVisibleOutline += soldier.VisibleOutline;
            soldier.OnMouseTarget += VisibleOutlineAllSoldiers;
            soldier.OnDeath += RemoveSoldier;
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

        public void RemoveSoldier(Soldier soldier)
        {
            soldier.OnDeath -= RemoveSoldier;
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

        public void UpdateOpponentTeam(Team team)
        {
            _opponentTeam = team;
        }

        public Vector3 GetTeamPosition()
        {
            if (_soliders == null || _soliders.Count == 0) return Vector3.zero;
            Vector3 sum = Vector3.zero;
            foreach (var soldier in _soliders)
            {
                sum += soldier.transform.position;
            }

            return sum / _soliders.Count + (Vector3.forward * -20f);
        }
    }
}