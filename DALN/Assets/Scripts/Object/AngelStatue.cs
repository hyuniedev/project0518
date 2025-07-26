using System;
using Controller;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Object
{
    public class AngelStatue : NetworkBehaviour
    {
        private NetworkVariable<int> _teamIdTarget = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> WaitPray { get; private set; } = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server); 
        [SerializeField] private Text teamNameText;
        [SerializeField] private Image teamIcon;
        [SerializeField] private GameObject angelIcon;
        private int _curTeamId = 0;
        public int WinnerId => _curTeamId;
        private void Start()
        {
            UpdateAngelStatue();
        }

        private void Update()
        {
            if (!IsClient) return;
            VisibleAngelIcon();
            if (_teamIdTarget.Value == 0 || _teamIdTarget.Value == _curTeamId) return;
            _curTeamId = _teamIdTarget.Value;
            UpdateAngelStatue();
            AudioController.Instance.Play("PrayCompleted", Vector3.zero);
        }

        private void UpdateAngelStatue()
        {
            teamNameText.text = _curTeamId==0?"None":$"Team {_curTeamId}";
            switch (_curTeamId)
            {
                case 1 : 
                    teamIcon.color = Color.green;
                    break;
                case 2 :
                    teamIcon.color = Color.gray;
                    break;
                case 3 :
                    teamIcon.color = Color.yellow;
                    break;
                default:
                    teamIcon.color = Color.black;
                    break;
            }
        }

        private void VisibleAngelIcon()
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                angelIcon.gameObject.SetActive(false);
            }
            else
            {
                Vector3 screenDir = (Camera.main.WorldToScreenPoint(transform.position) - new Vector3(Screen.width / 2, Screen.height / 2, 0)).normalized;
                float edgeBuffer = 50f;
                Vector3 iconPos = new Vector3(
                    Mathf.Clamp(screenDir.x * Screen.width/2 + Screen.width/2,edgeBuffer, Screen.width - edgeBuffer), 
                    Mathf.Clamp(screenDir.y * Screen.height/2 + Screen.height/2, edgeBuffer, Screen.height - edgeBuffer), 
                    0);
                angelIcon.transform.position = iconPos;
                angelIcon.gameObject.SetActive(true);
            }
        }
        
        public void RequestSetTeamIdTarget(int teamId)
        {
            if(_teamIdTarget.Value == teamId) return;
            SetTeamIdTargetServerRpc(teamId);
        }
        
        [ServerRpc]
        private void SetTeamIdTargetServerRpc(int teamId) => _teamIdTarget.Value = teamId;

        [ServerRpc(RequireOwnership = false)]
        public void SetWaitPrayServerRpc(bool wait) => WaitPray.Value = wait;
        
        private void OnMouseEnter()
        {
            GetComponent<Outline>().enabled = true;
        }

        private void OnMouseExit()
        {
            GetComponent<Outline>().enabled = false;
        }
    }
}