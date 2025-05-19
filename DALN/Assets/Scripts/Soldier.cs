using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Soldier : MonoBehaviour
{
    private NavMeshAgent _agent;
    private SoldierData _soldierData;
    public Action<Soldier> OnDeath;
    private Group _opponents = null;
    private Soldier _opponent = null;
    public Group Group { get; set; }
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        ActionEvent.OnIncreaseArmor += IncreaseArmor;
        ActionEvent.OnIncreaseDamage += IncreaseDamage;
    }

    private void OnDisable()
    {
        ActionEvent.OnIncreaseArmor -= IncreaseArmor;
        ActionEvent.OnIncreaseDamage -= IncreaseDamage;
    }

    public void MoveTo(Vector3 newPosition)
    {
        if (_opponents != null)
        {
            _opponent = null;
            _opponents = null;
        }
        _agent.SetDestination(newPosition);
    }

    public void UpdateData(SoldierData soldierData)
    {
        _soldierData = soldierData;
    }

    public void IncreaseDamage(int damage)
    {
        _soldierData.Damage += damage;
    }

    public void IncreaseArmor(int armor)
    {
        _soldierData.Armor += armor;
    }

    public void GetDamage(int damage)
    {
        _soldierData.Health -= damage;
        if (_soldierData.Health <= 0)
        {
            this.Group = null;
            OnDeath?.Invoke(this);
            ObjectPool.Instance.Enqueue(EObjectPoolType.Soldier, this.gameObject);
        }
    }

    public void SetGroupOpponent(Group opponent)
    {
        _opponents = opponent;
        ChangeOpponent(this);
    }

    private void ChangeOpponent(Soldier _)
    {
        // Remove ActionEvent On Prev-Opponent
        if (_opponent != null)
            _opponent.OnDeath -= ChangeOpponent;
        
        // Get new opponent
        if (_opponents.GetCountSoldiers() == 0)
        {
            _opponents = null;
            _opponent = null;
            return;
        }
        
        _opponent = (_opponents != null && _opponents.GetCountSoldiers()>0)? _opponents.GetSoldiers()[Random.Range(0,_opponents.GetCountSoldiers())] : null;
        
        if (_opponent != null)
        {
            // Assign ActionEvent to new Opponent
            _opponent.OnDeath += ChangeOpponent;
        }
    }

    private void OnMouseEnter()
    {
        Group.OnMouseHoverGroup(true);
    }

    private void OnMouseExit()
    {
        Group.OnMouseHoverGroup(false);
    }

    private void OnMouseUp()
    {
        ActionEvent.OnSelectGroup?.Invoke(Group);
    }

    public void VisibleOutline(bool visible)
    {
        this.GetComponent<Outline>().enabled = visible;
    }
}