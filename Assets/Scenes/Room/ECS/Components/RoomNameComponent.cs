using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RoomNameComponent : ISharedComponentData, IEquatable<RoomNameComponent>
{
    public string Value;

    public bool Equals(RoomNameComponent other) {
        return string.Equals(Value, other.Value);
    }

    public override int GetHashCode() {
        return Value.GetHashCode();
    }
}
