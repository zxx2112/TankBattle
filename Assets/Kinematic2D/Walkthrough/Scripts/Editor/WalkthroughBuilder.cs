using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Lightbug.Kinematic2D.Walkthrough
{

public class WalkthroughBuilder : EditorWindow
{
	const string Path = "Assets/Kinematic2D/Walkthrough/Scenes";

    	List<SceneAsset> sceneAssets = new List<SceneAsset>();

	
   	[MenuItem("Window/Kinematic 2D/Walkthrough Builder")]
    	public static void ShowWindow()
	{        
		EditorWindow.GetWindow( typeof(WalkthroughBuilder) , false , "Workthrough Builder" );
	}
	
	void OnGUI()
	{
		EditorGUILayout.HelpBox( 
			"With this editor you will be able to add the walkthrough scenes to the build settings. This is necesary to use the UI in the walkthrough scenes in order to skip from one scene to another.\n"
			+ "Of course you could do a build and play through a Desktop application.\n" + 
			"The main walkthought manager object is placed in the first scene ( using DontDestroyOnLoad), " + 
			"if you want to use this walkthrought UI start from there."

			, MessageType.Info 
		);

		GUI.enabled = false;
		EditorGUILayout.LabelField( Path + "/..." );
		GUI.enabled = true;

		if( GUILayout.Button( "Add scenes to build" ) )
		{
			Build();
		}

		EditorGUILayout.HelpBox( 
			"This action will remove all the current scenes from your build settings and " +
			"replaced them with the walkthrough scenes" 
			, MessageType.Warning 
		);
	}

	void Build()
	{
		sceneAssets.Clear();

		List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();

		string[] guids = AssetDatabase.FindAssets( 
			"t:Scene" , 
			new[]
			{ 
				Path
			}
		);
		
		Debug.Log( guids.Length + " scenes added to the build settings scene list.");
		
		foreach (var guid in guids)
		{
		    string assetPath = AssetDatabase.GUIDToAssetPath( guid );
		    sceneAssets.Add ((SceneAsset)AssetDatabase.LoadAssetAtPath( assetPath , typeof(SceneAsset) ) );
		}
		

		foreach (var sceneAsset in sceneAssets)
		{
			string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
			if (!string.IsNullOrEmpty(scenePath))
				editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
		}

		EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
	}
	
	

}

}