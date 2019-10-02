using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lightbug.Kinematic2D.Core
{



public struct BodyTransform
{
	Vector3 position;
	public Vector3 Position
	{ 
		get
		{ 
			return position;
		} 
	}


	Quaternion rotation;
	public Quaternion Rotation
	{ 
		get
		{ 
			return rotation;
		} 
	}
	
	
	Vector3 forward;
	public Vector3 Forward
	{ 
		get
		{ 
			return forward;
		} 
	}

	Vector3 right;
	public Vector3 Right
	{ 
		get
		{ 
			return right;
		} 
	}

	Vector3 up;
	public Vector3 Up
	{ 
		get
		{ 
			return up;
		} 
	}

	void UpdateFields()
	{
		up = rotation * Vector3.up;
		right = rotation * Vector3.right;
		forward = rotation * Vector3.forward;
	}

	public void Translate( Vector3 deltaPosition , Space space = Space.Self)
	{
		Vector3 result = deltaPosition;
		if(space == Space.Self)
		{
			result = 
				deltaPosition.x * right + 
				deltaPosition.y * up
			;
		}

		position += result;

		UpdateFields();
	}

	public void RotateAround( Vector3 point, Vector3 axis, float angle )
	{
		Vector3 delta = point - position;

		Quaternion targetRotation = Quaternion.AngleAxis( angle , Vector3.forward );
		delta = targetRotation * delta;
		
		position = point - delta;
		rotation =  rotation * targetRotation;		

		UpdateFields();
	}

	public void SetTransform( Transform transform )
	{
		this.position = transform.position;
		this.rotation = transform.rotation;
		

		UpdateFields();
	}

	public void SetTransform( Vector3 position , Quaternion rotation )
	{
		this.position = position;
		this.rotation = rotation;

		UpdateFields();
	}

	public void SetPosition( Vector3 position )
	{
		this.position = position;
		UpdateFields();
	}

	public void SetRotation( Quaternion rotation )
	{
		this.rotation = rotation;
		UpdateFields();
	}

	

	public string GetInfo()
	{
		string output = string.Concat( 
			"pos : <" , position.x , "," , position.y + "," , position.z , ">" , " --- " ,
			"rot : <" , rotation.eulerAngles.x , "," , rotation.eulerAngles.y + "," , rotation.eulerAngles.z , ">" );

		return output;
	}

}



/// <summary>
/// This class contains all the data regarding the character body shape.
/// </summary>
public abstract class CharacterBody : MonoBehaviour
{
	[Header ("Debug")]
	[SerializeField] protected bool drawBodyShapeGizmo = true;
	public bool DrawCollisionShape
	{
		get
		{
			return drawBodyShapeGizmo;
		}
	}

	[SerializeField] protected bool drawGroundedBodyShapeGizmo = true;
	public bool DrawGroundedBodyShapeGizmo
	{
		get
		{
			return drawGroundedBodyShapeGizmo;
		}
	}

	[SerializeField] protected Color gizmosColor = new Color( 0.52f , 1f , 0.24f );

	
	[Header("Body")]
	
	[Range_NoSlider(true)]
	public float width = 1;

	[Range_NoSlider(true)]
	public float height = 1;

	[Range_NoSlider(true)]
	public float depth = 1;

	[Tooltip("The skinWidth is used to prevent the character from getting stuck with other collider. If you are experiencing some problems try to increase this value.")] 	
	[Range_NoSlider( 0.001f , Mathf.Infinity )]
	[SerializeField] 
	protected float skinWidth = 0.02f;
	public float SkinWidth
	{ 
		get
		{ 
			return skinWidth; 
		} 
	}

	
	[Tooltip("The Step Offset determine the maximum step height the character can walk. \n")]
	[Range_NoSlider( 0.008f , Mathf.Infinity )]
	[SerializeField]
	protected float stepOffset = 0.25f;
	public float StepOffset
	{ 
		get
		{ 
			return stepOffset; 
		} 
	}

		
	
	[Space(10)]
	[Header("Raycast Options")]
	
	[Tooltip("Number of rays fired horizontally by the RayCast method (Grounded horizontal movement).")]
	[Range(3,100)] [SerializeField] protected int horizontalRays = 5;
	public int HorizontalRays
	{ 
		get
		{ 
			return horizontalRays; 
		}
	}
	
	
	[Space(10)]
	[Header("Boxcast")]
	[Tooltip("The RayCast method behaves different than the BoxCast method "
	+ "resulting in tiny variations in the final visual result, this field allows you to offset the "
	+ "contact point of the BoxCast method to match the results." + "\n\nDefault value : < 0.0045 , 0.0045 >")]
	[SerializeField] protected Vector2 boxContactOffset = new Vector2( 0.0045f , 0.0045f );
	public Vector2 BoxContactOffset
	{ 
		get
		{ 
			return boxContactOffset; 
		} 
	}

	/// <summary>
	/// The thickness of the box shape used by the Boxcast methods.
	/// </summary>
	const float boxThickness = 0.001f;

	/// <summary>
	/// The thickness of the box shape used by the Boxcast methods.
	/// </summary>
	public float BoxThickness
	{ 
		get
		{ 
			return boxThickness; 
		} 
	}
		

	protected float initialWidth;
	public float InitialWidth
	{ 
		get
		{
			return initialWidth; 
		} 
	}

	protected float initialHeight;
	public float InitialHeight
	{ 
		get
		{ 
			return initialHeight; 
		} 
	}

	protected float initialDepth;
	public float InitialDepth
	{ 
		get
		{ 
			return initialDepth; 
		} 
	}
	
	
	public BodyTransform bodyTransform = new BodyTransform();
	


	/// <summary>
	/// Half of the width.
	/// </summary>
	public float widthExtents
	{ 
		get
		{ 
			return width / 2;
		} 
	}

	/// <summary>
	/// Half of the height.
	/// </summary>
	public float heightExtents
	{ 
		get
		{ 
			return height / 2;
		} 
	}

	
	/// <summary>
	/// Effective area used for horizontal collision detection (Not grounded).
	/// </summary>
	public float horizontalArea
	{ 
		get
		{ 
			return height - 2 * skinWidth; 		
		} 
	}	

	/// <summary>
	/// Effective area used for horizontal collision detection, considering the step offset (Grounded).
	/// </summary>
	public float horizontalArea_StepOffset
	{
		get
		{ 
			return height - skinWidth - stepOffset;
		}	
	}

	/// <summary>
	/// Effective area used for vertical collision detection (Not grounded).
	/// </summary>
	public float verticalArea
	{
		get
		{
			return width - 2 * skinWidth;		
		}	
	}

	/// <summary>
	/// Effective box size of the character body shape excluding the skin width (Not Grounded).
	/// </summary>
	public Vector3 boxSize
	{ 
		get
		{ 
			return new Vector3( verticalArea , horizontalArea , 1 );		
		} 	
	}

	/// <summary>
	/// Effective box size of the character body shape excluding the skin width and the step offset area (Grounded).
	/// </summary>
	public Vector3 boxSize_StepOffset
	{ 
		get
		{ 
			return new Vector3( verticalArea , horizontalArea_StepOffset , 1 );		
		} 	
	}
	
	/// <summary>
	/// 
	/// </summary>
	public Vector3 boxSize_FullStepOffset
	{ 
		get
		{ 
			return new Vector3( width , height - stepOffset , 1 );		
		} 	
	}

	/// <summary>
	/// Effective box size used by the horizontal collision detection algorithm (Not grounded).
	/// </summary>
	public Vector3 horizontalBoxSize
	{ 
		get
		{ 
			return new Vector3( boxThickness , horizontalArea , 1 );		
		} 	
	}

	/// <summary>
	/// Effective box size used by the horizontal collision detection algorithm, excluding the step offset area. (Grounded).
	/// </summary>
	public Vector3 horizontalBoxSize_StepOffset
	{ 
		get
		{ 
			return new Vector3( boxThickness , horizontalArea_StepOffset , 1 );		
		} 	
	}

	/// <summary>
	/// Effective box size used by the vertical collision detection algorithm.
	/// </summary>
	public Vector3 verticalBoxSize
	{ 
		get
		{ 
			return new Vector3( verticalArea , boxThickness , 1 ); 
		} 
	}

	/// <summary>
	/// Effective box size used by the vertical collision detection algorithm.
	/// </summary>
	public Vector3 narrowVerticalBoxSize
	{ 
		get
		{ 
			return new Vector3( 0.1f * verticalArea , boxThickness , 1 ); 
		} 
	}	

	/// <summary>
	/// The current three dimensional character size.
	/// </summary>
	public virtual Vector3 CharacterSize
	{ 
		get
		{ 
			return new Vector3( 
				width , 
				height , 
				depth 
			);
		} 
	}

	/// <summary>
	/// The initial character size (on Awake).
	/// </summary>
	public virtual Vector3 InitialSize
	{ 
		get
		{ 
			return new Vector3(
				initialWidth ,
				initialHeight ,
				initialDepth
			);
		} 
	}

	// Collision -----------------------------------------------------------------------------------------------------	
	
	public Vector3 center
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Up * ( height / 2 );
		} 
	}

	public Vector3 center_StepOffset
	{
		get
		{ 
			return bodyTransform.Position + bodyTransform.Up * ( stepOffset + horizontalArea_StepOffset / 2 );
		} 
	}

	public Vector3 center_FullStepOffset
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Up * ( stepOffset + ( height - stepOffset ) / 2 );
		} 
	}

	public Vector3 bottomRight
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Right * ( width / 2);
		} 
	}

	public Vector3 bottomLeft
	{ 
		get
		{ 
			return bodyTransform.Position - bodyTransform.Right * ( width / 2);
		} 
	}

	public Vector3 topRight
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Up * height + bodyTransform.Right * ( width / 2 );
		} 
	}

	public Vector3 topLeft
	{ 
		get
		{ 
			return  bodyTransform.Position + bodyTransform.Up * height - bodyTransform.Right * ( width / 2 );
		} 
	}

	public Vector3 middleRight
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Right * widthExtents + 
			bodyTransform.Up * heightExtents;
		} 
	}

	public Vector3 middleLeft
	{ 
		get
		{ 
			return bodyTransform.Position - bodyTransform.Right * widthExtents + 
			bodyTransform.Up * heightExtents;
		} 
	}

	// Not Grounded Corners ------------------------------------------------------------------------------------------
	public Vector3 bottomRightCollision
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Right * (verticalArea/2) + 
			bodyTransform.Up * skinWidth;
		} 
	}

	public Vector3 bottomLeftCollision
	{ 
		get
		{ 
			return bodyTransform.Position - bodyTransform.Right * (verticalArea/2) + 
			bodyTransform.Up * skinWidth;
		} 
	}

	public Vector3 topRightCollision
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Right * (verticalArea/2) + 
			bodyTransform.Up * ( height - skinWidth);
		} 
	}

	public Vector3 topLeftCollision
	{ 
		get
		{ 
			return  bodyTransform.Position - bodyTransform.Right * (verticalArea/2) + 
			bodyTransform.Up * ( height - skinWidth);
		} 
	}

	public Vector3 middleRightCollision
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Right * (verticalArea/2) + 
			bodyTransform.Up * ( height / 2 );
		} 
	}

	public Vector3 middleLeftCollision
	{ 
		get
		{ 
			return bodyTransform.Position - bodyTransform.Right * (verticalArea/2) + 
			bodyTransform.Up * ( height / 2 );
		} 
	}

	public Vector3 middleTopCollision
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Up * ( height - skinWidth);
		} 
	}

	public Vector3 middleBottomCollision
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Up * ( skinWidth );
		} 
	}

	// Not Grounded Corners (StepOffset) ------------------------------------------------------------------------------------------
	public Vector3 bottomRightCollision_StepOffset
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Right * (verticalArea/2) + 
			bodyTransform.Up * stepOffset;
		} 
	}

	public Vector3 bottomLeftCollision_StepOffset
	{ 
		get
		{ 
			return bodyTransform.Position - bodyTransform.Right * (verticalArea/2) + 
			bodyTransform.Up * stepOffset;
		} 
	}
	
	public Vector3 middleBottomCollision_StepOffset
	{ 
		get
		{ 
			return bodyTransform.Position + bodyTransform.Up * ( stepOffset );
		} 
	}

	//------------------------------------------------------------------------------------------------------------------
	public abstract bool Is3D();

	public abstract void SetInterpolation( bool enabled );
	public abstract void SetCharacterSize( Vector3 size );
	

	public virtual void MoveRigidbody( Vector3 targetPosition ){}
	public virtual void RotateRigidbody( Quaternion targetRotation ){}
	
	protected virtual void OnEnable()
	{
		bodyTransform.SetTransform( base.transform.position , base.transform.rotation );
	}

	protected virtual void Awake()
	{
          bodyTransform.SetTransform( base.transform.position , base.transform.rotation );

		initialHeight = height;
		initialWidth = width;
		initialDepth = depth;
	}

	protected virtual void OnDrawGizmos()
	{
		Gizmos.color = gizmosColor;
		DrawRaycastGizmos();		

	}

	protected void DrawRaycastGizmos()
	{	
		Vector3 origin = Vector3.zero;

		if( !DrawGroundedBodyShapeGizmo )
			origin = center - base.transform.up * horizontalArea / 2;
		else
			origin = base.transform.position + base.transform.up * stepOffset;
			
						
		float step = ( !DrawGroundedBodyShapeGizmo ? horizontalArea : horizontalArea_StepOffset ) / (HorizontalRays - 1);

		for (int i = 0; i < HorizontalRays; i++)
		{
                    Vector3 rayOrigin = origin + base.transform.up * step * i;
                    Gizmos.DrawLine( rayOrigin , rayOrigin + base.transform.right * width);
		}

		
	}

}

}

	
