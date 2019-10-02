using System;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using VisualScripting.Entities.Runtime;
using System.Collections.Generic;
[Serializable, ComponentEditor]
public struct PlayerViewPrefab : ISharedComponentData, IEquatable<PlayerViewPrefab>
{
    public Unity.Entities.Entity Prefab;
    public bool Equals(PlayerViewPrefab other)
    {
        return Prefab == other.Prefab;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        if (!ReferenceEquals(Prefab, null))
            hash ^= Prefab.GetHashCode();
        return hash;
    }
}

[AddComponentMenu("Visual Scripting Components/PlayerViewPrefab")]
class PlayerViewPrefabProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public UnityEngine.GameObject Prefab = null;

    public void Convert(Unity.Entities.Entity entity, Unity.Entities.EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData(entity, new PlayerViewPrefab { Prefab = conversionSystem.GetPrimaryEntity(Prefab) });
    }

    public void DeclareReferencedPrefabs(List<UnityEngine.GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}