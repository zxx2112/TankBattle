using UnityEngine;
using Lightbug.Kinematic2D.Core;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Implementation
{

[AddComponentMenu("Kinematic2D/Implementation/Abilities/Wall Alignment")]
public class WallAlignment : CharacterAbility
{
	#region Events		

	public event CharacterAbilityEvent OnWallAlignmentPerformed;	
	
	#endregion


	[SerializeField]
	LayerMask wallLayerMask = 0;

	CollisionHitInfo hitInfo = new CollisionHitInfo();
     
	public override void Process(float dt)
	{
		if( !movementController.isCurrentlyOnState( MovementState.Normal ) )
			return;		
		
		if( !characterController2D.IsGrounded )
			return;	
		
		hitInfo.Reset();		
		
		CardinalCollisionType cardinalCollisionType = 
		characterController2D.IsFacingRight ? 
		CardinalCollisionType.Right : 
		CardinalCollisionType.Left;


		float skin = characterBody.SkinWidth;
		
		hitInfo = characterController2D.CharacterCollisions.CardinalCollision( 
			cardinalCollisionType , 
			skin , 
			skin , 
			wallLayerMask
		);

		if( hitInfo.collision )
		{			 
			float wallSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( characterController2D.transform.up , hitInfo.normal , characterController2D.transform.forward );
			float wallAngle = Mathf.Abs( wallSignedAngle );
			
			if(Lightbug.CoreUtilities.Utilities.isCloseTo( wallAngle , 90f , 0.1f ) )
			{
				characterController2D.Teleport( hitInfo.point , Quaternion.LookRotation( Vector3.forward , hitInfo.normal ) );
			
				if( OnWallAlignmentPerformed != null )
					OnWallAlignmentPerformed();
			}	
			
		}
	}

	
	
	
}

}