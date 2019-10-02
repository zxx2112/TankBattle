using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Core
{



	

/// <summary>
///	Replacement for the RayCastHit and RayCastHit2D structs
/// </summary>
public struct CollisionHitInfo
{
	public bool collision;
	public GameObject gameObject;	
	public float distance;
	public Vector3 point;
	public Vector3 normal;

	public void Reset()
	{
		collision = false;
		gameObject = null;
		distance = 0;
		point = Vector3.zero;
		normal = Vector3.up;
	}

}

public enum RaySelectionRule
{
	Shortest ,
	ShortestNonZero ,
	Longest
}

public static class PhysicsUtilities
{

	public static CollisionHitInfo Raycast( bool is3D , Vector3 origin, Vector3 castDirection , float castDistance , LayerMask layerMask )
	{
		return is3D ? 
		Raycast3D( origin, castDirection , castDistance , layerMask) : 
		Raycast2D( origin, castDirection , castDistance , layerMask);
	}

	public static CollisionHitInfo Boxcast( bool is3D , Vector3 boxCenter , Vector3 boxSize , Vector3 castDirection , float castDistance , Vector3 boxUp , LayerMask layerMask)
	{
		return is3D ? 
		Boxcast3D( boxCenter , boxSize , castDirection , castDistance , boxUp , layerMask) : 
		Boxcast2D( boxCenter , boxSize , castDirection , castDistance , boxUp , layerMask);
	}

	public static CollisionHitInfo BoxCastAll( bool is3D , Vector3 boxCenter , Vector3 boxSize , Vector3 castDirection , float castDistance , Vector3 boxUp , RaycastHit2D[] results2D , RaycastHit[] results3D , LayerMask layerMask)
	{
		return is3D ? 
		BoxcastAll3D( boxCenter , boxSize , castDirection , castDistance , boxUp , results3D , layerMask) : 
		BoxcastAll2D( boxCenter , boxSize , castDirection , castDistance , boxUp , results2D , layerMask);
	}

	// ---------------------------------------------------------------------------------------------------------
	// 2D ------------------------------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------------------------------
	static CollisionHitInfo Raycast2D( Vector3 origin, Vector3 castDirection , float castDistance , LayerMask layerMask )
	{
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();

		RaycastHit2D hitInfo2D = Physics2D.Raycast( 
			origin , 
			castDirection , 
			castDistance , 
			layerMask
		);

		hitInfo.collision = hitInfo2D.collider != null;

		if( hitInfo.collision )
			hitInfo.gameObject = hitInfo2D.collider.gameObject;
		
		hitInfo.distance = hitInfo2D.distance;
		hitInfo.point = hitInfo2D.point;
		hitInfo.normal = hitInfo2D.normal;

		return hitInfo;
	}

	static CollisionHitInfo Boxcast2D( Vector3 boxCenter , Vector3 boxSize , Vector3 castDirection , float castDistance , Vector3 boxUp , LayerMask layerMask)
	{		
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();

		RaycastHit2D hitInfo2D = Physics2D.BoxCast( 
			boxCenter , 
			boxSize ,
            Lightbug.CoreUtilities.Utilities.SignedAngle( Vector2.up , boxUp , Vector3.forward ) ,
			castDirection , 
			castDistance , 
			layerMask 
		);

		hitInfo.collision = hitInfo2D.collider != null;

		if( hitInfo.collision )
			hitInfo.gameObject = hitInfo2D.collider.gameObject;

		hitInfo.distance = hitInfo2D.distance;
		hitInfo.point = hitInfo2D.point;
		hitInfo.normal = hitInfo2D.normal;
		
		return hitInfo;
	}

	static CollisionHitInfo BoxcastAll2D( Vector3 boxCenter , Vector3 boxSize , Vector3 castDirection , float castDistance , Vector3 boxUp , RaycastHit2D[] results, LayerMask layerMask)
	{		
		CollisionHitInfo hitInfo2D = new CollisionHitInfo();
		hitInfo2D.Reset();

		int hits = Physics2D.BoxCastNonAlloc( 
			boxCenter , 
			boxSize ,
            Lightbug.CoreUtilities.Utilities.SignedAngle( Vector2.up , boxUp , Vector3.forward ) ,
			castDirection ,
			results ,
			castDistance , 
			layerMask 
		);

		for (int i = 0; i < hits; i++)
		{
			RaycastHit2D currentHitInfo2D = results[ i ];
			if( currentHitInfo2D.collider != null && currentHitInfo2D.distance != 0 )
			{
				hitInfo2D.collision = currentHitInfo2D.collider != null;

				if( hitInfo2D.collision )
					hitInfo2D.gameObject = currentHitInfo2D.collider.gameObject;

				hitInfo2D.distance = currentHitInfo2D.distance;
				hitInfo2D.point = currentHitInfo2D.point;
				hitInfo2D.normal = currentHitInfo2D.normal;
				
				break;
			}
		}
		
		return hitInfo2D;
		
	}

	// ---------------------------------------------------------------------------------------------------------
	// 3D ------------------------------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------------------------------

	static CollisionHitInfo Raycast3D(Vector3 origin, Vector3 castDirection , float castDistance , LayerMask layerMask )
	{
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();

		RaycastHit hitInfo3D;

		Physics.Raycast( 
			origin , 
			castDirection , 
			out hitInfo3D,
			castDistance , 
			layerMask
		);

		hitInfo.collision = hitInfo3D.collider != null;
		
		if( hitInfo.collision )
			hitInfo.gameObject = hitInfo3D.collider.gameObject;
		
		hitInfo.distance = hitInfo3D.distance;
		hitInfo.point = hitInfo3D.point;
		hitInfo.normal = hitInfo3D.normal;
		
		return hitInfo;
		
	}	

	static CollisionHitInfo Boxcast3D( Vector3 boxCenter , Vector3 boxSize , Vector3 castDirection , float castDistance , Vector3 boxUp , LayerMask layerMask)
	{
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();

		RaycastHit hitInfo3D;

		Physics.BoxCast( 
			boxCenter , 
			boxSize/2 ,
			castDirection ,
			out hitInfo3D , 
			Quaternion.LookRotation( Vector3.forward , boxUp ) ,
			castDistance ,
			layerMask 
		);

		hitInfo.collision = hitInfo3D.collider != null;
		
		if( hitInfo.collision )
			hitInfo.gameObject = hitInfo3D.collider.gameObject;
		
		hitInfo.distance = hitInfo3D.distance;
		hitInfo.point = hitInfo3D.point;
		hitInfo.normal = hitInfo3D.normal;

		return hitInfo;
		
	}
 
	static CollisionHitInfo BoxcastAll3D( Vector3 boxCenter , Vector3 boxSize , Vector3 castDirection , float castDistance , Vector3 boxUp , RaycastHit[] results, LayerMask layerMask)
	{		
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();

		//RaycastHit hitInfo3D;

		int hits = Physics.BoxCastNonAlloc( 
			boxCenter , 
			boxSize/2 ,
			castDirection ,
			results , 
			Quaternion.LookRotation( Vector3.forward , boxUp ) ,
			castDistance ,
			layerMask 
		);

		for( int i = 0; i < hits; i++ )
		{
			RaycastHit currentHitInfo3D = results[ i ];
			if( currentHitInfo3D.collider != null && currentHitInfo3D.distance != 0 )
			{
				hitInfo.collision = currentHitInfo3D.collider != null;

				if( hitInfo.collision )
					hitInfo.gameObject = currentHitInfo3D.collider.gameObject;

				hitInfo.distance = currentHitInfo3D.distance;
				hitInfo.point = currentHitInfo3D.point;
				hitInfo.normal = currentHitInfo3D.normal;
				
				break;
			}
		}
		
		return hitInfo;
		
	}


	public static CollisionHitInfo RaycastSweep( bool is3D , Vector3 start , Vector3 end , float numberOfRays , Vector3 castDirection , float castDistance , RaySelectionRule rule , LayerMask layerMask )
	{
		CollisionHitInfo hitInfo = new CollisionHitInfo();
		hitInfo.Reset();

		float castArea = Vector3.Magnitude( end - start );
		Vector3 startToEndDirection = ( end - start ).normalized;	

		hitInfo.distance = castDistance;		
		
		CollisionHitInfo currentHitInfo;		
					
		float step = castArea / (numberOfRays - 1);

		for (int i = 0; i < numberOfRays; i++)
		{
			Vector3 rayOrigin = start + startToEndDirection * step * i;		

			// Debug.DrawRay( rayOrigin , castDirection * castDistance , Color.magenta );	

			currentHitInfo = PhysicsUtilities.Raycast(
				is3D ,
				rayOrigin , 
				castDirection ,
				castDistance,
				layerMask
			);

			
			if( !currentHitInfo.collision)
				continue;

			switch( rule )
			{
				case RaySelectionRule.Shortest:

					if(currentHitInfo.distance < hitInfo.distance)					
						hitInfo = currentHitInfo;

				break;

				case RaySelectionRule.ShortestNonZero:

					if( currentHitInfo.distance != 0 && currentHitInfo.distance < hitInfo.distance)					
						hitInfo = currentHitInfo;
				break;

				case RaySelectionRule.Longest:

					if( currentHitInfo.distance > hitInfo.distance)					
						hitInfo = currentHitInfo;

				break;
			}
			
		}

		return hitInfo;

		
	}
	
}

}
