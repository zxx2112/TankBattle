using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class RoomSystem : ComponentSystem
{
    EntityQuery RoomEntity_Query;
    EntityQuery RoomName_Query_MissingAddPlayer;

    protected override void OnCreate() {
        RoomEntity_Query = GetEntityQuery(
            ComponentType.ReadOnly<RoomNameComponent>(),
            typeof(AddPlayerEvent));
        RoomName_Query_MissingAddPlayer = GetEntityQuery(
            ComponentType.ReadOnly<RoomNameComponent>(),
            ComponentType.Exclude<AddPlayerEvent>()
            );

        EventSystem<AddPlayerEvent>.Initialize(World);//初始化清理System
    }

    protected override void OnUpdate() {
        //加入房间请求事件
        EventSystem<AddPlayerEvent>.AddMissingBuffers(Entities, RoomName_Query_MissingAddPlayer, EntityManager);
        Entities.With(RoomEntity_Query).ForEach((Entity entity,DynamicBuffer<AddPlayerEvent> buffer) => {
            //Debug.Log("遍历RoomEntity");
            var length = buffer.Length;
            for (int i = 0; i < length; i++) {
                var ev = buffer[i];
                Debug.Log("开始处理一个事件");
                //创建Player
                //实现查找到PlayerSpawner
                var playerSpawner = GetSingleton<RoomPlayerSpawnerComponent>();
                var player = EntityManager.Instantiate(playerSpawner.RoomPlayerPrefab);
                EntityManager.SetName(player, "Player_" + ev.PlayerId);
                Debug.Log("处理一个事件结束");
            }
            //Debug.Log("处理所有事件结束");
        });
        
    }
}
