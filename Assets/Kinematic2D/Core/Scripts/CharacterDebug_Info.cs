using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lightbug.Kinematic2D.Core
{

[AddComponentMenu("Kinematic2D/Core/Debug/Character Info")]
public class CharacterDebug_Info : MonoBehaviour
{

	CharacterMotor characterMotor;	

	void Awake ()
	{
		characterMotor = GetComponent<CharacterMotor>();		
	}
	

	void OnGUI()
	{		
		GUILayout.BeginVertical("Box" , GUILayout.Width(320));
		GUILayout.Label(characterMotor.GetCollisionInfoString);
		GUILayout.EndVertical();		
	
	}

	
}

}

