using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lightbug.Kinematic2D.Implementation
{

[AddComponentMenu("Kinematic2D/Implementation/Abilities/Vertical Movement")]
public class VerticalMovement : CharacterAbility
{
	
	#region Events	

	public delegate void NotGroundedJumpDelegate( int airJumpsLeft );
	public delegate void GroundedJumpDelegate();

	public event CharacterAbilityEvent OnJumpPerformed;
	public event CharacterAbilityEvent OnCancelJump;
	public event GroundedJumpDelegate OnGroundedJumpPerformed;
	public event NotGroundedJumpDelegate OnNotGroundedJumpPerformed;
	
	#endregion


	[SerializeField] VerticalMovementProfile verticalData;

	[Tooltip("Number of jumps that the character can make in the air.")]
	public int availableNotGroundedJumps = 1;

	[Header("Jump cancellation")]	

	[Tooltip("If the jump button is released the character will stop jumping.")]
	public bool cancelOnRelease = true;

	[Tooltip("The ratio between 0 (ground) and 1 (maximum height) in which the cancel action will occur.")]
	[Range_NoSlider(true)] public float cancelJumpMinTime = 0.1f;

	[Range_NoSlider(true)] public float cancelJumpMaxTime = 0.2f;

	[Tooltip("The higher is the cancel Jump Factor the faster the chracter will try to return to the ground.")]
	[Range_NoSlider(true)] public float cancelJumpFactor = 0.5f;


	[Header("Slopes")]	

	[Tooltip("Should the character jump exclusively on stable ground?")]
	public bool jumpOnlyOnStableGround = false;

	[Header("Environment")]

	public bool isAffectedByMovementAreas = true;

	VerticalMovementProfile defaultVerticalData;

	GameObject collidedTrigger = null;

	float gravity = 10f;

	
	


	float jumpSpeed;

	int notGroundedJumpsLeft;

	float jumpTimer = 0;
	bool cancelJump = false;

	bool enableJumpTimer = false;
	

	protected override void Awake()
	{
		base.Awake();

		defaultVerticalData = verticalData;
		

		CalculateGravityParameters();

		notGroundedJumpsLeft = availableNotGroundedJumps;

	}
	

	public override void Process( float dt )
	{   	
		if( movementController.isCurrentlyOnState( MovementState.Normal ) )
		{		
		
			ProcessGravity( dt );

			ProcessJump(dt);

			ProcessMovementArea();

		}
		
	}

	public void CalculateGravityParameters()
	{	
		gravity = (2 * verticalData.jumpHeight ) / Mathf.Pow( verticalData.jumpDuration , 2 );
		jumpSpeed = gravity * verticalData.jumpDuration;

		
		
	}

	void ProcessGravity( float dt )
	{
		
		if(characterController2D.IsGrounded)
		{			
			characterController2D.SetVelocityY( 0f );
			ResetNotGroundedJump();		
		}
		else
		{	
			float gravity = characterController2D.Velocity.y < 0 ? this.gravity * verticalData.descendingGravityMultiplier : this.gravity;
			characterController2D.AddVelocityY( - gravity * dt );

			if( characterController2D.IsHead )
			{
				float minimumVelocity = characterController2D.MinimumMovement / dt;
				characterController2D.SetVelocityY( - ( minimumVelocity + 0.001f )  );
			}
		}
	}

	void ProcessJump( float dt )
	{	
		if( characterController2D.IsCrushed )
			return;

		if( characterBrain == null )
			return;
		
		if( !cancelOnRelease )
		{
			if( characterBrain.CharacterAction.jumpPressed )
			{				
				Jump();
			}

			return;
		}
		

		if( characterBrain.CharacterAction.jumpPressed )
		{
			Jump();

			enableJumpTimer = true;
			jumpTimer = 0;
			cancelJump = false;

			return;
		}

		if( characterController2D.Velocity.y <= 0 )
		{
			jumpTimer = 0;
			enableJumpTimer = false;
			cancelJump = false;
			return;
		}

		if( enableJumpTimer )
		{
			jumpTimer += dt;

			
			if( jumpTimer < cancelJumpMinTime )
			{
				if( characterBrain.CharacterAction.jumpReleased )
					cancelJump = true;
				
			} 
			else if( jumpTimer < cancelJumpMaxTime )
			{
				if( characterBrain.CharacterAction.jumpReleased )
				{
					jumpTimer = 0;
					enableJumpTimer = false;					
					cancelJump = true;
				}
				
				
				if( cancelJump )
				{
					CancelJump();
					

					cancelJump = false;
					jumpTimer = 0;
					enableJumpTimer = false;
				}

			}
			
			
		}

		
	}


	void CancelJump()
	{
		characterController2D.SetVelocityY( characterController2D.Velocity.y * cancelJumpFactor );

		if( OnCancelJump != null )
			OnCancelJump();
	}
	
	

	void Jump()
	{			
		
		if( !characterController2D.IsGrounded )
		{
			if( notGroundedJumpsLeft != 0 )
			{
				characterController2D.ForceNotGroundedState();		
				characterController2D.SetVelocityY( jumpSpeed );
				notGroundedJumpsLeft--;	


				if( OnNotGroundedJumpPerformed != null )
					OnNotGroundedJumpPerformed( notGroundedJumpsLeft );		
			}
		}	
		else
		{
			if( jumpOnlyOnStableGround && !characterController2D.IsOnStableGround)
			{
				return;
			}

			characterController2D.ForceNotGroundedState();		
			characterController2D.SetVelocityY( jumpSpeed );

			if( OnGroundedJumpPerformed != null )
				OnGroundedJumpPerformed();	
			
		}

		if( OnJumpPerformed != null )
			OnJumpPerformed();
		
	}
	

	/// <summary>
	/// Resets the number of available air jumps to the initial value.
	/// </summary>
	public void ResetNotGroundedJump()
	{
		notGroundedJumpsLeft = availableNotGroundedJumps;
	}


	void ProcessMovementArea()
	{
		if(!isAffectedByMovementAreas)
			return;

		if( characterController2D.CollidedTrigger == null )
		{
			if(collidedTrigger != null)
			{
				RevertMovementParameters();
				collidedTrigger = null;
			}
			
			
			return;
		}
		else
		{
			
			if( ( collidedTrigger == null ) || 
				( characterController2D.CollidedTrigger != collidedTrigger ))
			{
				MovementArea movementArea = characterController2D.CollidedTrigger.GetComponent<MovementArea>();
				if( movementArea != null )
				{
					SetMovementParameters( movementArea );
				}

				collidedTrigger = characterController2D.CollidedTrigger;
			}

			
		
		}
	}

	

	

	void SetMovementParameters( MovementArea movementArea )
	{
		CharacterMovementProfile data = movementArea.CharacterMovementData;
		if( data == null  )
			return;
		
		if( data.verticalMovementData != null )
		{
			this.verticalData = data.verticalMovementData;
			CalculateGravityParameters();

			characterController2D.SetVelocityY( characterController2D.Velocity.y * data.verticalMovementData.entrySpeedMultiplier );	
			
		}

		
		
	}

	void RevertMovementParameters()
	{	
		this.verticalData = defaultVerticalData;
		CalculateGravityParameters();
		
	}

	
	public override string GetInfo()
	{ 
		return "It handles the vertical movement of the character in normal state (the vertical movement is affected by " + 
		"gravity and jump velocity).";
	}

	
}

}
