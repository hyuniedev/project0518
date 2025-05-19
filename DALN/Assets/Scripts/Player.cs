using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class Player : NetworkBehaviour
{
    private Team _selectedTeam = new Team();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Soldier"))
                {
                    _selectedTeam = hit.collider.gameObject.transform.parent.GetComponent<Team>();
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _selectedTeam.TeamMoveTo(hit.point);
            }
        }
    }
}