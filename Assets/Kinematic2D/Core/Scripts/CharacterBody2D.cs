using UnityEngine;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Core
{

[AddComponentMenu("Kinematic2D/Core/Character Body 2D")]
public sealed class CharacterBody2D : CharacterBody
{
	const float Default2DDepth = 0.001f;

	new Rigidbody2D rigidbody2D;

	public override bool Is3D()
	{
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		
		depth = Default2DDepth;
		
		BoxCollider2D collider = gameObject.GetOrAddComponent<BoxCollider2D>();


		collider.size = CharacterSize;
		collider.offset = new Vector2( 0 , heightExtents );

		rigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
		rigidbody2D.isKinematic = true;


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
		
		Gizmos.DrawWireCube( deltaCenter , new Vector3( CharacterSize.x , CharacterSize.y , 0.001f ) );
		Gizmos.DrawWireCube( deltaCenterStepOffset , new Vector3( verticalArea , horizontalArea_StepOffset , 0.001f ) );
				
		Gizmos.matrix = oldGizmosMatrix;
		
	}

	public override void MoveRigidbody( Vector3 targetPosition )
	{
		rigidbody2D.MovePosition( targetPosition );
	}

	public override void RotateRigidbody( Quaternion targetRotation )
	{		
		rigidbody2D.MoveRotation( targetRotation.eulerAngles.z );
	}

	public override void SetInterpolation( bool enabled )
	{
		rigidbody2D.interpolation = enabled ? 
		RigidbodyInterpolation2D.Interpolate : 
		RigidbodyInterpolation2D.None;
	}

	public override void SetCharacterSize( Vector3 size )
	{
		width = size.x;
		height = size.y;

	}	
	

	
}

}
