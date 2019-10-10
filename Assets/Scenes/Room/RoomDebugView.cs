using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class RoomDebugView : MonoBehaviour
{

    EntityManager entityManager;

    private void Start() {
        var world = World.Active;
        entityManager = world.EntityManager;
        //获取Query
        roomNameQuery = entityManager.CreateEntityQuery(typeof(RoomNameComponent));
        playerQuery = entityManager.CreateEntityQuery(
            typeof(PlayerNameComponent),
            typeof(PlayerReadyStateComponent));
    }


    private void OnGUI() {

        var boxRect = new Rect(0, 0, 300, 200);
        GUI.Box(boxRect, "RoomInfo");

        var areaRect = new Rect(20, 20, 260, 160);
        GUILayout.BeginArea(areaRect);
        GUILayout.BeginVertical();
        GUILayout.Label("Room Name:"+ GetRoonName());
        if (playerQuery.CalculateEntityCount() == 0)
            DrawJoinButton();
        else {
            var isReady = playerQuery.GetSingleton<PlayerReadyStateComponent>();
            if(isReady.IsReady) {
                DrawCancelReadyButton();
            }
            else
                DrawReadyButton();
        }
        DrawPlayers();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    EntityQuery roomNameQuery;

    private string GetRoonName() {
        if (roomNameQuery.CalculateChunkCount() > 0) {
            var singleton = roomNameQuery.GetSingletonEntity();
            return entityManager.GetSharedComponentData<RoomNameComponent>(singleton).Value;
        } else
            return "Null";
    }

    EntityQuery playerQuery;

    private void DrawPlayers() {
        GUILayout.Label("Players");
        var players = playerQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
        for (int i = 0; i < players.Length; i++) {
            var playerName = entityManager.GetSharedComponentData<PlayerNameComponent>(players[i]);
            var isReady = entityManager.GetComponentData<PlayerReadyStateComponent>(players[i]);

            GUILayout.BeginHorizontal();
            GUILayout.Label(playerName.Value);
            GUILayout.Label(isReady.IsReady.ToString());
            GUILayout.EndHorizontal();
        }
        players.Dispose();
    }

    private void DrawJoinButton() {
        if(GUILayout.Button("Join Room")) {
            var world = World.Active;
            var entityManager = world.EntityManager;
            var roomQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<RoomNameComponent>());
            var roomEntity = roomQuery.GetSingletonEntity();
            var buffer = entityManager.GetBuffer<AddPlayerEvent>(roomEntity);
            buffer.Add(new AddPlayerEvent { PlayerId = 1 });
            Debug.Log("发送事件");
        }
    }

    private void DrawReadyButton() {
        if(GUILayout.Button("Ready")) {
            var players = playerQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < players.Length; i++) {
                var isReady = entityManager.GetComponentData<PlayerReadyStateComponent>(players[i]);
                isReady.IsReady = true;
                entityManager.SetComponentData(players[i], isReady);
            }

            players.Dispose();
        }
    }

    private void DrawCancelReadyButton() {
        if (GUILayout.Button("Cancel Ready")) {
            var players = playerQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < players.Length; i++) {
                var isReady = entityManager.GetComponentData<PlayerReadyStateComponent>(players[i]);
                isReady.IsReady = false;
                entityManager.SetComponentData(players[i], isReady);
            }

            players.Dispose();
        }
    }
}
