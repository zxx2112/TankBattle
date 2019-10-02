using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Kinematic2D.Core;


namespace Lightbug.Kinematic2D.Implementation
{

public enum ActionInputType
{
	GetAxis ,
	GetAxisRaw ,
	GetInput ,
	GetInputDown ,
	GetInputUp
}

[System.Obsolete("This component is obsolete" )]
[AddComponentMenu("Kinematic2D/Implementation/Brain/Human Brain")]
public class CharacterHumanBrain : CharacterBrain
{	
	public CharacterInputData inputData;

	
	public override bool IsAI()
	{
		return false;
	}
	

		
	/// <summary>
	/// Checks for human inputs and updates the action struct.
	/// </summary>
	protected override void Update()
	{		
		if( inputData == null || Time.timeScale == 0 )
			return;

		characterAction.right |= GetAxis( inputData.horizontalAxisName ) > 0;
		characterAction.left |= GetAxis( inputData.horizontalAxisName ) < 0;
		characterAction.up |= GetAxis( inputData.verticalAxisName ) > 0;
		characterAction.down |= GetAxis( inputData.verticalAxisName ) < 0;
		
		characterAction.jumpPressed |= GetButtonDown( inputData.jumpName );
		characterAction.jumpReleased |= GetButtonUp( inputData.jumpName );

		characterAction.dashPressed |= GetButtonDown( inputData.dashName );
		characterAction.dashReleased |= GetButtonUp( inputData.dashName );

		characterAction.jetPack |= GetButton( inputData.jetPackName );

		

	}

	
	protected virtual float GetAxis( string axisName , bool raw = true )
	{
		return raw ? Input.GetAxisRaw( axisName ) : Input.GetAxis( axisName );
	}

	protected virtual bool GetButton( string actionInputName )
	{
		return Input.GetButton( actionInputName );
	}

	protected virtual bool GetButtonDown( string actionInputName )
	{
		return Input.GetButtonDown( actionInputName );
	}

	protected virtual bool GetButtonUp( string actionInputName )
	{
		return Input.GetButtonUp( actionInputName );
	}

	
}

}


