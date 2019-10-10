using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PlayerNameComponentProxy : MonoBehaviour, IConvertGameObjectToEntity
{

    public string PlayerName;
    

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

        dstManager.AddSharedComponentData(entity, new PlayerNameComponent { Value = PlayerName});
       
    }
}
