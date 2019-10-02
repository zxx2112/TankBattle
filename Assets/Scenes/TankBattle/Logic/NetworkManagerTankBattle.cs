using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace TankBattle
{
    public class NetworkManagerTankBattle : NetworkManager
    {
        [SerializeField] GameObject otherInfoPrefab = null;

        public override void OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage) {
            base.OnServerAddPlayer(conn, extraMessage);

            //第一个玩家连接的时候
            if(numPlayers == 1) {
                //实例化游戏信息物体
                var otherInfo = Instantiate(otherInfoPrefab);
                NetworkServer.Spawn(otherInfo);//要求预制体上有NetworkIdentity
            }
        }
    }
}


