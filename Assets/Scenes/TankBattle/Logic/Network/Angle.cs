using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace TankBattle
{
    public class Angle : NetworkBehaviour
    {
        [SyncVar]
        public uint value;
    }
}


