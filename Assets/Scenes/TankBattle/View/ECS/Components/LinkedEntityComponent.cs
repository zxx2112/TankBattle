using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct LinkedEntityComponent : IComponentData
{
    public Entity entity;
}
