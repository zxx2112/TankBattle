using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Kinematic2D.Core;



namespace Lightbug.Kinematic2D.Implementation
{



[AddComponentMenu("Kinematic2D/Implementation/Character Controller 2D")]
public sealed class CharacterController2D : CharacterMotor
{	
	[Header("Animation")]

	[SerializeField]
	CharacterAnimation characterAnimation = null;


	CharacterBrain characterBrain;
	

	StateMachineController<MovementState> movementController = new StateMachineController<MovementState>();
	public StateMachineController<MovementState> MovementController
	{
		get
		{
			return movementController;
		}
	}

	StateMachineController<PoseState> poseController = new StateMachineController<PoseState>();
	public StateMachineController<PoseState> PoseController
	{
		get
		{
			return poseController;
		}
	}
	
	

	List<CharacterAbility> abilitiesList = new List<CharacterAbility>();

	
	// Movement State -------------------------------------------------------------------------------------------
	public MovementState CurrentMovementState
	{
		get
		{ 
			return movementController.CurrentState;
		} 
	}  

	public MovementState PreviousMovementState
	{
		get
		{ 
			return movementController.PreviousState;
		} 
	} 

	// Pose State -------------------------------------------------------------------------------------------------
	public PoseState CurrentPoseState
	{
		get
		{ 
			return poseController.CurrentState;
		} 
	} 

	public PoseState PreviousPoseState
	{
		get
		{ 
			return poseController.PreviousState;
		} 
	} 

	//--------------------------------------------------------------------------------------------------------------
	
		
	
	
	protected override void Awake()
	{          
		
		base.Awake();
			
		characterBrain = GetComponent<CharacterBrain>();
		if( characterBrain == null )
			Debug.Log( "\"CharacterBrain\" component is missing." );
			
				
		movementController.Initialize( gameObject );
		poseController.Initialize( gameObject );		
         
		InitializeAbilities();
	}	

			
	protected override void UpdateBehaviour( float dt )
	{		
		UpdateDynamicGround();

		UpdateAbilities( dt );
		
		base.UpdateBehaviour( dt );	

		if( characterAnimation != null )
			characterAnimation.UpdateAnimations( dt );

		if( characterBrain != null )
			if( !characterBrain.IsAI())
				characterBrain.ResetActions();

	}
	
		
	void UpdateAbilities( float dt )
	{

		for (int i = 0; i < abilitiesList.Count; i++)
		{
			if( abilitiesList[i] == null || !abilitiesList[i].isEnabled )
				continue;

						
			RunAbility( abilitiesList[i] , dt );			

		}       

	}

	void InitializeAbilities()
	{
		CharacterAbility[] abilitiesArray = GetComponents<CharacterAbility>();
		for (int i = 0; i < abilitiesArray.Length ; i++)
		{
			abilitiesList.Add( abilitiesArray[i] );
		}

	}

	void RunAbility( CharacterAbility ability , float dt )
	{
		if( !ability.isEnabled )
			return;

		ability.Process( dt );
		
	}
}


}
