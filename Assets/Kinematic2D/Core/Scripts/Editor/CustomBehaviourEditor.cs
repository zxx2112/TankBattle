using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lightbug.Kinematic2D.Implementation
{
	using Lightbug.Kinematic2D.Core;

// [ CustomEditor(typeof(CharacterController2D)) , CanEditMultipleObjects ]
// public class CharacterController2DEditor : CustomBehaviourEditor 
// {


// 	protected override void DrawInspector()
// 	{
// 		base.DrawInspector();
// 	}

// 	public override void OnInspectorGUI()
// 	{
// 		DrawInspector();
		
// 	}
// }

[CustomEditor(typeof(MotionCustomBehaviour)) , CanEditMultipleObjects ]
public class CustomBehaviourEditor : Editor 
{
		
	public override void OnInspectorGUI()
	//protected virtual void DrawInspector()
	{
		GUILayout.BeginVertical("Box");
		EditorGUILayout.HelpBox(
			"Make sure the 'Update method' and the 'Motion mode' in the character motor component correspond each other." + 
			"\nFor a Rigidbody motion mode (interpolated or non interpolated) choose the Fixed update method, otherwise you " +
			"can select the Update/LateUpdate method of movement.",
			 MessageType.Info);
		
		DrawDefaultInspector();
		
		GUILayout.EndVertical();
	}

	
}



}

