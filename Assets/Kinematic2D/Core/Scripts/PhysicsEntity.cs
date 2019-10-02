using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lightbug.Kinematic2D.Core
{

/// <summary>
/// Custom motion behaviour for non-characters (Platfrom 2D and 3D)
/// </summary>	
public abstract class PhysicsEntity : MonoBehaviour
{
	// Transform catched
	Transform catchedTransform = null;
	new protected Transform transform
	{
		get
		{
			if( catchedTransform == null )
				catchedTransform = gameObject.transform;
			
			return catchedTransform;
		}
	}

	[SerializeField] protected UpdateMethod updateType;	
	[SerializeField] protected MotionMode motionMode;

	public void SetUpdateType( UpdateMethod updateType )
	{
		this.updateType = updateType;
	}
	
	
	protected virtual void UpdateBehaviour(float dt){}

	// Physics
	protected abstract void MoveRigidbody( Vector3 targetPosition );
	protected abstract void RotateRigidbody( Quaternion targetRotation );	


	// Default Unity Messages --------------------------------------------------
	
	void Update()
	{
		if( updateType != UpdateMethod.Update )
			return;

		UpdateBehaviour( Time.deltaTime );
	}

	void LateUpdate()
	{
		if( updateType != UpdateMethod.LateUpdate )
			return;

		UpdateBehaviour( Time.deltaTime );
	}

	void FixedUpdate()
	{
		if( updateType != UpdateMethod.FixedUpdate )
			return;

		UpdateBehaviour( Time.fixedDeltaTime );
	}



}




}
