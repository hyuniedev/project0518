using System;
using System.Collections;
using Controller;
using Data_Manager;
using Object;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameUI : NetworkBehaviour
    {
        private static readonly int InAnim = Animator.StringToHash("In");
        public Player Player { get; set; } = null;
        [SerializeField] private Text timeDownTxt;
        [SerializeField] private Text costTxt;
        [SerializeField] private Button listTeamButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Transform listTeamsParent;
        [SerializeField] private GameObject itemTeamPrefab;
        [SerializeField] private GameObject addTeamButtonPrefab;
        [SerializeField] private GameObject listTeamPanel;
        [SerializeField] private GameObject gameOverPanel;

        private NetworkVariable<int> _timeDown { get; set; }
        private int _timeDownLocal;
        private int _currentCost = 0;
        private float _spawnCostTime;
        public bool IsOpenPanel = false;

        public void Awake()
        {
            _timeDown = new NetworkVariable<int>(GameData.Instance.gameData.timeMatch, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
        }

        private void Start()
        {
            if (IsServer)
            {
                StartCoroutine(TimeDown());
            }

            if (IsClient)
            {
                gameOverPanel.SetActive(false);
                _spawnCostTime = GameData.Instance.gameData.initSpawnCostTime;
                StartCoroutine(IncreaseCostPerSeconds());
                listTeamButton.onClick.AddListener(UpdateStateListTeamPanel);
            }
        }

        private void Update()
        {
            if (IsClient)
            {
                if (Input.GetKeyDown(KeyCode.Tab)) UpdateStateListTeamPanel();
                
                TimeDownUpdate();

                if (_timeDown.Value <= 0)
                {
                    StopAllCoroutines();
                    gameOverPanel.SetActive(true);
                    RequestDespawnSoldiersServerRpc();
                    this.enabled = false;
                }
                
                if (Player == null) return;
                if (Player.Teams.Count != listTeamsParent.childCount-1)
                {
                    foreach (Transform item in listTeamsParent)
                        Destroy(item.gameObject);
                    int index = 0;
                    foreach (var team in Player.Teams)
                    {
                        index++;
                        var itemTeam = Instantiate(itemTeamPrefab, listTeamsParent);
                        itemTeam.GetComponent<TeamItem>().SetUp(team, $"Team {index}" ,IncreaseDamage, IncreaseArmor);
                        itemTeam.GetComponent<Button>().onClick.AddListener(() => { Player.SelectedTeam = team;});
                    }
                    if (Player.Teams.Count < 10)
                    {
                        var addTeamButton = Instantiate(addTeamButtonPrefab, listTeamsParent);
                        addTeamButton.GetComponent<Button>().onClick.AddListener(CreateNewTeam);
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDespawnSoldiersServerRpc()
        {
            var soliders = FindObjectsByType<Soldier>(FindObjectsSortMode.None);
            foreach (var soldier in soliders)
                soldier.GetComponent<NetworkObject>().Despawn();
        }

        public void UpdateStateListTeamPanel()
        {
            IsOpenPanel = !IsOpenPanel;
            listTeamPanel.GetComponent<Animator>().SetBool(InAnim,IsOpenPanel);
        }
        
        #region Update propety value

        public void IncreaseCostSpeed()
        {
            if(!IsClient) return;
            var maxDecTime = GameData.Instance.gameData.initSpawnCostTime +
                             GameData.Instance.gameData.valuePerChangeSpawnCostTime *
                             GameData.Instance.gameData.maxTimeOfIncreaseSpawnCostTime;
            if (_spawnCostTime - maxDecTime < 0.1f) return;
            var feeUpgrade = GameData.Instance.gameData.feeInitIncreaseSpawnCostTime + 
                             (_spawnCostTime - GameData.Instance.gameData.initSpawnCostTime) /
                             GameData.Instance.gameData.valuePerChangeSpawnCostTime *
                             GameData.Instance.gameData.rangeFeePerIncreaseSpawnCostTime;
            if (_currentCost < feeUpgrade) return;
            _currentCost -= (int)feeUpgrade;
            _spawnCostTime += GameData.Instance.gameData.valuePerChangeSpawnCostTime;
            UpdateCostTxt();
        }

        private void IncreaseDamage(Team team)
        {
            if(!IsClient || Player == null) return;
            var curDamage = team.GetSoldierData.Damage;
            var maxDamage = GameData.Instance.gameData.initDamage + GameData.Instance.gameData.valuePerIncreaseDamage *
                GameData.Instance.gameData.maxTimeOfIncreaseDamage;
            if (curDamage == maxDamage) return;
            var feeUpgrade = GameData.Instance.gameData.feeInitIncreaseDamage + (curDamage - GameData.Instance.gameData.initDamage) /
                              GameData.Instance.gameData.valuePerIncreaseDamage *
                             GameData.Instance.gameData.rangeFeePerIncreaseDamage;
            if (_currentCost < feeUpgrade) return;
            _currentCost -= feeUpgrade;
            UpSoldiersValueOnTeam(team,damage: GameData.Instance.gameData.valuePerIncreaseDamage);
            UpdateCostTxt();
            AudioController.Instance.Play("Coin",Vector3.zero);
        }
        
        private void IncreaseArmor(Team team)
        {
            if(!IsClient || Player == null) return;
            var curArmor = team.GetSoldierData.Armor;
            var maxArmor = GameData.Instance.gameData.initArmor + GameData.Instance.gameData.valuePerIncreaseArmor *
                GameData.Instance.gameData.maxTimeOfIncreaseArmor;
            if (curArmor == maxArmor) return;
            var feeUpgrade = GameData.Instance.gameData.feeInitIncreaseArmor + (curArmor - GameData.Instance.gameData.initArmor) /
                              GameData.Instance.gameData.valuePerIncreaseArmor *
                GameData.Instance.gameData.rangeFeePerIncreaseArmor;
            if (_currentCost < feeUpgrade) return;
            _currentCost -= feeUpgrade;
            UpSoldiersValueOnTeam(team,armor: GameData.Instance.gameData.valuePerIncreaseArmor);
            UpdateCostTxt();
            AudioController.Instance.Play("Coin",Vector3.zero);
        }

        private void CreateNewTeam()
        {
            if(!IsClient || Player == null) return;
            if(_currentCost < GameData.Instance.gameData.feeCreateSoldierTeam) return;
            _currentCost -= GameData.Instance.gameData.feeCreateSoldierTeam;
            UpdateCostTxt();
            Player.CreateNewTeam();
        }

        private void UpSoldiersValueOnTeam(Team team, int damage = 0, int armor = 0)
        {
            team.UpSoldiersValue(damage, armor);
        }

        #endregion

        private IEnumerator IncreaseCostPerSeconds()
        {
            while (_timeDown.Value > 0)
            {
                _currentCost++;
                UpdateCostTxt();
                yield return new WaitForSeconds(_spawnCostTime);
            }
        }

        private void UpdateCostTxt() => costTxt.text = $"$ {_currentCost}";
        
        private void TimeDownUpdate()
        {
            if (_timeDown.Value != _timeDownLocal)
            {
                _timeDownLocal = _timeDown.Value;
                int minutes = _timeDownLocal / 60;
                int seconds = _timeDownLocal % 60;
                timeDownTxt.text = $"{minutes:00}:{seconds:00}";
            }
        }

        private IEnumerator TimeDown()
        {
            while (_timeDown.Value > 0)
            {
                _timeDown.Value--;
                yield return new WaitForSeconds(1);
            }
        }
    }
}