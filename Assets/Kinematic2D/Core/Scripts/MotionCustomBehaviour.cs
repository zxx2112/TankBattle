using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lightbug.Kinematic2D.Core
{

public enum MotionMode
{
	Transform ,
	RigidbodyNonInterpolated ,
	RigidbodyInterpolated
}


public class MotionCustomBehaviour : UpdateCustomBehaviour
{		
	
	[Tooltip("The motion mode manages the movement/rotation method that will be called internally by the character motor.\n\n" + 
	"For the 'Transform' mode use Update/LateUpdate, for the 'RigidbodyNonInterpolated' and " + 
	"'RigidbodyInterpolated' use FixedUpdate instead.\n\nDo not worry about enabling/disabling yourself the interpolation settings in the Rigidbody component, "
	+ "the CharacterMotor takes care of it.")]
	[SerializeField] protected MotionMode motionMode = MotionMode.Transform;	


	protected CharacterBody characterBody;
	public CharacterBody CharacterBody
	{
		get
		{
			return characterBody;
		}
	}

	protected bool interpolate = true;


	public void SetMotionMode( MotionMode motionMode )
	{
		this.motionMode = motionMode;
	}
	
	protected override void Awake()
	{
		characterBody = GetComponent<CharacterBody>();
	}

	void Start()
	{
		characterBody.SetInterpolation( motionMode == MotionMode.RigidbodyInterpolated );

	}	

	public void MoveRigidbody( Vector3 targetPosition )
	{
		if(motionMode == MotionMode.Transform)
		{
			transform.position = targetPosition;
		}
		else
		{
			characterBody.MoveRigidbody( targetPosition );
		}				
			
	}

	public void RotateRigidbody( Quaternion targetRotation )
	{
		if(motionMode == MotionMode.Transform)
		{
			transform.rotation = targetRotation;
		}
		else
		{
			characterBody.RotateRigidbody( targetRotation );
		}
		
	}
}

}
