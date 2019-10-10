using System;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using VisualScripting.Entities.Runtime;
using System.Collections.Generic;
[Serializable, ComponentEditor]
public struct PlayerReadyStateComponent : IComponentData
{
    public bool IsReady;
}

[AddComponentMenu("Visual Scripting Components/PlayerReadyStateComponent")]
class PlayerReadyStateComponentProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public bool IsReady;

    public void Convert(Unity.Entities.Entity entity, Unity.Entities.EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new PlayerReadyStateComponent { IsReady = IsReady });
    }

    public void DeclareReferencedPrefabs(List<UnityEngine.GameObject> referencedPrefabs)
    {
    }
}