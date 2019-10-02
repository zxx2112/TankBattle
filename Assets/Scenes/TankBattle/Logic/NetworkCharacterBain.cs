using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Lightbug.Kinematic2D.Implementation;
using Sirenix.OdinInspector;
using Unity.Entities;

namespace TankBattle
{
    public class NetworkCharacterBain : CharacterBrain
    {

        [SerializeField] NetworkIdentity identity = null;
        [SerializeField] CharacterInputData inputData = null;

        EntityManager entityManager;
        EntityQuery CurrentGameState_Query;

        private void Awake() {
            entityManager = World.Active.EntityManager;
            CurrentGameState_Query = entityManager.CreateEntityQuery(typeof(CurrentGameState));
        }

        public override bool IsAI() {
            return false;
        }

        protected override void Update() {
            if (identity == null || identity.isLocalPlayer == false) return;//没有网络身份，屏蔽任何输入

            //需要当前为Moving状态才能接受移动输入
            if (CurrentGameState_Query.CalculateEntityCount()>0) {
                var infoEntity = CurrentGameState_Query.GetSingletonEntity();
                var currentGameState = entityManager.GetComponentObject<CurrentGameState>(infoEntity);
                if (currentGameState.value != GameState.Moving) return;
            }

            if (inputData == null || Time.timeScale == 0) //输入需要的数据，时间缩放不能为0
                return;

            //重写语义转换，加入网络因素
            characterAction.right |= GetAxis(inputData.horizontalAxisName) > 0;
            characterAction.left |= GetAxis(inputData.horizontalAxisName) < 0;
            characterAction.up |= GetAxis(inputData.verticalAxisName) > 0;
            characterAction.down |= GetAxis(inputData.verticalAxisName) < 0;

            characterAction.jumpPressed |= GetButtonDown(inputData.jumpName);
            characterAction.jumpReleased |= GetButtonUp(inputData.jumpName);

            characterAction.dashPressed |= GetButtonDown(inputData.dashName);
            characterAction.dashReleased |= GetButtonUp(inputData.dashName);

            characterAction.jetPack |= GetButton(inputData.jetPackName);
        }


        protected virtual float GetAxis(string axisName, bool raw = true) {
            return raw ? Input.GetAxisRaw(axisName) : Input.GetAxis(axisName);
        }

        protected virtual bool GetButton(string actionInputName) {
            return Input.GetButton(actionInputName);
        }

        protected virtual bool GetButtonDown(string actionInputName) {
            return Input.GetButtonDown(actionInputName);
        }

        protected virtual bool GetButtonUp(string actionInputName) {
            return Input.GetButtonUp(actionInputName);
        }
    }
}


