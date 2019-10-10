//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Sirenix.OdinInspector;
////using Mirror;

//namespace TankBattle
//{

//    public class DebugView : MonoBehaviour
//    {
//        public Rect rect = new Rect(Screen.width / 2, 0, 200, 700);

//        //游戏信息
//        [ReadOnly]
//        public float forcePercent;//力量百分比
//        public string currentTurnAcor;//本地玩家
//        public GameState currentGameState;//当前游戏状态
//        public uint currentTurnNumber;//当前回合数

//        //当前回合的玩家

//        EntityQuery CurrentTurnActorForcePercent_Query;
//        EntityQuery CurrentGameState_Query;
//        EntityQuery CurrentTurnNumber_Query;
//        EntityQuery Server_Query;

//        EntityManager entityManager;

//        private void Awake() {
//            entityManager = World.Active.EntityManager;
//            CurrentTurnActorForcePercent_Query = entityManager.CreateEntityQuery(
//                typeof(Player),
//                ComponentType.ReadOnly<CurrentTurnActorTag>(),//当前回合主角
//                typeof(ForcePercent));//力度百分比

//            CurrentGameState_Query = entityManager.CreateEntityQuery(typeof(CurrentGameState));
//            CurrentTurnNumber_Query = entityManager.CreateEntityQuery(typeof(CurrentTurnNumber));
//            Server_Query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<ServerTag>());
//        }

//        private void Update() {
//            if(CurrentTurnActorForcePercent_Query.CalculateEntityCount() > 0) {
//                var singletonEntiy = CurrentTurnActorForcePercent_Query.GetSingletonEntity();
//                var ForcePercent = entityManager.GetComponentObject<ForcePercent>(singletonEntiy);
//                if (ForcePercent == null) return;
//                forcePercent = ForcePercent.value;
//                currentTurnAcor = ForcePercent.netId.ToString();
//            }

//            if(CurrentGameState_Query.CalculateEntityCount() > 0) {
//                var infoEntity = CurrentGameState_Query.GetSingletonEntity();
//                currentGameState = entityManager.GetComponentObject<CurrentGameState>(infoEntity).value;
//            }

//            if(CurrentTurnNumber_Query.CalculateChunkCount()> 0) {
//                var infoEntity = CurrentGameState_Query.GetSingletonEntity();
//                currentTurnNumber = entityManager.GetComponentObject<CurrentTurnNumber>(infoEntity).value;
//            }


            
//        }


//        private void OnGUI() {
//            GUILayout.BeginArea(rect);

//            GUILayout.BeginVertical();

//            GUILayout.BeginHorizontal();
//            GUILayout.Label("力量百分比：");
//            GUILayout.Label(forcePercent.ToString("F2"));
//            GUILayout.EndHorizontal();

//            DrawDebugItem("当前回合玩家", currentTurnAcor);
//            DrawDebugItem("当前游戏状态", currentGameState.ToString());
//            DrawDebugItem("当前回合数", currentTurnNumber.ToString());

//            //if(Server_Query.CalculateEntityCount()>0) {
//            //    if(GUILayout.Button("Start Game")) {
//            //        //发送开始游戏事件
//            //        //查找到LocalPlayer,用其上的GameFlowController发送Command来开始游戏
//            //        var PlayerGameFlowController_Query = entityManager.CreateEntityQuery(typeof(Player),typeof(GameFlowController));
//            //        var playerArray = PlayerGameFlowController_Query.ToComponentArray<Player>();
//            //        for (int i = 0; i < playerArray.Length; i++) {
//            //            if(playerArray[i].isLocalPlayer) {
//            //                var gameFlowController = playerArray[i].GetComponent<GameFlowController>();
//            //                gameFlowController.CmdGameStart();
//            //            }
//            //        }
//            //        //NetworkServer.SendToAll(new GameStartMessage());
//            //    }

//            //}

//            GUILayout.EndVertical();

//            GUILayout.EndArea();
//        }

//        private void DrawDebugItem(string label,string value) {
//            GUILayout.BeginHorizontal();
//            GUILayout.Label(label+":");
//            GUILayout.Label(value);
//            GUILayout.EndHorizontal();
//        }

//    }

//}
