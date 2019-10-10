using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class RoomNameComponentProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public string roomName;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData(entity, new RoomNameComponent { Value = roomName});
        //dstManager.AddBuffer<AddPlayerEvent>(entity);
    }
}
