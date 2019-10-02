using UnityEngine;
using UnityEditor;

using Lightbug.Kinematic2D.Core;

namespace Lightbug.Kinematic2D.Implementation
{
	


[CustomEditor( typeof(MotionCustomBehaviour) , true ) , CanEditMultipleObjects ]
public class MotionCustomBehaviourEditor : Editor 
{
		
	public override void OnInspectorGUI()
	{
		GUILayout.Space(10f);

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

