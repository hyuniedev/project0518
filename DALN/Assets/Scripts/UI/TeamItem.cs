using System;
using Data_Manager;
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
        [SerializeField] private Text feeUpDamageTxt;
        [SerializeField] private Text feeUpArmorTxt;
        
        private void Update()
        {
            if (_team != null && _team.GetNumSoldiers() > 0)
            {
                damageTxt.text = _team.GetSoldierData.Damage.ToString();
                armorTxt.text = _team.GetSoldierData.Armor.ToString();
                feeUpDamageTxt.text = $"-{NextFreeIncrementDamage(_team.GetSoldierData.Damage)}$";
                feeUpArmorTxt.text = $"-{NextFreeIncrementArmor(_team.GetSoldierData.Armor)}$";
            }
        }

        public void SetUp(Team team,string nameTeam ,Action<Team> onUpDamage, Action<Team> onUpArmor)
        {
            _team = team;
            upDamageBtn.onClick.AddListener(() => onUpDamage?.Invoke(team));
            upArmorBtn.onClick.AddListener(() => onUpArmor?.Invoke(team));
            teamName.text = nameTeam;
        }

        private int NextFreeIncrementDamage(int curDamage)
        {
            var maxDamage = GameData.Instance.gameData.initDamage + GameData.Instance.gameData.valuePerIncreaseDamage *
                GameData.Instance.gameData.maxTimeOfIncreaseDamage;
            if (curDamage == maxDamage) return 0;
            return GameData.Instance.gameData.feeInitIncreaseDamage + (curDamage - GameData.Instance.gameData.initDamage) /
                GameData.Instance.gameData.valuePerIncreaseDamage *
                GameData.Instance.gameData.rangeFeePerIncreaseDamage;
        }

        private int NextFreeIncrementArmor(int curArmor)
        {
            var maxArmor = GameData.Instance.gameData.initArmor + GameData.Instance.gameData.valuePerIncreaseArmor *
                GameData.Instance.gameData.maxTimeOfIncreaseArmor;
            if (curArmor == maxArmor) return 0;
            return GameData.Instance.gameData.feeInitIncreaseArmor + (curArmor - GameData.Instance.gameData.initArmor) /
                GameData.Instance.gameData.valuePerIncreaseArmor *
                GameData.Instance.gameData.rangeFeePerIncreaseArmor;
        }
    }
}