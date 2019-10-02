using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Entities;

namespace TankBattle
{
    public class Bullet : NetworkBehaviour,IConvertGameObjectToEntity
    {

        bool destroyed;

        Entity _entity;

        public class ExplodeMessage : MessageBase
        {
            public Vector2 position;
        }

        [SerializeField] LayerMask collideWithLayer;

        public override void OnStartClient() {
            NetworkClient.RegisterHandler<ExplodeMessage>(GenerateExplode);
        }


        /// <summary>
        /// 应该只在服务端执行
        /// </summary>
        /// <param name="collision"></param>
        private void OnTriggerEnter2D(Collider2D collision) {
            if (destroyed) return;

            if(isServer) {
                var collisionLayer = 1 << collision.gameObject.layer;
                var result = collisionLayer & collideWithLayer.value;
                if (result > 0) {
                    Debug.Log("[Server]向所有客户端广播生成爆炸");
                    var message = new ExplodeMessage() {
                        position = transform.position
                    };
                    NetworkServer.SendToAll(message);

                    //发送ECS爆炸事件
                    var eventBuffer = World.Active.EntityManager.GetBuffer<ExplodeEvent>(_entity);
                    eventBuffer.Add(new ExplodeEvent());
                    Debug.Log("[Bullet]发送ExplodeEvent");

                    //如果不延迟，那么会直接销毁，ExplodeMessage也不会发送
                    StartCoroutine(DelayOneFrameDestroy());
                   
                }
            }
        }

        private IEnumerator DelayOneFrameDestroy() {
            destroyed = true;
            yield return null;
            NetworkServer.Destroy(gameObject);
        }

        private void GenerateExplode(NetworkConnection connection,ExplodeMessage message) {
            var stamp = GetComponent<TerrainStampGenerator>();
            if (stamp != null) stamp.GenerateStamp(message.position);
        }

        //
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponent(entity, typeof(BulletTag));
            dstManager.AddBuffer<ExplodeEvent>(entity);
            _entity = entity;
        }
    }
}


