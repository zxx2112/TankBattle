using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PlayerNameComponent : ISharedComponentData,IEquatable<PlayerNameComponent>
{
    public string Value;

    public bool Equals(PlayerNameComponent other) {
        return string.Equals(Value, other.Value);
    }

    public override int GetHashCode() {
        return Value.GetHashCode();
    }
}
