using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Lightbug.Kinematic2D.Core
{

public abstract class PhysicsEntity3D : PhysicsEntity
{
	
	Rigidbody rigidbody3D;

	protected virtual void Awake()
	{

		rigidbody3D = GetComponent<Rigidbody>();
				
		rigidbody3D.interpolation = motionMode == MotionMode.RigidbodyInterpolated ? 
		RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
	}

	
	protected override void MoveRigidbody( Vector3 targetPosition )
	{
		if(motionMode == MotionMode.Transform)
		{
			transform.position = targetPosition;
		}
		else if(motionMode == MotionMode.RigidbodyNonInterpolated)
		{
			rigidbody3D.position = targetPosition;
		}
		else
		{
			rigidbody3D.MovePosition( targetPosition );
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
			rigidbody3D.rotation = targetRotation;
		}
		else
		{
			rigidbody3D.MoveRotation( targetRotation );
		}	
	}
	
}

}
