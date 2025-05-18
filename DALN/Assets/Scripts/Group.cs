using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    private List<Soldier> _soldiers;

    public int GetCountSoldiers()
    {
        return _soldiers.Count;
    }

    public void AddSoldiers(List<Soldier> newSoldiers)
    {
        _soldiers.AddRange(newSoldiers);
    }

    public List<Soldier> CutSoldiers(int numSoldiers)
    {
        var soldiers = _soldiers.GetRange(0, numSoldiers);
        _soldiers.RemoveRange(0, numSoldiers);
        return soldiers;
    }
    
    public void GroupMoveTo(Vector3 pos)
    {
        foreach (var sol in _soldiers)
        {
            sol.MoveTo(pos);
        }
    }
}