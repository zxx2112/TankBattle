using Lightbug.Kinematic2D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCharacterController2D : MonoBehaviour
{
    CharacterMotor m_characterMotor;

    [SerializeField] float m_speed = 4f;

    private void Awake() {
        m_characterMotor = GetComponent<CharacterMotor>();
    }

    private void FixedUpdate() {
        var velocity = (Input.GetAxisRaw("Horizontal") * Vector2.right +
            Input.GetAxisRaw("Vertical") * Vector2.up
        ).normalized * m_speed;
        m_characterMotor.SetVelocity(velocity);
    }
}
