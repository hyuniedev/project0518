using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Group group;
    public Camera camera;
    private void Update()
    {
        CheckPositionMouseClick();
    }

    private void CheckPositionMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                group.GroupMoveTo(hit.point);
            }
        }
    }
}
