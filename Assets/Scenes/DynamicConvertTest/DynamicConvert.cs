using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DynamicConvertTest
{
    public class DynamicConvert : MonoBehaviour
    {
        public GameObject Prefab;

        private void Awake() {
            Convert(Prefab);
        }

        public Entity Convert(GameObject input) {
            //从GameObject prefab到Entity prefab
            var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(input, World.Active);
            return prefab;
        }
    }

}

