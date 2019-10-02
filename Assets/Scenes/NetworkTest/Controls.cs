using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

namespace NetworkTest
{
    public class Controls : MonoBehaviour
    {
        private Vector2 m_Move;
        public void OnMove(InputAction.CallbackContext context) {
            m_Move = context.ReadValue<Vector2>();
        }

        private void Update() {

        }
    }

}

