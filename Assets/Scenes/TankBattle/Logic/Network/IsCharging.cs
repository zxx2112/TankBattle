using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace TankBattle
{
    public class IsCharging : NetworkBehaviour
    {
        [SyncVar]
        public bool value;
    }
}


