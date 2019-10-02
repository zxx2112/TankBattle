using System;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using VisualScripting.Entities.Runtime;
using System.Collections.Generic;
using TankBattle;

[Serializable, ComponentEditor]
public struct GameStateComponent : IComponentData
{
    public GameState Value;
}

[AddComponentMenu("Visual Scripting Components/GameStateComponent")]
class GameStateComponentProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameState Value = GameState.Moving;

    public void Convert(Unity.Entities.Entity entity, Unity.Entities.EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new GameStateComponent { Value = Value });
    }

    public void DeclareReferencedPrefabs(List<UnityEngine.GameObject> referencedPrefabs)
    {
    }
}