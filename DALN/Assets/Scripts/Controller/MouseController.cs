using System;
using UnityEngine;

namespace Controller
{
    public class MouseController : MonoBehaviour
    {
        [SerializeField] private GameObject arrowObject;
        private float timeHideArrow = float.MaxValue;
        private void Start()
        {
            arrowObject.SetActive(false);
        }

        private void OnEnable()
        {
            ActionEvent.OnMove += ActiveArrow;
        }

        private void OnDisable()
        {
            ActionEvent.OnMove -= ActiveArrow;
        }
        
        private void Update()
        {
            if (Time.time > timeHideArrow)
            {
                arrowObject.SetActive(false);
            }
        }

        private void ActiveArrow(Vector3 position)
        {
            arrowObject.transform.position = position + Vector3.up * 1f;
            arrowObject.SetActive(true);
            timeHideArrow = Time.time + 2f;
        }
    }
}