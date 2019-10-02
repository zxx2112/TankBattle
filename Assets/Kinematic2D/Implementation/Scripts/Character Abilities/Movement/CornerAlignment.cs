using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Kinematic2D.Core;

namespace Lightbug.Kinematic2D.Implementation
{

public struct CornerAlignmentResult
{
	public bool success;
	public Quaternion normalRotation;
	public Vector3 point;
	
	public void Reset()
	{
		success = false;
		normalRotation = Quaternion.identity;
		point = Vector3.zero;
	}
}


[AddComponentMenu("Kinematic2D/Implementation/Abilities/Corner Alignment")]
public class CornerAlignment : CharacterAbility
{
	#region Events	

	public event CharacterAbilityEvent OnCornerAlignmentPerformed;
	
	#endregion
     
	[SerializeField] LayerMask layerMask = 0;
	public LayerMask LayerMask
	{ 
		get
		{ 
			return layerMask; 
		} 
	}


	[Tooltip("Length of the ray fired by the raycast method.")]
	[Range(0.01f,10f)] [SerializeField] float cornerDetectionDistance = 1;

	bool wasGrounded = false;

     public override void Process(float dt)
     {
		if( !movementController.isCurrentlyOnState( MovementState.Normal ) )
			return;		
		
		if( !characterController2D.IsGrounded && wasGrounded )
		{

			if( characterController2D.Velocity.y <= 0 )
				CornerAlign();
		}

		wasGrounded = characterController2D.IsGrounded;
		
     }

	
	void CornerAlign()
	{		
		
		CornerAlignmentResult result = CornerAlignCollisions( 
			characterController2D.Velocity.x > 0 , 
			cornerDetectionDistance ,
			layerMask
		);

		if( result.success )
		{
			characterController2D.Teleport( result.point , result.normalRotation );

			if( OnCornerAlignmentPerformed != null )
				OnCornerAlignmentPerformed();
		}
			
		
	
	}

	/// <summary>
	/// Performs the collision detection method used in the "Corner Alignment" feature.
	/// </summary>
	CornerAlignmentResult CornerAlignCollisions( bool positiveDirection , float cornerDetectionDistance , LayerMask layerMask )
	{
		CornerAlignmentResult result = new CornerAlignmentResult();
		result.Reset();

		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();
				
		if(cornerDetectionDistance < 0)
			return result;
		
		float castDistance = characterBody.SkinWidth + cornerDetectionDistance;
		
					
		Vector2 rayOrigin = characterController2D.transform.position + 
		(positiveDirection ? -1 : 1) * characterController2D.transform.right * characterBody.verticalArea/2 - 
		characterController2D.transform.up * characterBody.SkinWidth;

		hitInfo = PhysicsUtilities.Raycast(
			characterBody.Is3D() , 
			rayOrigin , 
			(positiveDirection ? -1 : 1) * characterController2D.transform.right ,
			castDistance,
			layerMask
		);

		if( hitInfo.collision )
		{
			result.point = hitInfo.point;
			result.normalRotation = Quaternion.LookRotation( characterBody.bodyTransform.Forward , hitInfo.normal );
			result.success = true;
		}
			
						
			
		return result;
	}

	public override string GetInfo()
	{ 
		return "Allows the character to align itself with the outline of the ground [It requires an extra raycast]."; 
	}

	
	
}

}