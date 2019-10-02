//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Sirenix.OdinInspector;
//using System.Linq;
//using UnityAtoms;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//namespace TankBattle.Utils
//{
//    [CreateAssetMenu(menuName = "TankBattle/SceneCollector")]
//    public class SceneCollector : ScriptableObject
//    {
//        [ReadOnly]
//        public List<StringConst> sceneNames;

//#if UNITY_EDITOR
//        public List<SceneAsset> scenes;

//        [Button("Convert")]
//        public void Convert() {
//            sceneNames.Clear();
//            //清除所有子物体
//            var allObject = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
//            foreach (var obj in allObject.Where(x => x != this)) {
//                AssetDatabase.RemoveObjectFromAsset(obj);
//            }

//            sceneNames.AddRange(scenes.Select(x => {
//                var instance = ScriptableObject.CreateInstance<StringConst>();
//                //instance.IsReadOnly = true;
//                instance.value = x.name;
//                instance.name = instance.value;
//                AssetDatabase.AddObjectToAsset(instance, this);//成为子物体
//                return instance;
//            } ));
//            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
//            EditorUtility.SetDirty(this);
//        }

//#endif

//    }

//}

