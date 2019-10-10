//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;

//namespace TankBattle
//{
//    /// <summary>
//    /// ServerOnly
//    /// </summary>
//    [UpdateInGroup(typeof(SimulationSystemGroup))]
//    [UpdateBefore(typeof(TurnSystem))]
//    public class GameStateMachineSystem : ComponentSystem
//    {

//        private Dictionary<GameState, StateBase> stateMap = new Dictionary<GameState, StateBase>();

//        private GameState nextState = GameState.NotStart;

//        EntityQuery InfoEntity_Query;

//        EntityQuery ServerPlayer_Query;

//        EntityQuery CurrentGameState_Query;




//        protected override void OnCreate() {
//            stateMap[GameState.NotStart] = new NotStartState(EntityManager, Entities, PostUpdateCommands);
//            stateMap[GameState.Moving] = new MovingState(EntityManager,Entities,PostUpdateCommands);
//            stateMap[GameState.Chraging] = new ChargingState(EntityManager, Entities, PostUpdateCommands);
//            stateMap[GameState.BulletFly] = new BulletFlyState(EntityManager, Entities, PostUpdateCommands);

//            InfoEntity_Query = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<InfoTag>());
//            ServerPlayer_Query = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<ServerTag>());
//            CurrentGameState_Query = EntityManager.CreateEntityQuery(typeof(CurrentGameState));
//        }

//        protected override void OnUpdate() {
//            //等待InfoEntity初始化完毕
//            var count = InfoEntity_Query.CalculateEntityCount();
//            if (count == 0) return;

//            //执行状态机行为
//            var infoEntity = CurrentGameState_Query.GetSingletonEntity();
//            var currentGameState = EntityManager.GetComponentObject<CurrentGameState>(infoEntity);

//            if (nextState != currentGameState.value) {
//                Debug.Log($"[GameStateMachineSystem]状态改变{currentGameState.value}->{nextState}");
//                currentGameState.value = nextState;//发送状态改变事件
//            }
//            if(stateMap.TryGetValue(currentGameState.value, out var state)) {
//                nextState = state.OnStateUpdate();
//            }
//        }

//        public abstract class StateBase
//        {
//            protected EntityManager entityManager;
//            protected EntityQueryBuilder entities;
//            protected EntityCommandBuffer postUpdateCommands;

//            public StateBase(EntityManager em,EntityQueryBuilder eqb,EntityCommandBuffer ecb) {
//                this.entityManager = em;
//                this.entities = eqb;
//                this.postUpdateCommands = ecb;
//            }

//            public virtual GameState GetStateType() {
//                return GameState.Moving;
//            }
//            public virtual void OnStateEnter() {
                
//            }

//            /// <summary>
//            /// 每个状态会不断的检测退出条件，决定下一个Update时候的状态
//            /// </summary>
//            public virtual GameState OnStateUpdate() {
//                return GetStateType();//永远停留在当前状态
//            }

//            public virtual void OnStateExit() {

//            }
//        }

//        public class NotStartState : StateBase
//        {
//            EntityQuery InfoEntity_Query_MissingGameStartEvent;
//            EntityQuery InfoEntity_Query;
//            public NotStartState(EntityManager em, EntityQueryBuilder eqb, EntityCommandBuffer ecb) : base(em, eqb, ecb) {
//                InfoEntity_Query_MissingGameStartEvent = em.CreateEntityQuery(
//                    ComponentType.ReadOnly<InfoTag>(),
//                    ComponentType.Exclude<GameStartEvent>());

//                InfoEntity_Query = em.CreateEntityQuery(ComponentType.ReadOnly<InfoTag>());

//                EventSystem<GameStartEvent>.Initialize(em.World);
//            }

//            public override void OnStateEnter() {
                
//            }

//            public override GameState OnStateUpdate() {
//                //收到游戏开始事件后
//                EventSystem<GameStartEvent>.AddMissingBuffers(entities, InfoEntity_Query_MissingGameStartEvent, entityManager);
//                var infoEntity = InfoEntity_Query.GetSingletonEntity();
//                var gameStartEventBuffer = entityManager.GetBuffer<GameStartEvent>(infoEntity);
//                if(gameStartEventBuffer.Length > 0) {
//                    //初始化游戏信息
//                    TurnService.InitTurnInfo(entityManager);
//                    //然后进入Moving状态
//                    return GameState.Moving;
//                }

//                return GameState.NotStart;
//            }

//            public override void OnStateExit() {
                
//            }
//        }

//        public class MovingState : StateBase
//        {
//            EntityQuery CurrentTurnActorIsCharging_Query;//当前回合Actor

//            public MovingState(EntityManager em,EntityQueryBuilder eqb, EntityCommandBuffer ecb) : base(em,eqb, ecb) {
//                CurrentTurnActorIsCharging_Query = em.CreateEntityQuery(
//                    ComponentType.ReadOnly<CurrentTurnActorTag>(),
//                    typeof(IsCharging));
//            }

//            public override GameState GetStateType() {
//                //如果检测到当前Actor玩家正在充能，则进入Charging状态
//                if(CurrentTurnActorIsCharging_Query.CalculateEntityCount()>0) {
//                    var currentTurnActorEntity = CurrentTurnActorIsCharging_Query.GetSingletonEntity();
//                    var isCharging = entityManager.GetComponentObject<IsCharging>(currentTurnActorEntity);
//                    if (isCharging.value) {
//                        return GameState.Chraging;
//                    }
//                }
//                return GameState.Moving;
//            }

//            public override GameState OnStateUpdate() {
//                return GetStateType();

//                //监听当前回合玩家开始蓄力
                

//            }
//        }

//        public class ChargingState : StateBase
//        {
//            EntityQuery CurrentTurnActorIsCharging_Query;//当前回合Actor
//            public ChargingState(EntityManager em, EntityQueryBuilder eqb, EntityCommandBuffer ecb) : base(em, eqb, ecb) {
//                CurrentTurnActorIsCharging_Query = em.CreateEntityQuery(
//                    ComponentType.ReadOnly<CurrentTurnActorTag>(),
//                    typeof(IsCharging));
//            }

//            public override void OnStateEnter() {
//                base.OnStateEnter();
//            }
//            public override GameState OnStateUpdate() {
//                //检测充能结束
//                //如果检测到当前Actor玩家充能结束，则进入BulletFly状态
//                if (CurrentTurnActorIsCharging_Query.CalculateEntityCount() > 0) {
//                    var currentTurnActorEntity = CurrentTurnActorIsCharging_Query.GetSingletonEntity();
//                    var isCharging = entityManager.GetComponentObject<IsCharging>(currentTurnActorEntity);
//                    if (!isCharging.value) {
//                        return GameState.BulletFly;
//                    }
//                }

//                return GameState.Chraging;
//            }
//            public override void OnStateExit() {
//                base.OnStateExit();
//            }
//        }
//        public class BulletFlyState : StateBase
//        {
//            EntityQuery Bullet_Query_MissingExplodeEvent;
//            EntityQuery Bullet_Query;

//            EntityQuery CurrentTurnNumber_Query;


//            public BulletFlyState(EntityManager em, EntityQueryBuilder eqb, EntityCommandBuffer ecb) : base(em, eqb, ecb) {
//                EventSystem<ExplodeEvent>.Initialize(em.World);
//                Bullet_Query_MissingExplodeEvent = em.CreateEntityQuery(
//                    typeof(Bullet),
//                    ComponentType.Exclude<ExplodeEvent>());
//                Bullet_Query = em.CreateEntityQuery(typeof(Bullet));

//                CurrentTurnNumber_Query = em.CreateEntityQuery(typeof(CurrentTurnNumber));
//            }

//            public override void OnStateEnter() {
//                base.OnStateEnter();
//            }
//            public override GameState OnStateUpdate() {
//                //检测爆炸发生事件(或者场景中不再含有子弹实体)
//                EventSystem<ExplodeEvent>.AddMissingBuffers(entities, Bullet_Query_MissingExplodeEvent, entityManager);
//                var bulletArray = Bullet_Query.ToEntityArray(Unity.Collections.Allocator.TempJob);

//                bool hasRecievedExplodeEvent = false;
//                for (int i = 0; i < bulletArray.Length; i++) {
//                    var eventBuffer = entityManager.GetBuffer<ExplodeEvent>(bulletArray[i]);
//                    if(eventBuffer.Length > 0) {
//                        Debug.Log("[BulletFlyState]响应ExplodeEvent");
//                        hasRecievedExplodeEvent = true;
//                        break;                       
//                    }
//                }

//                bulletArray.Dispose();

//                if (hasRecievedExplodeEvent) {
//                    //切换回合
//                    TurnService.NextTurn(entityManager);

//                    //回合切换完成，重新进入Moving状态
//                    return GameState.Moving;
//                }

//                return GameState.BulletFly;
//            }
//            public override void OnStateExit() {
//                base.OnStateExit();
//            }
//        }
//    }
//}


