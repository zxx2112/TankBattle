using UnityEngine;
using Sirenix.OdinInspector;
using Mirror;
using Lightbug.Kinematic2D;
using Lightbug.Kinematic2D.Core;
using Lightbug.Kinematic2D.Implementation;

namespace TankBattle
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] CharacterBody2D body = null;
        [SerializeField] CharacterController2D controller = null;
        [SerializeField] CharacterBrain brain = null;
        [SerializeField] HorizontalMovement horizontal = null;
        [SerializeField] VerticalMovement vertical = null;
        [SerializeField] InputHandler inputHandler = null;

        bool clientStarted;
        public bool ClientStarted => clientStarted;

        public override void OnStartClient() {
            clientStarted = true;

            this.name = $"Player(NetId:{netId})";
        }

        public override void OnStartLocalPlayer() {
            body.enabled = isLocalPlayer;
            controller.enabled = isLocalPlayer;
            brain.enabled = isLocalPlayer;
            horizontal.enabled = isLocalPlayer;
            vertical.enabled = isLocalPlayer;
            inputHandler.enabled = isLocalPlayer;
        }

    }

}

