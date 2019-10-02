using UnityEngine;
using UnityEditor;

namespace Lightbug.Kinematic2D.Core
{

[ CustomEditor( typeof(CharacterMotor) , true ) , CanEditMultipleObjects ]
public class CharacterMotorEditor : Editor
{
     CharacterMotor monobehaviour;


	void OnEnable()
	{
		monobehaviour = target as CharacterMotor;
	}

	void OnSceneGUI()
	{	
          if( monobehaviour.CharacterBody != null ) 			
		     monobehaviour.CharacterBody.bodyTransform.SetTransform( monobehaviour.transform );
	}
	

}

}
