using System;
using System.Collections.Generic;
using UnityEngine;

namespace Object
{
    public class Team
    {
        private List<Soldier> _soliders = new List<Soldier>();
        private Action<bool> OnVisibleOutline;
        public Action<Team> OnAllSoldiersOnTeamDeath;
        
        
        private Vector3[] _destinationOffsets = {Vector3.zero, new Vector3(0.5f,0,0), new Vector3(-0.5f,0,0), new Vector3(0.25f,0,-0.5f), new Vector3(-0.25f,0,-0.5f)};

        private Vector3[] _destinationOnPray =
        {
            new Vector3(0, 0, -5), new Vector3(-1, 0, -5), new Vector3(1, 0, -5), new Vector3(-2, 0, -5),
            new Vector3(2, 0, -5)
        };
        
        public void AddSoldier(Soldier soldier)
        {
            OnVisibleOutline += soldier.VisibleOutline;
            soldier.OnMouseTarget += VisibleOutlineAllSoldiers;
            soldier.OnDeath += RemoveSoldier;
            soldier.OnTargetOpponent += SetOpponentTeam;
            _soliders.Add(soldier);
        }

        private void VisibleOutlineAllSoldiers(bool visible) => OnVisibleOutline?.Invoke(visible);

        public Transform GetTransformFirstSoldier() => _soliders[0].transform;

        public int GetNumSoldiers() => _soliders.Count;
        
        private void RemoveSoldier(Soldier soldier)
        {
            soldier.OnDeath -= RemoveSoldier;
            OnVisibleOutline -= soldier.VisibleOutline;
            soldier.OnMouseTarget -= VisibleOutlineAllSoldiers;
            soldier.OnTargetOpponent -= SetOpponentTeam;
            _soliders.Remove(soldier);
            if(_soliders.Count==0)
                OnAllSoldiersOnTeamDeath?.Invoke(this);
        }

        public void TeamMoveTo(Vector3 newPosition, bool isPray = false)
        {
            for (int i = 0 ; i < _soliders.Count ; i++)
            {
                _soliders[i].RequestMoveTo(newPosition + (isPray ? _destinationOnPray[i] : _destinationOffsets[i]),isPray);
            }
        }

        public bool ContainsSoldier(Soldier soldier) => _soliders.Contains(soldier);

        public void SetOpponentTeam(ulong opponentId)
        {
            foreach (var soldier in _soliders)
            {
                soldier.SetOpponentServerRpc(opponentId);
            }
        }

        public void UpSoldiersValue(int damage, int armor)
        {
            foreach (var soldier in _soliders)
                soldier.UpSoldierDataServerRpc(damage, armor);
        }
        
        public SoldierData GetSoldierData => _soliders[0].SoldierData.Value;
    }
}