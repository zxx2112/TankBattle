using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lightbug.Kinematic2D.Core
{

public abstract class PhysicsEntity2D : PhysicsEntity
{
	
	new Rigidbody2D rigidbody2D;

	protected virtual void Awake()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		
		if( motionMode != MotionMode.Transform)
			rigidbody2D.interpolation = motionMode == MotionMode.RigidbodyInterpolated ? 
			RigidbodyInterpolation2D.Interpolate : RigidbodyInterpolation2D.None;
	}

	protected override void MoveRigidbody( Vector3 targetPosition )
	{
		if(motionMode == MotionMode.Transform)
		{
			transform.position = targetPosition;
		}
		else if(motionMode == MotionMode.RigidbodyNonInterpolated)
		{
			rigidbody2D.position = targetPosition;
		}
		else
		{
			rigidbody2D.MovePosition( targetPosition );
		}				
			
	}

	protected override void RotateRigidbody( Quaternion targetRotation )
	{
		if(motionMode == MotionMode.Transform)
		{
			transform.rotation = targetRotation;
		}
		else if(motionMode == MotionMode.RigidbodyNonInterpolated)
		{
			rigidbody2D.rotation = targetRotation.eulerAngles.z;
		}
		else
		{
			rigidbody2D.MoveRotation( targetRotation.eulerAngles.z );
		}	
		
	}
	
}

}
