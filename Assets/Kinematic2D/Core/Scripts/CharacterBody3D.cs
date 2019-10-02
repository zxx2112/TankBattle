using UnityEngine;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Core
{

[AddComponentMenu("Kinematic2D/Core/Character Body 3D")]
public sealed class CharacterBody3D : CharacterBody
{
	Rigidbody rigidbody3D;

	public override bool Is3D()
	{
		return true; 
	}
	
	

	protected override void Awake()
	{
		base.Awake();
		
		BoxCollider collider = gameObject.GetOrAddComponent<BoxCollider>();
		collider.size = CharacterSize;
		collider.center = new Vector3( 0 , heightExtents , 0 );

		rigidbody3D = gameObject.GetOrAddComponent<Rigidbody>();
		rigidbody3D.isKinematic = true;

		
	}

	protected override void OnDrawGizmos()
	{
		if( !drawBodyShapeGizmo )
			return;
		
		base.OnDrawGizmos();

		Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
		Matrix4x4 gizmoMatrix = Matrix4x4.TRS( transform.position, transform.rotation, transform.localScale);

		Gizmos.matrix *= gizmoMatrix;

		Vector3 deltaCenter = Vector3.Magnitude( center - transform.position ) * Vector3.up;
		Vector3 deltaCenterStepOffset = Vector3.Magnitude( center_StepOffset - transform.position ) * Vector3.up;
		
		Gizmos.DrawWireCube( deltaCenter , CharacterSize );
		Gizmos.DrawWireCube( deltaCenterStepOffset , new Vector3( verticalArea , horizontalArea_StepOffset , CharacterSize.z ) );
				
		Gizmos.matrix = oldGizmosMatrix;
		
	}
	
	public override void MoveRigidbody( Vector3 targetPosition )
	{
		rigidbody3D.MovePosition( targetPosition );
	}

	public override void RotateRigidbody( Quaternion targetRotation )
	{		
		rigidbody3D.MoveRotation( targetRotation );
	}

	public override void SetInterpolation( bool enabled )
	{
		rigidbody3D.interpolation = enabled ? 
		RigidbodyInterpolation.Interpolate : 
		RigidbodyInterpolation.None;
	}

	public override void SetCharacterSize( Vector3 size )
	{
		width = size.x;
		height = size.y;
		depth = size.z;

	}
}

}
