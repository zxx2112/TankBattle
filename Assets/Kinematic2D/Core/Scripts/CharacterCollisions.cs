using UnityEngine;
using System.Text;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Core
{



/// <summary>
/// Directions of the cardinal collision method.
/// </summary>
public enum CardinalCollisionType
{
	Up, 
	Down, 
	Left, 
	Right
}




public struct GroundDetectionRay
{
	public bool collision;
	public Vector3 point;
	public float distance;
	public Vector3 normal;
	public float verticalSlopeSignedAngle;
	public float verticalSlopeAngle;

	public bool stable;

	public void Reset()
	{
		collision = false;
		point = Vector3.zero;
		distance = 0;
		normal = Vector3.up;
		verticalSlopeSignedAngle = 0;
		verticalSlopeAngle = 0;

		stable = false;
	}
}

public struct GroundAlignmentResult
{	
	public GroundDetectionRay leftRay;
	public GroundDetectionRay rightRay;

	public void Reset()
	{		
		leftRay.Reset();
		rightRay.Reset();

	}
}

[RequireComponent( typeof( CharacterBody ) )]
[RequireComponent( typeof( CharacterMotor ) )]
public class CharacterCollisions : MonoBehaviour
{
	const int BufferSize = 10;

	CharacterBody characterBody = null;
	CharacterMotor characterMotor = null;

	bool is3D = false;
		
	// Collider2D[] Colliders2D = new Collider2D[ BufferSize ]; 
	// Collider[] Colliders3D = new Collider[ BufferSize ]; 

	RaycastHit2D[] results2D = new RaycastHit2D[ BufferSize ]; 
	RaycastHit[] results3D = new RaycastHit[ BufferSize ]; 


	void Awake()
	{
		hideFlags = HideFlags.HideInInspector;

		characterBody = GetComponent<CharacterBody>();
		characterMotor = GetComponent<CharacterMotor>();		
		is3D = characterBody.GetComponent<CharacterBody3D>() != null;		
	
	}

			
	/// <summary>
	/// Performs a collision test in any of the four cardinal directions.
	/// </summary>
	public CollisionHitInfo CardinalCollision( CardinalCollisionType cardinalCollisionType, float skin , float extraDistance , LayerMask layerMask)
	{
		
		CollisionHitInfo hitInfo = new CollisionHitInfo();		
		hitInfo.Reset();	

		Vector3 origin = Vector3.zero;

		float castDistance = skin + extraDistance;

		Vector3 direction = Vector3.zero;

		Vector3 center = characterBody.bodyTransform.Position + characterBody.bodyTransform.Up * characterBody.heightExtents;

		switch(cardinalCollisionType)
		{
			case CardinalCollisionType.Up:
				direction = characterBody.bodyTransform.Up;
				origin = center + direction * (characterBody.height/2 - skin);
			break;
			case CardinalCollisionType.Down:
				direction = - characterBody.bodyTransform.Up;
				origin = center + direction * (characterBody.height/2 - skin);
			break;
			case CardinalCollisionType.Left:
				direction = - characterBody.bodyTransform.Right;
				origin = center + direction * (characterBody.width/2 - skin);
			break;
			case CardinalCollisionType.Right:
				direction = characterBody.bodyTransform.Right;
				origin = center + direction * (characterBody.width/2 - skin);
			break;
		}


		
		hitInfo = PhysicsUtilities.Raycast(
			is3D ,
			origin ,
			direction , 
			castDistance , 
			layerMask
		);					

		

		return hitInfo;
	}
	


	/// <summary>
	/// Performs a collision detection in the horizontal grounded direction.
	/// </summary>
	public CollisionHitInfo HorizontalGroundedCollision( Vector3 castDirection , bool positiveDirection , float movementAmount ,  LayerMask layerMask )
	{
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();		

		if(movementAmount < 0)
			return hitInfo;
		
		float castDistance = characterBody.SkinWidth + movementAmount;						


		hitInfo = PhysicsUtilities.RaycastSweep( 
			characterBody.Is3D() ,
			positiveDirection ? characterBody.bottomRightCollision_StepOffset : characterBody.bottomLeftCollision_StepOffset ,
			positiveDirection ? characterBody.topRightCollision : characterBody.topLeftCollision ,
			characterBody.HorizontalRays ,
			castDirection , 
			castDistance , 
			RaySelectionRule.ShortestNonZero ,
			layerMask
		);

		return hitInfo;
	}

	/// <summary>
	/// Performs a collision detection in the horizontal direction, considering the whole character characterBody.
	/// </summary>
	public CollisionHitInfo HorizontalNotGroundedCollision( float deltaMovement , LayerMask layerMask)
	{ 
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();		

		float movementSign = Mathf.Sign( deltaMovement );
		float movementAmount = Mathf.Abs( deltaMovement );		

		float castDistance = characterBody.SkinWidth + movementAmount;						
		Vector3 castDirection = movementSign * characterBody.bodyTransform.Right;
		

		Vector3 boxCenter = characterBody.center + 
		movementSign * characterBody.bodyTransform.Right * ( characterBody.widthExtents - characterBody.SkinWidth - (characterBody.BoxThickness)/2 );						
					
		hitInfo = PhysicsUtilities.Boxcast( 
			is3D ,
			boxCenter , 
			characterBody.horizontalBoxSize,
			castDirection ,
			castDistance ,
			characterBody.bodyTransform.Up ,
			layerMask
		);

		return hitInfo;

		



	}

	/// <summary>
	/// Performs a collision detection in the vertical direction, considering the whole character characterBody.
	/// </summary>
	public CollisionHitInfo VerticalNotGroundedCollision( float deltaMovement , LayerMask layerMask )
	{
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();

		float movementSign = Mathf.Sign( deltaMovement );
		float movementAmount = Mathf.Abs( deltaMovement );
		
		float castDistance = characterBody.SkinWidth + movementAmount;	
		Vector3 castDirection = movementSign * characterBody.bodyTransform.Up;	
		

		Vector3 boxCenter = characterBody.center + 
		movementSign * characterBody.bodyTransform.Up * ( characterBody.heightExtents - characterBody.SkinWidth - characterBody.BoxThickness/2 );
		
		hitInfo = PhysicsUtilities.BoxCastAll( 
			is3D ,
			boxCenter , 
			characterBody.verticalBoxSize ,
			castDirection , 
			castDistance , 
			characterBody.bodyTransform.Up ,
			results2D ,
			results3D ,
			layerMask
		);

		return hitInfo;

	}

	/// <summary>
	/// Performs a collision test towards the ground.
	/// </summary>
	public CollisionHitInfo ProbeGroundCollision( float groundClampingDistance , LayerMask layerMask )
	{
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();		

		if( groundClampingDistance < 0)
			return hitInfo;
		
		float castDistance = characterBody.StepOffset + groundClampingDistance;
		
		
		// BOXCAST ----------------------------------------------------------------------------------------------------------------
		Vector3 boxCenter = characterBody.bodyTransform.Position + 
		characterBody.bodyTransform.Up * ( characterBody.StepOffset + characterBody.BoxThickness/2 );
	
		hitInfo = PhysicsUtilities.Boxcast( 
			is3D ,
			boxCenter , 
			characterBody.verticalBoxSize ,
			- characterBody.bodyTransform.Up ,
			castDistance , 
			characterBody.bodyTransform.Up ,
			layerMask
		);


		return hitInfo;		
		

	}

	/// <summary>
	/// Performs a collision test in the horizontal direction (depending of the mode selected) in order to
	/// depenetrate the character from moving colliders.
	/// </summary>
	public CollisionHitInfo HorizontalDepenetrationCollision( bool grounded , bool positiveDirection , LayerMask layerMask)
	{ 
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();


		float xDirection = positiveDirection ? 1 : -1;	
		Vector3 castDirection = xDirection * characterBody.bodyTransform.Right;
		float castDistance = characterBody.width - characterBody.SkinWidth - characterBody.BoxThickness;	

		hitInfo.distance = castDistance;

		Vector3 boxCenter = Vector3.zero;		
		if( grounded )
		{
			boxCenter = positiveDirection ?
			characterBody.bottomLeftCollision_StepOffset + 
			characterBody.bodyTransform.Up * ( characterBody.height - characterBody.StepOffset - characterBody.SkinWidth ) / 2 + 
			characterBody.bodyTransform.Right * ( characterBody.BoxThickness / 2 ) :
			characterBody.bottomRightCollision_StepOffset + 
			characterBody.bodyTransform.Up * ( characterBody.height - characterBody.StepOffset - characterBody.SkinWidth ) / 2 - 
			characterBody.bodyTransform.Right * ( characterBody.BoxThickness / 2 );	
		}
		else
		{				
			boxCenter = positiveDirection ?
			characterBody.middleLeftCollision + characterBody.bodyTransform.Right * ( characterBody.BoxThickness / 2 ) :
			characterBody.middleRightCollision - characterBody.bodyTransform.Right * ( characterBody.BoxThickness / 2 );	
		}
						
		hitInfo = PhysicsUtilities.Boxcast( 
			is3D ,
			boxCenter , 
			characterBody.horizontalBoxSize,
			castDirection ,
			castDistance ,
			characterBody.bodyTransform.Up ,
			layerMask
		);		
					
		return hitInfo;

	}

	/// <summary>
	/// Performs a collision test in the horizontal direction (depending of the mode selected) in order to
	/// depenetrate the character from moving colliders.
	/// </summary>
	public CollisionHitInfo VerticalDepenetrationCollision( bool positiveDirection , LayerMask layerMask )
	{ 
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();

		float yDirection = positiveDirection ? 1 : -1;

		Vector3 castDirection = yDirection * characterBody.bodyTransform.Up;				
		float castDistance = characterBody.height - characterBody.SkinWidth - characterBody.BoxThickness;		
		
		// BOXCAST -------------------------------------------------------------------------------------------------------------------
		Vector3 boxCenter = positiveDirection ?
			characterBody.middleBottomCollision + characterBody.bodyTransform.Up * ( characterBody.BoxThickness / 2 ) :
			characterBody.middleTopCollision - characterBody.bodyTransform.Up * ( characterBody.BoxThickness / 2 ); 
		

		hitInfo = PhysicsUtilities.Boxcast( 
			is3D ,
			boxCenter , 
			characterBody.verticalBoxSize,
			castDirection ,
			castDistance ,
			characterBody.bodyTransform.Up ,
			layerMask
		);
		
					
		return hitInfo;

	}


	/// <summary>
	/// Performs the collision detection method used to align the character to the ground.
	/// </summary>
	public GroundAlignmentResult GroundRaysCollisions( float skin, float extraDistance , float maxSlopeAngle , LayerMask layerMask )
	{		
		GroundAlignmentResult result = new GroundAlignmentResult();
		result.Reset();

		CollisionHitInfo leftHitInfo;
		CollisionHitInfo rightHitInfo;
		
		
		if(extraDistance < 0)
			return result;
		
		float castDistance = skin + extraDistance;
		
		Vector3 leftRayOrigin = characterBody.bodyTransform.Position - characterBody.bodyTransform.Right * characterBody.verticalArea/2 + characterBody.bodyTransform.Up * skin;
		Vector3 rightRayOrigin = characterBody.bodyTransform.Position + characterBody.bodyTransform.Right * characterBody.verticalArea/2 + characterBody.bodyTransform.Up * skin;
			
		
		leftHitInfo = PhysicsUtilities.Raycast( 
			is3D ,
			leftRayOrigin , 
			- characterBody.bodyTransform.Up ,
			castDistance,
			layerMask
		);

		rightHitInfo = PhysicsUtilities.Raycast( 
			is3D ,
			rightRayOrigin , 
			- characterBody.bodyTransform.Up ,
			castDistance,
			layerMask
		);

		
		float leftLocalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle(characterBody.bodyTransform.Up , leftHitInfo.normal , characterBody.bodyTransform.Forward );
		float leftLocalSlopeAngle = Mathf.Abs(leftLocalSlopeSignedAngle);

		float rightLocalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle(characterBody.bodyTransform.Up , rightHitInfo.normal , characterBody.bodyTransform.Forward );
		float rightLocalSlopeAngle = Mathf.Abs(rightLocalSlopeSignedAngle);

	
		
		if( 	( leftHitInfo.collision && Lightbug.CoreUtilities.Utilities.isCloseTo( leftLocalSlopeAngle , 90 , 0.01f ) ) || 
			( rightHitInfo.collision && Lightbug.CoreUtilities.Utilities.isCloseTo( rightLocalSlopeAngle , 90 , 0.01f ) ) )
			return result;


		// Left result
		result.leftRay.collision = leftHitInfo.collision;
		if( leftHitInfo.collision )
		{
			result.leftRay.point = leftHitInfo.point;
			result.leftRay.distance = leftHitInfo.distance;
			result.leftRay.normal = leftHitInfo.normal;
			result.leftRay.verticalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( characterMotor.CurrentVerticalDirection , leftHitInfo.normal , characterBody.bodyTransform.Forward );
			result.leftRay.verticalSlopeAngle = Mathf.Abs( result.leftRay.verticalSlopeSignedAngle );
			result.leftRay.stable = result.leftRay.verticalSlopeAngle <= maxSlopeAngle;
		}
		else
		{
			result.leftRay.Reset();
		}
		

		// Right result
		result.rightRay.collision = rightHitInfo.collision;
		if( rightHitInfo.collision )
		{
			result.rightRay.point = rightHitInfo.point;
			result.rightRay.distance = rightHitInfo.distance;
			result.rightRay.normal = rightHitInfo.normal;
			result.rightRay.verticalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( characterMotor.CurrentVerticalDirection , rightHitInfo.normal , characterBody.bodyTransform.Forward );
			result.rightRay.verticalSlopeAngle = Mathf.Abs( result.rightRay.verticalSlopeSignedAngle );
			result.rightRay.stable = result.rightRay.verticalSlopeAngle <= maxSlopeAngle;
		}
		else
		{
			result.rightRay.Reset();
		}

		return result;

	}

			


}

}


