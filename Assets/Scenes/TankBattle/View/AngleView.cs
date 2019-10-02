using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TankBattle
{
    public class AngleView : MonoBehaviour
    {
        [SerializeField] Angle angle = null;
        [SerializeField] TextMeshPro angleText = null;
        [SerializeField] uint angleValue = 0;
        [SerializeField] Transform pointer = null;

        private void Update() {
            if(angleText != null)
                angleText.text = $"Angle:{angleValue}";
        }

        private void FixedUpdate() {
            //刷新角度
            if(angle != null) {
                angleValue = angle.value;
                pointer.localEulerAngles = new Vector3(0, 0, angleValue);
            }
        }
    }
}


