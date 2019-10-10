using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct RoomPlayerSpawnerComponent : IComponentData
{
    public Entity RoomPlayerPrefab;
}


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class RoomPlayerSpawnerProxy : MonoBehaviour, IConvertGameObjectToEntity,IDeclareReferencedPrefabs
{
    public GameObject roomPlayerPrefab;


    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        referencedPrefabs.Add(roomPlayerPrefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new RoomPlayerSpawnerComponent {
            RoomPlayerPrefab = conversionSystem.GetPrimaryEntity(roomPlayerPrefab)
        }); ;
    }

}
