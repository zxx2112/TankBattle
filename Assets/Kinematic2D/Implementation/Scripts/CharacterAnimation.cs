using UnityEngine;
using Lightbug.Kinematic2D.Core;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Implementation
{

[AddComponentMenu("Kinematic2D/Implementation/Animation/CharacterAnimation( Animator )")]
public class CharacterAnimation : MonoBehaviour
{	
	
	[Header("State names")]

	[SerializeField]string slopeSlideName = "SlopeSlide";
	[SerializeField] string groundName = "Ground";
	[SerializeField] string airName = "Air";
	[SerializeField] string dashName = "Dash";
	[SerializeField] string wallSlideName = "WallSlide";
	[SerializeField] string jetPackName ="JetPack";
	[SerializeField] string crouchName = "Crouch";

	[Header("Blend tree parameters")]

	[SerializeField] 
	string airBlendName = "AirBlend";

	[SerializeField] 
	string groundBlendName = "GroundBlend";

	[Range(0.1f , 1f)]
	[SerializeField] float airBlendSensitivity = 0.4f;

	[Range_NoSlider(true)]
	[SerializeField] float groundBlendLerpFactor = 10f;


	CharacterController2D characterController2D;	
	CharacterMotor characterMotor;
	CharacterBrain characterBrain;
		
	
	float groundBlendValue = 0f;

	Animator animator;
	

	protected virtual void Awake()
	{		
		Transform parent = transform.parent; 	

		
		characterMotor = parent.GetComponent<CharacterMotor>();
		if( characterMotor == null )
			Debug.Log( "\"CharacterMotor\" component is missing, Does the root object contain this component?" );
		
		characterController2D = parent.GetComponent<CharacterController2D>();
		if( characterController2D == null )
			Debug.Log( "\"CharacterController2D\" component is missing, Does the parent contain this component?" );
		
		characterBrain = parent.GetComponent<CharacterBrain>();
		if( characterBrain == null )
			Debug.Log( "\"CharacterBrain\" component is missing, Does the parent contain this component?" );

		
		animator = gameObject.GetOrAddComponent<Animator>();

		if(animator == null)
			Debug.Log("The Animator component is Missing");
		else if(animator.runtimeAnimatorController == null)
				Debug.Log("The Runtime animator controller is Missing");
				
	}
	
	
		
	public void UpdateAnimations( float dt )
	{		
		if( characterController2D.PoseController.isCurrentlyOnState( PoseState.Crouch ))
		{
			if( !isCurrentlyOnState( crouchName ) )
				PlayAnimation( crouchName );
			
			return;
		}
		
		switch (characterController2D.MovementController.CurrentState )
		{
		    	case MovementState.Normal:

				if(characterMotor.IsGrounded)
				{
					if(!characterMotor.IsOnStableGround && characterMotor.IsSliding )
					{	
						if( !isCurrentlyOnState( slopeSlideName ) )
				 			PlayAnimation( slopeSlideName );					
					}
					else
					{						
						if( !isCurrentlyOnState( groundName ) )
						{
				 			PlayAnimation( groundName );
							groundBlendValue = 0f;
						}
					}
					
				}
				else
				{
					if( !isCurrentlyOnState( airName ) )
				 		PlayAnimation( airName );
						
				}


				
				float airBlendValue = airBlendSensitivity * characterMotor.Velocity.y;

				if( characterBrain != null )
				{
					float targetGroundBlendValue = characterBrain.CharacterAction.left || characterBrain.CharacterAction.right ? 1f : 0f;				
					groundBlendValue = Mathf.Lerp( groundBlendValue , targetGroundBlendValue , groundBlendLerpFactor * dt );
				}
				else
				{
					groundBlendValue = 0;
				}

				UpdateBlendTreeValues( 
					airBlendName , 
					groundBlendName , 
					airBlendValue , 
					groundBlendValue  
				);

		    		break;

		    	case MovementState.Dash:

				if( !isCurrentlyOnState( dashName ) )
				 	PlayAnimation( dashName );

		    		break;
		    
		    	case MovementState.WallSlide:

				if( !isCurrentlyOnState( wallSlideName ) )
				 	PlayAnimation( wallSlideName );
				
		    		break;		    
		    
		    	case MovementState.JetPack:

				if( !isCurrentlyOnState( jetPackName ) )
				 	PlayAnimation( jetPackName );

		    		break;
		    
		}
		
	}	

	// Virtual methods -----------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// Checks if the current animation state is equal to a given state.
	/// </summary>
	/// <param name="stateName">Name of the state.</param>
	/// <returns></returns>
	protected virtual bool isCurrentlyOnState( string stateName )
	{
		return animator.GetCurrentAnimatorStateInfo(0).IsName( stateName );
	}

	/// <summary>
	/// Checks if the current animation state is equal to a given state.
	/// </summary>
	/// <param name="stateName">Name of the state.</param>
	/// <returns></returns>
	protected virtual void PlayAnimation( string animationName )
	{
		animator.Play( animationName );
	}

	/// <summary>
	/// Sends the blend values to the blend tree.
	/// </summary>
	/// <param name="airBlendName">Name of the blend variable from the Air blend tree.</param>
	/// <param name="groundBlendName">Name of the blend variable from the Ground blend tree.</param>
	/// <param name="airBlendValue">Value of the blend variable from the Air blend tree.</param>
	/// <param name="groundBlendValue">Value of the blend variable from the Ground blend tree.</param>
	protected virtual void UpdateBlendTreeValues( string airBlendName , string groundBlendName , float airBlendValue , float groundBlendValue )
	{
		animator.SetFloat( airBlendName , airBlendValue );
		animator.SetFloat( groundBlendName , groundBlendValue );
	}


}

}
