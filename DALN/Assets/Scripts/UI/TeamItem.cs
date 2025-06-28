using System;
using Object;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TeamItem : MonoBehaviour
    {
        private Team _team;
        
        [SerializeField] private Text teamName;
        [SerializeField] private Text damageTxt;
        [SerializeField] private Text armorTxt;
        [SerializeField] private Button upDamageBtn;
        [SerializeField] private Button upArmorBtn;
        
        private void Update()
        {
            if (_team != null && _team.GetNumSoldiers() > 0)
            {
                damageTxt.text = _team.GetSoldierData.Damage.ToString();
                armorTxt.text = _team.GetSoldierData.Armor.ToString();
            }
        }

        public void SetUp(Team team, Action<Team> onUpDamage, Action<Team> onUpArmor)
        {
            _team = team;
            upDamageBtn.onClick.AddListener(() => onUpDamage?.Invoke(team));
            upArmorBtn.onClick.AddListener(() => onUpArmor?.Invoke(team));
        }
    }
}