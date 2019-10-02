using JetBrains.Annotations;
using UnityEngine;

namespace VisualScripting.Entities.Runtime
{
    [PublicAPI]
    public struct Wait : ICoroutine
    {
        public float Time;
        public float DeltaTime { get; set; }

        public bool MoveNext()
        {
            Time -= DeltaTime;
            return Time > 0;
        }
    }
}
