using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Entities;

namespace TankBattle
{
    public class GameFlowController : NetworkBehaviour
    {

        private EntityManager entityManager;

        private EntityQuery CurrentTurnActor_Query;

        private EntityQuery Player_Query;

        private EntityQuery Info_Query;

        private void Awake() {
            entityManager = World.Active.EntityManager;
            CurrentTurnActor_Query = entityManager.CreateEntityQuery(typeof(CurrentTurnActor));
            Player_Query = entityManager.CreateEntityQuery(typeof(Player));
            Info_Query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<InfoTag>());
            //NetworkClient.RegisterHandler<GameStartMessage>();
        }

        [Command]
        public void CmdGameStart() {
            //客户端请求游戏开始
            //向ECS系统发送GameStart事件
            var infoEntity = Info_Query.GetSingletonEntity();
            var gameStartEventBuffer = entityManager.GetBuffer<GameStartEvent>(infoEntity);
            gameStartEventBuffer.Add(new GameStartEvent());
        }


        [ClientRpc]
        public void RpcGameStart() {
            //增删CurrentTurnActorTag
            Debug.Log("[GameFlowController]GameStart!");

            var currentTurnActorEntity = CurrentTurnActor_Query.GetSingletonEntity();
            var currentTurnActor = entityManager.GetComponentObject<CurrentTurnActor>(currentTurnActorEntity);

            var players = Player_Query.ToComponentArray<Player>();
            var playerEntities = Player_Query.ToEntityArray(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < players.Length; i++) {
                if(players[i].netId == currentTurnActor.value) {
                    if (!entityManager.HasComponent<CurrentTurnActorTag>(playerEntities[i])) {
                        entityManager.AddComponentData(playerEntities[i],new CurrentTurnActorTag());
                        Debug.Log($"[GameFlowController]为{entityManager.GetName(playerEntities[i])}增加CurrentTurnActorTag");
                    }
                }
                else {
                    if (entityManager.HasComponent<CurrentTurnActorTag>(playerEntities[i])) {
                        entityManager.RemoveComponent<CurrentTurnActorTag>(playerEntities[i]);
                        Debug.Log($"[GameFlowController]移除{entityManager.GetName(playerEntities[i])}的CurrentTurnActorTag");
                    }
                }
            }

            playerEntities.Dispose();
        }
    }
}


