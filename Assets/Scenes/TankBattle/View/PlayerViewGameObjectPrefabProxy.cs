using System;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using VisualScripting.Entities.Runtime;
using System.Collections.Generic;
[Serializable, ComponentEditor]
public struct PlayerViewGameObjectPrefab : ISharedComponentData, IEquatable<PlayerViewGameObjectPrefab>
{
    public Unity.Entities.Entity PlayerViewPrefab;
    public bool Equals(PlayerViewGameObjectPrefab other)
    {
        return PlayerViewPrefab == other.PlayerViewPrefab;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        if (!ReferenceEquals(PlayerViewPrefab, null))
            hash ^= PlayerViewPrefab.GetHashCode();
        return hash;
    }
}

[AddComponentMenu("Visual Scripting Components/PlayerViewGameObjectPrefab")]
class PlayerViewGameObjectPrefabProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public UnityEngine.GameObject PlayerViewPrefab = null;

    public void Convert(Unity.Entities.Entity entity, Unity.Entities.EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData(entity, new PlayerViewGameObjectPrefab { PlayerViewPrefab = conversionSystem.GetPrimaryEntity(PlayerViewPrefab) });
    }

    public void DeclareReferencedPrefabs(List<UnityEngine.GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(PlayerViewPrefab);
    }
}