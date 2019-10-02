using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.IO;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Undying
{
    public class SceneLoader : MonoBehaviour
    {
        [ValueDropdown("GetListOfScenes")]
        [SerializeField] private string scene = "";
        

        [ShowInInspector]
        [ReadOnly]
        public string SceneName => Path.GetFileNameWithoutExtension(scene);

        private void Awake() {
            ManualLoad();
        }

        [Button]
        private void ManualLoad() {

#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorLoadScene();
            else
                RealGameLoadScene();
#else
            RealGameLoadScene();
#endif
        }

        private void RealGameLoadScene() {
            var scene = SceneManager.GetSceneByName(SceneName);

            //Assert.IsTrue(scene.IsValid(), $"场景名[{SceneName}]无效");

            var isLoaded = scene.isLoaded;
            if (!isLoaded) {
                SceneManager.LoadScene(SceneName, LoadSceneMode.Additive);
            }
        }

#if UNITY_EDITOR
        private IEnumerable<string> GetListOfScenes() {
            var allAssets = AssetDatabase.GetAllAssetPaths();
            Array.Sort(allAssets);
            Array.Reverse(allAssets);
            return allAssets.Where(x => !x.StartsWith("Packages")).Where(x => x.IndexOf(".unity") != -1);
        }

        private void EditorLoadScene() {
            var isLoaded = EditorSceneManager.GetSceneByName(SceneName).isLoaded;
            if (!isLoaded) {
                EditorSceneManager.OpenScene(scene, OpenSceneMode.Additive);
            }
        }
    }
#endif

}

