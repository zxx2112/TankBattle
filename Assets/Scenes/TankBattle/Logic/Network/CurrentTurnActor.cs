using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace TankBattle
{
    /// <summary>
    /// 当前回合的玩家
    /// </summary>
    public class CurrentTurnActor : NetworkBehaviour
    {
        [SyncVar]
        public uint value;//当前回合玩家的网络ID
    }
}


