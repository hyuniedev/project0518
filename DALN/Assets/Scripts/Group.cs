using UnityEngine;

public class Group : MonoBehaviour
{
    [SerializeField] private Soldier[] _soldiers;

    public void GroupMoveTo(Vector3 pos)
    {
        foreach (var sol in _soldiers)
        {
            sol.MoveTo(pos);
        }
    }
}