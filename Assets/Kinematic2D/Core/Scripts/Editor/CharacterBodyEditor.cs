using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lightbug.Kinematic2D.Core
{

[ CustomEditor(typeof(CharacterBody) , true ) , CanEditMultipleObjects ]
public class CharacterBodyEditor : Editor
{

	CharacterBody monobehaviour;

	void OnEnable()
	{
		
		if(monobehaviour == null)
			monobehaviour = (CharacterBody)target;

		monobehaviour.bodyTransform.SetTransform( monobehaviour.transform );

		
	}
	


	void OnSceneGUI()
	{		
		
		if (!monobehaviour.DrawCollisionShape)
			return;		
		
		monobehaviour.bodyTransform.SetTransform( monobehaviour.transform );
	}

	
}

}



