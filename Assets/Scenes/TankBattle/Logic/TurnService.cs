//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;

//namespace TankBattle
//{
//    public static class TurnService
//    {
//        //初始化回合信息
//        public static void InitTurnInfo(EntityManager em) {
//            TurnService.SetTurnNumber(em, 1);
//            Debug.Log("[InitTurnInfo]回合初始化为1");

//            var infoQuery = em.CreateEntityQuery(
//                ComponentType.ReadOnly<InfoTag>(),
//                typeof(CurrentTurnActor));

//            if (infoQuery.CalculateEntityCount() > 0) {
//                var infoEntity = infoQuery.GetSingletonEntity();
//                var currentTurnActor = em.GetComponentObject<CurrentTurnActor>(infoEntity);

//                var player_Query = em.CreateEntityQuery(typeof(Player));
//                if (player_Query.CalculateEntityCount() > 0) {
//                    var playerArray = player_Query.ToComponentArray<Player>();//托管内存
//                    var firstPlayer = playerArray[0];
//                    currentTurnActor.value = firstPlayer.netId;
//                    Debug.Log($"[InitTurnInfo]当前回合玩家初始化为{firstPlayer.netId}");
//                } 
//                else
//                    throw new System.Exception("玩家数量为0，无法初始化当前回合玩家");


//            } else
//                throw new System.Exception("游戏信息组件未找到");

            

//            //var gameFlowControllerEntity = GameFlowController_Query.GetSingletonEntity();
//            //var gameFlowCOntroller = EntityManager.GetComponentObject<GameFlowController>(gameFlowControllerEntity);
//            //gameFlowCOntroller.RpcGameStart();
//            //Debug.Log("[InitTurnInfo]通知客户端游戏开始");
//        }

//        public static void NextTurn(EntityManager entityManager) {
//            var CurrentTurnNumber_Query = entityManager.CreateEntityQuery(typeof(CurrentTurnNumber));
//            var nextTurnNumber = GetTurnNumber(entityManager, CurrentTurnNumber_Query) + 1;
//            SetTurnNumber(entityManager, nextTurnNumber);
//        }

//        public static uint GetTurnNumber(EntityManager entityManager,EntityQuery CurrentTurnNumber_Query) {
//            var currentTurnNumberEntity = CurrentTurnNumber_Query.GetSingletonEntity();
//            var currentTurnNumber = entityManager.GetComponentObject<CurrentTurnNumber>(currentTurnNumberEntity);
//            return currentTurnNumber.value;
//        }

//        public static void SetTurnNumber(EntityManager entityManager, uint turnNumber) {
//            var CurrentTurnNumber_Query = entityManager.CreateEntityQuery(typeof(CurrentTurnNumber));
//            var currentTurnNumberEntity = CurrentTurnNumber_Query.GetSingletonEntity();
//            var currentTurnNumber = entityManager.GetComponentObject<CurrentTurnNumber>(currentTurnNumberEntity);
//            currentTurnNumber.value = turnNumber;

//            Debug.Log("[TurnService]设置回合数为" + turnNumber);
//        }
//    }
//}


