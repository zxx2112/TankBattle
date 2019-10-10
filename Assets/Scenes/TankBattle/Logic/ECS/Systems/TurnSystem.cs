//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;

//namespace TankBattle
//{
//    //仅在服务端运行
//    [UpdateInGroup(typeof(SimulationSystemGroup))]
//    [UpdateAfter(typeof(GameStateMachineSystem))]
//    public class TurnSystem : ComponentSystem
//    {
//        EntityQuery ServerPlayer_Query;
//        EntityQuery CurrentTurnNumber_Query;
//        EntityQuery CurrentTurnActor_Query;
//        EntityQuery GameFlowController_Query;
//        EntityQuery Info_Query_MissingNextTurnEvent;
//        EntityQuery Info_Query_MissingNextTurnDoneEvent;
//        EntityQuery Info_Query;

//        bool inited = false;

//        protected override void OnCreate() {
//            ServerPlayer_Query = EntityManager.CreateEntityQuery(
//                typeof(Player),
//                ComponentType.ReadOnly<ServerTag>());
//            CurrentTurnNumber_Query = EntityManager.CreateEntityQuery(typeof(CurrentTurnNumber));
//            CurrentTurnActor_Query = EntityManager.CreateEntityQuery(typeof(CurrentTurnActor));
//            GameFlowController_Query = EntityManager.CreateEntityQuery(typeof(GameFlowController));
//            Info_Query_MissingNextTurnEvent = EntityManager.CreateEntityQuery(
//                ComponentType.ReadOnly<InfoTag>(),
//                ComponentType.Exclude<NextTurnEvent>());
//            Info_Query_MissingNextTurnDoneEvent = EntityManager.CreateEntityQuery(
//                ComponentType.ReadOnly<InfoTag>(),
//                ComponentType.Exclude<NextTurnDoneEvent>());

//            Info_Query = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<InfoTag>());

//            EventSystem<NextTurnEvent>.Initialize(World);
//            EventSystem<NextTurnDoneEvent>.Initialize(World);
//        }


//        protected override void OnUpdate() {
//            return;
//            // 回合初始化
//            if (!inited) {
//                // 仅在服务端执行,初始化回合数，有可能太快了网络还没初始化完成
//                Entities.With(ServerPlayer_Query).ForEach((Entity serverPlayerEntity, Player player) => {
//                    if (player.ClientStarted) {
//                        TurnService.SetTurnNumber(EntityManager, 1);
//                        inited = true;
//                        Debug.Log("[TurnSystem]回合初始化为1");

//                        var currentTurnActorEntity = CurrentTurnActor_Query.GetSingletonEntity();
//                        var currentTurnActor = EntityManager.GetComponentObject<CurrentTurnActor>(currentTurnActorEntity);
//                        currentTurnActor.value = player.netId;
//                        Debug.Log($"[TurnSystem]当前回合玩家初始化为{player.netId}");

//                        var gameFlowControllerEntity = GameFlowController_Query.GetSingletonEntity();
//                        var gameFlowCOntroller = EntityManager.GetComponentObject<GameFlowController>(gameFlowControllerEntity);
//                        gameFlowCOntroller.RpcGameStart();
//                        Debug.Log("[TurnSystem]通知客户端游戏开始");
//                        // (客户端会读取当前回合玩家NetworkId，然后增删CurrentTurnActorTag)
//                    }
//                });
//            }

//            if(!inited) return;
//        }


//    }
//}


