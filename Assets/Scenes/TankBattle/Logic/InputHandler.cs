using Destructible2D;
using Mirror;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using Unity.Entities;

namespace TankBattle
{
    public class InputHandler : NetworkBehaviour
    {
        [BoxGroup("配置")]
        //[SerializeField] float movementSpeed = 1;
        //[SerializeField] float Intercept = 0;
        [SerializeField] float velocityMultiplier = 1;
        [SerializeField] float angleSpeedMultiplier = 1;
        [SerializeField] Vector2 offset = new Vector2(0,1);
        [BoxGroup("引用")]
        [SerializeField] GameObject bulletPrefab = null;
        [SerializeField] ForcePercent forcePercent = null;
        [SerializeField] Angle angle = null;
        [SerializeField] Transform gunAnchor = null;
        [SerializeField] IsCharging isCharging = null;
        
        private InputActions controls;

        private EntityQuery CurrentGameState_Query;

        private EntityQuery CurrentTurnActor_Query;

        private EntityQuery CurrentTurnActorTransform_Query;

        private EntityManager entityManager;

        private float tempAngleDelta = 1;
        private Vector2 direction = Vector2.zero;

        void Awake() {
            controls = new InputActions();

            controls.Player.Charge.started += OnChargeStarted;
            controls.Player.Charge.canceled += OnChargeCanceled;
            controls.Player.Angle.started += OnAngleStarted;
            controls.Player.Angle.canceled += OnAngleCanceled;

            entityManager = World.Active.EntityManager;
            CurrentGameState_Query = entityManager.CreateEntityQuery(typeof(CurrentGameState));
            CurrentTurnActor_Query = entityManager.CreateEntityQuery(typeof(CurrentTurnActor));
            CurrentTurnActorTransform_Query = entityManager.CreateEntityQuery(
                typeof(CurrentTurnActorTag),
                typeof(Transform));
        }

        private void Start() {
            var entityManager = World.Active.EntityManager;
        }

        public void OnEnable() {
            controls.Enable();
        }

               
        public void OnDisable() {
            controls.Disable();
        }

        private void Update() {
            var delta = controls.Player.Angle.ReadValue<float>();
            if (delta > 0) {
                //Debug.Log("Angle > 0");
                //开始累积超过1就给angle+1
                tempAngleDelta += angleSpeedMultiplier * Time.deltaTime;
                if (tempAngleDelta > 1) {
                    tempAngleDelta = 0;
                    angle.value = (uint)Mathf.Clamp((int)(angle.value + 1), 0, 90);
                }
            } else if (delta < 0) {
                //Debug.Log("Angle < 0");
                //开始累积超过-1就给angle-1
                tempAngleDelta -= angleSpeedMultiplier * Time.deltaTime;
                if (tempAngleDelta < -1) {
                    tempAngleDelta = 0;
                    angle.value = (uint)Mathf.Clamp((int)(angle.value - 1), 0, 90);
                }
            }

            //计算角度方向
            var angleValue = angle.value;
            var x = Mathf.Cos(angleValue * Mathf.Deg2Rad);
            var y = Mathf.Sin(angleValue * Mathf.Deg2Rad);
            direction = new Vector2(x, y);
        }

        void FixedUpdate() {
            // 只有处于蓄力状态才允许蓄力
            if(isCharging.value) {
                if (controls.Player.Charge.ReadValue<float>() > 0) {
                    //使用EntityManager设置ForcePercent组件的值
                    OnChargeButtonPressing();
                }
            }
        }

        private void OnChargeStarted(CallbackContext ctx) {
            // 不是本地玩家，禁用蓄力
            if (!isLocalPlayer) return;
            // 在没轮到自己的时候禁止蓄力
            var infoEntiy = CurrentTurnActor_Query.GetSingletonEntity();
            var currentTurnActor = entityManager.GetComponentObject<CurrentTurnActor>(infoEntiy);
            if (currentTurnActor.value != netId) return;
            // 在不是Moving状态的时候禁止蓄力
            var gameStateEntity = CurrentGameState_Query.GetSingletonEntity();
            var currentGameState = entityManager.GetComponentObject<CurrentGameState>(gameStateEntity);
            if (currentGameState.value != GameState.Moving) return;

            isCharging.value = true;

        }

        private void OnChargeCanceled(CallbackContext ctx) {
            isCharging.value = false;

            Vector2 position = Vector2.zero;

            //使用鼠标位置
            //var mainCamera = Camera.main;
            //if(mainCamera != null) {
            //    position = D2dHelper.ScreenToWorldPosition(Input.mousePosition, Intercept, mainCamera);
            //}
            //使用当前回合Actor位置
            if(CurrentTurnActorTransform_Query.CalculateEntityCount()>0) {
                var currentTurnActorEntity = CurrentTurnActorTransform_Query.GetSingletonEntity();
                var transform = entityManager.GetComponentObject<Transform>(currentTurnActorEntity);

                position = gunAnchor.position;
            }

            var force = forcePercent.value;
            CmdFire(position, direction * velocityMultiplier * force);
            //发送一个事件，驱动ECS切换状态机
        }
        private void OnAngleStarted(CallbackContext ctx) {
            if(ctx.ReadValue<float>() > 0)
                tempAngleDelta = 1;
            else if(ctx.ReadValue<float>() < 0)
                tempAngleDelta = -1;

            //Debug.Log("[InputHandler]OnAngleStarted");
        }

        private void OnAngleCanceled(CallbackContext ctx) {
            //Debug.Log("[InputHandler]OnAngleCanceled");
        }

        /// <summary>
        /// 检测空格按下状态，增加力度百分比
        /// </summary>
        /// <param name="ctx"></param>
        private void OnChargeButtonPressing() {
            forcePercent.value = Mathf.Clamp01(forcePercent.value + 0.01f);//每秒加0.25
        }

        /// <summary>
        /// 服务端执行
        /// </summary>
        /// <param name="position"></param>
        [Command]
        void CmdFire(Vector2 position,Vector2 velocity) {//使用direction，还是Angle作为参数？其实没啥差别
            //在服务端调用
            //生成爆炸物体
            Debug.Log("[Server]生成子弹");
            var bulletInstance = GameObject.Instantiate(bulletPrefab);
            bulletInstance.transform.position = (Vector3)position;
            NetworkServer.Spawn(bulletInstance);
            bulletInstance.GetComponent<Rigidbody2D>().velocity = velocity;
            //通知客户端RPC命令完成
            RpcOnFire(position);
        }
        /// <summary>
        /// 客户端执行
        /// </summary>
        /// <param name="position"></param>
        [ClientRpc]
        void RpcOnFire(Vector2 position) {
            //通知所有客户端
            Debug.Log("[Client]RpcOnFire");
            //var stamp = GetComponent<TerrainStampGenerator>();
            //if (stamp != null) stamp.GenerateStamp(position
        }

        private void OnDrawGizmos() {
            Gizmos.DrawLine(transform.position,transform.position + (Vector3)direction);
        }
    }
}



