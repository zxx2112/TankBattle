//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;

//namespace TankBattle
//{
//    [UpdateInGroup(typeof(InitializationSystemGroup))]
//    public class PlayerInitSystem : ComponentSystem
//    {
//        EntityQuery Player_Query_WithoutServerOrClient;
//        protected override void OnCreate() {
//            Player_Query_WithoutServerOrClient = EntityManager.CreateEntityQuery(
//                typeof(Player),
//                ComponentType.Exclude<ServerTag>(),
//                ComponentType.Exclude<ClientTag>());
//        }

//        protected override void OnUpdate() {
//            //查询所有Player，检查是否初始化完毕，初始化完毕后添加Server或者Clent组件
//            Entities.With(Player_Query_WithoutServerOrClient).ForEach((Entity playerEntity,Player player) => {
//                if(player.ClientStarted) {
//                    if(player.isServer) {
//                        EntityManager.AddComponentData(playerEntity, new ServerTag());
//                        Debug.Log($"为{EntityManager.GetName(playerEntity)}添加ServerTag");
//                    }
//                    else {
//                        EntityManager.AddComponentData(playerEntity, new ClientTag());
//                        Debug.Log($"为{EntityManager.GetName(playerEntity)}添加ClientTag");
//                    }
//                }
//            });
//        }
//    }
//}


