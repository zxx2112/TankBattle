using UnityEngine;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Core
{



/// <summary>
/// The vertical direction modes availables for the character vertical alignment.
/// </summary>
public enum VerticalAlignmentMode 
{
	World , 
	Local ,
	Object
}

/// <summary>
/// Container of information regarding the collision process.
/// </summary>
public struct CollisionInfo
{
	// Ground Info	
	public GameObject groundObject;
	public int groundLayer;
	public Vector3 groundNormal;
	public Vector3 groundContactPoint;

	// Collision flags	
	public bool bottom;
	public bool top;	
	public bool left;
	public bool right;

	public bool crushed;

	// Slope info
	public float verticalSlopeSignedAngle;
	public float verticalSlopeAngle;
	public float verticalSlopeAngleSign;

	public bool onStableGround;

	// Wall info
	public GameObject wallObject;
	public float wallSignedAngle;
	public float wallAngle;
	public float wallAngleSign;

	//Movement
	public Vector3 groundMovementDirection;

		
	/// <summary>
	/// Reset the fields values to the default.
	/// </summary>
	public void Reset()
	{
		bottom = false;
		top = false;
		left = false;
		right = false;

		crushed = false;

		verticalSlopeSignedAngle = 0;
		verticalSlopeAngle = 0;
		verticalSlopeAngleSign = 1;

		onStableGround = false;

		groundObject = null;
		groundLayer = 0;
		groundNormal = Vector3.up;
		groundContactPoint = Vector3.zero;
		groundMovementDirection = Vector3.right;

		wallObject = null;
		wallSignedAngle = 0;
		wallAngle = 0;
		wallAngleSign = 1;		

	}

	public string GetInfoString()
	{		
		//builder.Remove(0 , builder.Length);
		string output = string.Concat( 
			"Debug : \n\n"  , 
			"Ground Info : \n" ,
			"Bottom = "  + bottom.ToString() + '\n' ,
			"Top = " + top.ToString() + '\n' ,
			"Left = " + left.ToString() + '\n' ,
			"Right = " + right.ToString() + "\n" , 
			"Crushed = " + crushed.ToString() + "\n\n" ,
			"Slopes Info : \n" ,
			"VerticalSlopeSignedAngle = " + verticalSlopeSignedAngle.ToString() + '\n',
			"VerticalSlopeAngle = " + verticalSlopeAngle.ToString() + '\n' ,
			"VerticalSlopeAngleSign = " + verticalSlopeAngleSign.ToString() + '\n' ,
			"OnStableGround = " + onStableGround.ToString() + "\n\n" ,
			"Ground Info : \n" ,
			"GroundObject = " + ( groundObject != null ? groundObject.name : " ----- " ) + '\n',
			"GroundLayer = " + LayerMask.LayerToName(groundLayer) + '\n' ,
			"GroundNormal = " + groundNormal.ToString() + '\n' ,
			"GroundContactPoint = " + groundContactPoint.ToString() + "\n\n" ,
			"Wall Info : \n" ,
			"WallObject = " + ( wallObject != null ? wallObject.name : " ----- " ) + '\n' ,
			"WallSignedAngle = " + wallSignedAngle.ToString() + '\n' ,
			"WallAngle = " + wallAngle.ToString() + '\n' ,
			"WallAngleSign = " + wallAngleSign.ToString() + '\n'
		);

				
		return output;
	}
	
}


/// <summary>
/// Container of information for the dynamic ground.
/// </summary>
public struct DynamicGroundInfo
{
	public Transform targetTransform;
	public Vector3 position;
	public Quaternion rotation;
	public Vector2 velocity;

	public void Reset()
	{
		targetTransform = null;
		position = Vector3.zero;
		rotation = Quaternion.identity;
		velocity = Vector2.zero;
	}

	public void UpdateTarget( Transform targetTransform )
	{
		this.targetTransform = targetTransform;
		position = targetTransform.position;
		rotation = targetTransform.rotation;
	}

	public void UpdateInfo()
	{		
		if( targetTransform == null )
			return;
		
		position = targetTransform.position;
		rotation = targetTransform.rotation;
	}
}



[AddComponentMenu("Kinematic2D/Core/Character Motor")]
public class CharacterMotor : MotionCustomBehaviour
{
	

	#region Events
	
	public delegate void CharacterEvent();

	public event CharacterEvent OnGroundCollision;
	public event CharacterEvent OnHeadCollision;
	public event CharacterEvent OnRightCollision;
	public event CharacterEvent OnLeftCollision;
	public event CharacterEvent OnCrushedCollision;

	public event CharacterEvent OnGroundedRightCollision;	
	public event CharacterEvent OnGroundedLeftCollision;

	public event CharacterEvent OnNotGroundedRightCollision;
	public event CharacterEvent OnNotGroundedLeftCollision;
	
	#endregion
	


	protected CharacterCollisions characterCollisions = null;
	public CharacterCollisions CharacterCollisions
	{
		get
		{
			return characterCollisions;
		}
	}
	

	#region Settings

	[Space(10)]

	[CustomClassDrawer]
	public DebugSettings debugSettings = new DebugSettings();
	
	[CustomClassDrawer]
	public LayerMaskSettings layerMaskSettings = new LayerMaskSettings();

	[CustomClassDrawer]
	public GroundSettings groundSettings = new GroundSettings();

	[CustomClassDrawer]
	public VerticalAlignmentSettings verticalAlignmentSettings = new VerticalAlignmentSettings();

	[CustomClassDrawer]
	public SlideSettings slideSettings = new SlideSettings();

	[CustomClassDrawer]
	public GroundAlignmentSettings groundAlignmentSettings = new GroundAlignmentSettings();
	
	[CustomClassDrawer]
	public DynamicGroundSettings dynamicGroundSettings = new DynamicGroundSettings();

	[CustomClassDrawer]
	public VelocitySettings velocitySettings = new VelocitySettings();

	[CustomClassDrawer]
	public DepenetrationSettings depenetrationSettings = new DepenetrationSettings();

	#endregion

	//-----------------------------------------------------------------------------------------------------------
	
	public Vector3 Position
	{
		get
		{
			return characterBody.bodyTransform.Position;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return characterBody.bodyTransform.Rotation;
		}
	}

	public Vector3 Forward
	{ 
		get
		{ 
			return characterBody.bodyTransform.Forward;
		} 
	}

	public Vector3 Right
	{ 
		get
		{ 
			return characterBody.bodyTransform.Right;
		} 
	}

	public Vector3 Up
	{ 
		get
		{ 
			return characterBody.bodyTransform.Up;
		} 
	}
	
	Vector3 currentVerticalDirection = Vector3.up;
	public Vector3 CurrentVerticalDirection
	{ 
		get
		{ 
			return currentVerticalDirection; 
		} 
	}

	

	DynamicGroundInfo currentDynamicGroundInfo;
	DynamicGroundInfo previousDynamicGroundInfo;

	GameObject collidedTrigger = null;
	public GameObject CollidedTrigger
	{ 
		get
		{ 
			return collidedTrigger; 
		} 
	}

	public string collidedTriggerTag
	{ 
		get
		{ 
			return collidedTrigger != null ? collidedTrigger.tag : null; 
		} 
	}


	Vector2 velocity = Vector2.zero;
	public Vector2 Velocity
	{
		get
		{
			return velocity;
		}
	}
		
	const float minimumMovement = 0.001f;
	public float MinimumMovement
	{
		get
		{
			return minimumMovement;
		}
	}
	
	// ------------------------------------------------------------------------------------------------------------------
	// COLLISION INFO ---------------------------------------------------------------------------------------------------
	// ------------------------------------------------------------------------------------------------------------------


	CollisionInfo collisionInfo = new CollisionInfo();
	
	public string GetCollisionInfoString 
	{ 
		get
		{ 
			return string.Concat( 
				collisionInfo.GetInfoString() , 
				"\n\nCollided Trigger : " , 
				collidedTrigger != null ? collidedTrigger.name : "null" ,
				"\n\nis Facing : " , 
				isFacingRight ? "Right" : "Left"
			); 
		} 
	}
	
	// Collision flags ------------------------------------------------------------------------------
	public bool IsGrounded 
	{ 
		get
		{ 
			return collisionInfo.bottom; 
		} 
	}
	
	
	public bool IsHead 
	{ 
		get 
		{ 
			return collisionInfo.top; 
		} 
	} 

	public bool IsAgainstLeftWall 
	{ 
		get 
		{
			return collisionInfo.left; 
		} 
	} 

	
	public bool IsAgainstRightWall 
	{ 
		get 
		{ 
			return collisionInfo.right;  
		} 
	}

	public bool IsCrushed 
	{ 
		get 
		{ 
			return collisionInfo.crushed;  
		} 
	}

	

	// Wall Info ----------------------------------------------------------------------------------------------------

	public GameObject WallObject 
	{ 
		get
		{  
			return collisionInfo.wallObject; 
		} 
	}

	public float WallSignedAngle 
	{ 
		get
		{  
			return collisionInfo.wallSignedAngle; 
		} 
	}

	public float WallAngle 
	{ 
		get
		{  
			return collisionInfo.wallAngle; 
		} 
	}

	public float WallAngleSign 
	{ 
		get
		{  
			return collisionInfo.wallAngleSign; 
		} 
	}


	// Ground Info ---------------------------------------------------------------------------------------------------

	public float VerticalSlopeSignedAngle 
	{ 
		get
		{  
			return collisionInfo.verticalSlopeSignedAngle; 
		} 
	}

	public float VerticalSlopeAngle 
	{ 
		get
		{  
			return collisionInfo.verticalSlopeAngle; 
		} 
	}

	public float VerticalSlopeAngleSign 
	{ 
		get
		{  
			return collisionInfo.verticalSlopeAngleSign; 
		} 
	}
	
	
	public bool IsOnRightVerticalSlope 
	{ 
		get
		{  
			return collisionInfo.verticalSlopeSignedAngle > 0; 
		} 
	}

	public bool IsOnLeftVerticalSlope 
	{ 
		get
		{  
			return collisionInfo.verticalSlopeSignedAngle < 0; 
		} 
	}

		
	public bool IsOnStableGround
	{ 
		get
		{ 
			return collisionInfo.onStableGround;			
		}
	}

	public Vector3 GroundMovementDirection 
	{ 
		get
		{  
			return collisionInfo.groundMovementDirection; 
		} 
	}

	public Vector3 GroundContactPoint 
	{ 
		get
		{  
			return collisionInfo.groundContactPoint; 
		} 
	}

	public int GroundLayer 
	{ 
		get
		{  
			return collisionInfo.groundLayer; 
		} 
	}

	public Vector3 GroundNormal 
	{ 
		get
		{  
			return collisionInfo.groundNormal; 
		} 
	}

	public GameObject GroundObject 
	{ 
		get
		{  
			return collisionInfo.groundObject; 
		} 
	}
	
	
	// ------------------------------------------------------------------------------------------------------------------
	// ------------------------------------------------------------------------------------------------------------------

	
	
	bool isFacingRight = true;
	public bool IsFacingRight 
	{ 
		get 
		{ 
			return isFacingRight; 
		} 
	}

	public bool IsSliding
	{
		get
		{
			return collisionInfo.bottom && !collisionInfo.onStableGround;
		}
	}

	public bool IsOnDynamicGround
	{
		get
		{
			return currentDynamicGroundInfo.targetTransform != null;
		}
	}

	public Vector3 DynamicGroundDisplacement
	{
		get
		{
			currentDynamicGroundInfo.UpdateInfo();
			return currentDynamicGroundInfo.position - previousDynamicGroundInfo.position;	
		}
	}

	public Vector3 DynamicGroundVelocity
	{
		get
		{
			float dt = updateMethod == UpdateMethod.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
			return DynamicGroundDisplacement / dt;		
		}
		
	}


	CollisionHitInfo collisionHitInfo = new CollisionHitInfo(); 
	GroundAlignmentResult groundAlignInfo = new GroundAlignmentResult(); 

	bool forceNotGroundedStateFlag = false;

	Vector3 lastSlidingVelocity;
	Vector3 lastGroundedVelocity;
	
	float dt = 0;

	protected override void Awake()
	{				
		base.Awake();

		characterCollisions = gameObject.GetOrAddComponent<CharacterCollisions>();	

		collisionInfo.groundMovementDirection = transform.right;

		CalculateVerticalDirection();
		AlignCharacterTowardsUp();
		
		
		if( layerMaskSettings.profile == null )
		{
			Debug.Log("Missing layerMask settings profile!");
			this.enabled = false;
		}
		
	}

	
	// Vertical Direction --------------------------------------------------------------------------------------
	
		
	public void SetVerticalDirection( Vector2 direction )
	{
		currentVerticalDirection = direction;
	}

	void CalculateVerticalDirection()
	{
		switch ( verticalAlignmentSettings.mode)
		{
			case VerticalAlignmentMode.World:
				currentVerticalDirection = verticalAlignmentSettings.worldVerticalDirection.normalized;
		    	break;

		    	case VerticalAlignmentMode.Local:
				currentVerticalDirection = characterBody.bodyTransform.Up;
		    	break;

			case VerticalAlignmentMode.Object:

				if( verticalAlignmentSettings.verticalReferenceObject == null )
				{
					Debug.Log("Missing vertical reference object!");
					break;
				}

				float verticalSign = verticalAlignmentSettings.towardsTheReference ? -1 : 1;
				currentVerticalDirection = verticalSign * ( characterBody.bodyTransform.Position - verticalAlignmentSettings.verticalReferenceObject.position ).normalized;
				
			break;
		}

		currentVerticalDirection.z = 0;
	}

		

	/// <summary>
	/// It teleports the character to a specific location.
	/// </summary>
	public void Teleport( Vector3 targetPosition )
	{	
		characterBody.bodyTransform.SetPosition( targetPosition );
		CalculateVerticalDirection();
		
	}

	/// <summary>
	/// It teleports the character to a specific location with a given rotation.
	/// </summary>
	public void Teleport( Vector3 targetPosition , Quaternion targetRotation )
	{	
		verticalAlignmentSettings.worldVerticalDirection =  targetRotation * Vector3.up;
		characterBody.bodyTransform.SetTransform( targetPosition , targetRotation );

		CalculateVerticalDirection();
		
	}


	// Velocity --------------------------------------------------------------------------------------------------

	/// <summary>
	/// Sets the velocity vector.
	/// </summary>
	public void SetVelocity( Vector2 velocity , Space space = Space.Self )
	{		 
		if( space == Space.Self )
		{
			this.velocity = velocity;
		}
		else
		{
			this.velocity = transform.InverseTransformDirection( velocity );
		}
	}	


	/// <summary>
	/// Sets the x component of the velocity vector.
	/// </summary>
	public void SetVelocityX( float value , Space space = Space.Self )
	{
		velocity.x = value;
	}
	
	/// <summary>
	/// Sets the y component of the velocity vector.
	/// </summary>
	public void SetVelocityY( float value , Space space = Space.Self )
	{
		velocity.y = value;
	}

	/// <summary>
	/// Adds a value to the x component of the velocity vector.
	/// </summary>
	public void AddVelocityX( float value , Space space = Space.Self )
	{
		velocity.x += value;		
	}

	/// <summary>
	/// Adds a value to the y component of the velocity vector.
	/// </summary>
	public void AddVelocityY( float value , Space space = Space.Self )
	{
		velocity.y += value;		
	}

	/// <summary>
	/// Adds a vector to the velocity vector.
	/// </summary>
	public void AddVelocity( Vector2 velocity , Space space = Space.Self )
	{
		if( space == Space.Self )
		{
			this.velocity += velocity;
		}
		else
		{
			this.velocity += (Vector2)transform.InverseTransformDirection( velocity );
		}
	}

	/// <summary>
	/// Reset the velocity vector to Zero.
	/// </summary>
	public void ResetVelocity()
	{
		velocity = Vector2.zero;
	}
		

	// Ground Alignment -------------------------------------------------------------------------------------------
	
	/// <summary>
	/// Aligns the character towards the up reference vector.
	/// </summary>
	public void AlignCharacterTowardsUp()
	{
		CalculateVerticalDirection();

		Vector3 verticalDirection = verticalAlignmentSettings.mode == VerticalAlignmentMode.Local ? 
		characterBody.bodyTransform.Up : currentVerticalDirection;
		Quaternion targetRotation = Quaternion.LookRotation( Vector3.forward , verticalDirection );

		
		characterBody.bodyTransform.SetRotation( targetRotation );
	}

	/// <summary>
	/// Sets the forceNotGroundedStateFlag to true. 
	/// The CharacterMotor will internally force the character to the not grouned state 
	/// (after the dynamic ground movement has been processed).
	/// </summary>
	public void ForceNotGroundedState()
	{
		forceNotGroundedStateFlag = true;		
	}

	/// <summary>
	/// Sets the grounded state to false (not grounded).
	/// </summary>
	void InternalForceNotGroundedState()
	{
		collisionInfo.bottom = false;
		currentDynamicGroundInfo.Reset();
		previousDynamicGroundInfo.Reset();

		if( !( verticalAlignmentSettings.mode == VerticalAlignmentMode.Local ) && groundAlignmentSettings.isEnabled )
			SlopeAlignReverse();		

		collisionInfo.Reset();	// must be reset after the "SlopeAlignReverse" Method
		
	}
	



	// Triggers ---------------------------------------------------------------------------------------------------------

	void OnTriggerEnter2D( Collider2D collider )
	{
		collidedTrigger = collider.gameObject;
	}

	void OnTriggerExit2D( Collider2D collider )
	{
		collidedTrigger = null;
	}

	void OnTriggerEnter( Collider collider )
	{
		collidedTrigger = collider.gameObject;
	}
	
	void OnTriggerExit( Collider collider )
	{
		collidedTrigger = null;
	}



	/// <summary>
	/// Limits the minumun horizontal velocity to the "minimumMovement" amount.
	/// </summary>
	/// <param name="deltaPosition"> the vector to modify.</param>	
	void ClampDisplacement( ref Vector3 deltaPosition )
	{		
		if( Mathf.Abs( deltaPosition.x ) < minimumMovement )
			deltaPosition.x = 0;		

		if( Mathf.Abs( deltaPosition.y ) < minimumMovement )
			deltaPosition.y = 0;
	}

	/// <summary>
	/// Main update method of the Character behaviour. 
	/// </summary>
	protected override void UpdateBehaviour( float dt )
	{
		this.dt = dt;
		
		Vector3 deltaPosition = velocity * this.dt;		
		ClampDisplacement( ref deltaPosition );

		if( !dynamicGroundAlreadyUpdated )
			ProcessDynamicGround();		
		else
			dynamicGroundAlreadyUpdated = false;
		

		if( depenetrationSettings.isEnabled )
			Depenetrate( IsGrounded , deltaPosition );

		if( forceNotGroundedStateFlag )
		{
			InternalForceNotGroundedState();
			forceNotGroundedStateFlag = false;

		}

		Move( deltaPosition , IsGrounded , true );
		
	}

	bool dynamicGroundAlreadyUpdated = false;

	protected void UpdateDynamicGround()
	{
		if( !dynamicGroundAlreadyUpdated )
		{
			ProcessDynamicGround();

			dynamicGroundAlreadyUpdated = true;
		}
	}
	
	
	/// <summary>
	/// Main method to move the character while detecting collisions.
	/// </summary>
	public void Move( Vector3 deltaPosition , bool groundedMovement , bool updateData )
	{
		if( groundSettings.alwaysNotGrounded )
			collisionInfo.bottom = false;		

		if( groundedMovement )
		{
			GroundMovement( deltaPosition );			
		}
		else
		{	
			AlignCharacterTowardsUp();
			NotGroundedMovement( deltaPosition , updateData );		
		}		
		
		MoveRigidbody( characterBody.bodyTransform.Position );
		RotateRigidbody( characterBody.bodyTransform.Rotation );

		CalculateVerticalDirection();
		
	}

	
	void ProcessDynamicGround()
	{
		if( !dynamicGroundSettings.isEnabled )
			return;
		
		if(currentDynamicGroundInfo.targetTransform != null)
		{
			currentDynamicGroundInfo.UpdateInfo();
				
			DynamicGroundMovement();
		}
		
	}
		
	/// <summary>
	/// It performs the collision detection and movement of the character if the grounded state is false (not grounded state).
	/// </summary>
	void NotGroundedMovement( Vector3 deltaPosition , bool updateData )
	{	
		collisionHitInfo.Reset();		
		
		LayerMask positiveHorizontalLayermask = layerMaskSettings.profile.obstacles;
		LayerMask negativeHorizontalLayermask = layerMaskSettings.profile.obstacles | 
		layerMaskSettings.profile.oneWayPlatforms;

		LayerMask horizontalMask = velocity.y > 0 ? positiveHorizontalLayermask : negativeHorizontalLayermask;
		
		collisionHitInfo = characterCollisions.HorizontalNotGroundedCollision(			
			deltaPosition.x ,
			horizontalMask 
		);
		
		
		if( collisionHitInfo.collision && collisionHitInfo.distance != 0 )// if(collisionHitInfo.collision)
		{	
			float distance = collisionHitInfo.distance - characterBody.SkinWidth;
			characterBody.bodyTransform.Translate( Mathf.Sign(deltaPosition.x) * Vector2.right * distance);
		}
		else
		{
			characterBody.bodyTransform.Translate( Vector2.right * deltaPosition.x );
		}

		if( updateData )
			FillHorizontalCollisionData( false );
		
		
		//Vertical
		LayerMask positiveVerticalLayermask = layerMaskSettings.profile.obstacles;
		LayerMask negativeVerticalLayermask = layerMaskSettings.profile.obstacles | 
		layerMaskSettings.profile.oneWayPlatforms;

		LayerMask verticalMask = velocity.y > 0 ? positiveVerticalLayermask : negativeVerticalLayermask;

		collisionHitInfo.Reset();

		
		collisionHitInfo = characterCollisions.VerticalNotGroundedCollision(
			deltaPosition.y , 
			verticalMask 
		);
		
		
		if( collisionHitInfo.collision && collisionHitInfo.distance != 0 )
		{									

			float distance = collisionHitInfo.distance - characterBody.SkinWidth;
			characterBody.bodyTransform.Translate( Mathf.Sign(deltaPosition.y) * Vector2.up * distance);



			if( deltaPosition.y < 0)
			{
				if(OnGroundCollision != null)
					OnGroundCollision();

				CalculateVerticalDirection();

				float verticalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( currentVerticalDirection , collisionHitInfo.normal , characterBody.bodyTransform.Forward );
				float verticalSlopeAngle = Mathf.Abs( verticalSlopeSignedAngle );
				bool isStableOnGround = verticalSlopeAngle <= groundSettings.maxSlopeAngle;

				if( !groundSettings.alwaysNotGrounded )
				{

					bool grounded = Lightbug.CoreUtilities.Utilities.BelongsToLayerMask( collisionHitInfo.gameObject.layer , verticalMask );
					
					if( updateData )
						FillGroundData( grounded , isStableOnGround );

				}
				

			}
			else if (deltaPosition.y > 0)
			{
				collisionInfo.top = true;

				if(OnHeadCollision != null)
					OnHeadCollision();

				
			}

		}
		else
		{	
			characterBody.bodyTransform.Translate( deltaPosition.y * Vector2.up);

			if( updateData )
			{
				collisionInfo.top = false;
				collisionInfo.bottom = false;
			}
		}


		
	}


	
	/// <summary>
	/// It performs the collision detection and movement of the character if the grounded state is true (grounded state).
	/// </summary>
	void GroundMovement( Vector3 deltaPosition )
	{	
		bool wasStable = collisionInfo.onStableGround;

		if( collisionInfo.onStableGround )
		{			

			if( groundAlignmentSettings.isEnabled )
				StableGroundAlignmentMovement( deltaPosition );
			else
				StableGroundMovement( deltaPosition );
			
		}
		else
		{			

			if( groundAlignmentSettings.isEnabled )
				UnstableGroundAlignmentMovement( deltaPosition );
			else
				UnstableGroundMovement( deltaPosition );

						
			
		}

		CalculatePostVelocity( wasStable );


		if( dynamicGroundSettings.isEnabled )
			CheckDynamicGround();
		
		
		
	}


	/// <summary>
	/// It handles the movement of the character when it is stable on the ground.
	/// </summary>	
	void MoveHorizontallyOnGround( Vector3 deltaPosition )
	{	
		// if( deltaPosition.x == 0 )
		// 	return;

		float inputXMagnitud = Mathf.Abs( deltaPosition.x );
		float inputXSign = Mathf.Sign( deltaPosition.x );

		collisionHitInfo.Reset();		
		collisionHitInfo = characterCollisions.HorizontalGroundedCollision(
			inputXSign * collisionInfo.groundMovementDirection ,
			inputXSign == 1 ,
			inputXMagnitud,			
			layerMaskSettings.profile.obstacles
		);
		
		if( collisionHitInfo.collision )
		{
			float distance = collisionHitInfo.distance - characterBody.SkinWidth;
			characterBody.bodyTransform.Translate( Mathf.Sign( deltaPosition.x ) * collisionInfo.groundMovementDirection * distance , Space.World );

		}
		else
		{
			characterBody.bodyTransform.Translate( deltaPosition.x * collisionInfo.groundMovementDirection , Space.World );
		}

		
		

		FillHorizontalCollisionData( true );
		
	}

	

	/// <summary>
	/// It calculates the slide velocity and applies it to the character.
	/// </summary>
	void CalculateSlideVelocity( ref Vector3 deltaPosition )
	{	
		float slideSign = - collisionInfo.verticalSlopeAngleSign;

		float slideCursor = ( collisionInfo.verticalSlopeAngle - groundSettings.maxSlopeAngle ) / ( 90 - groundSettings.maxSlopeAngle );

		float speed = slideSettings.isAffectedBySlope ? slideSettings.speed * slideSettings.influenceCurve.Evaluate(slideCursor) : 
		slideSettings.speed;
		
		lastSlidingVelocity = slideSign * speed * GroundMovementDirection;		
		SetVelocityX( slideSign * speed );
		deltaPosition = velocity * dt;

		
	}

	//BoxCollider2D col2D = null;

	
	/// <summary>
	/// Depenetrates the character from moving/rotating colliders. Only horizontal depenetration is performed if the character is grounded, for not groundeded state 
	/// vertical depenetration is added to the process.
	/// </summary>
	void Depenetrate( bool grounded , Vector2 deltaPosition )
	{			

		bool rightCodition = false;
		bool leftCondition = false;

		// Horizontal Depenetration ----------------------------------------------------------------------------------------------------
		float horizontalCastDistance = characterBody.width - characterBody.SkinWidth - characterBody.BoxThickness;
				
		// Right
		collisionHitInfo.Reset();
		collisionHitInfo = characterCollisions.HorizontalDepenetrationCollision( 
			grounded , 
			true ,
			layerMaskSettings.profile.obstacles
		);

		if(collisionHitInfo.collision)
		{
			if( collisionHitInfo.distance != 0 )
			{				
				float distance = collisionHitInfo.distance - horizontalCastDistance;
				characterBody.bodyTransform.Translate( Vector3.right * distance );
			}	
			else
			{
				rightCodition = true;
			}		
		}
		

		// Left
		collisionHitInfo.Reset();		
		collisionHitInfo = characterCollisions.HorizontalDepenetrationCollision( 
			grounded ,
			false ,
			layerMaskSettings.profile.obstacles
		);

		if(collisionHitInfo.collision)
		{
			if( collisionHitInfo.distance != 0 )
			{
				float distance = collisionHitInfo.distance - horizontalCastDistance;
				characterBody.bodyTransform.Translate( Vector3.left * distance );
			}	
			else
			{
				leftCondition = true;
			}			
		}

		if( grounded && leftCondition && rightCodition )
		{
			if( !collisionInfo.crushed )
			{
				if( OnCrushedCollision != null )
					OnCrushedCollision();

				collisionInfo.crushed = true;
			}
			
		}
		else
		{
			collisionInfo.crushed = false;
		}
		


		// Vertical Depenetration ----------------------------------------------------------------------------------------------------
		if(!grounded)
		{
			collisionHitInfo.Reset();
			collisionHitInfo = characterCollisions.VerticalDepenetrationCollision( true , layerMaskSettings.profile.obstacles );

			if( !grounded && collisionHitInfo.collision)
			{			
				if( collisionHitInfo.distance != 0 )
				{
					float distance = collisionHitInfo.distance - ( characterBody.height - characterBody.BoxThickness );	
					characterBody.bodyTransform.Translate( Vector3.up * distance );
				}
			}

			collisionHitInfo.Reset();
			collisionHitInfo = characterCollisions.VerticalDepenetrationCollision( false , layerMaskSettings.profile.obstacles );

			if( collisionHitInfo.collision)
			{			
				if( collisionHitInfo.distance != 0 )
				{
					float distance = collisionHitInfo.distance - ( characterBody.height - characterBody.BoxThickness );	
					characterBody.bodyTransform.Translate( Vector3.down * distance );
				}
			}

			
		}

	}	

		
	/// <summary>
	/// It Performs a collision test towards the ground, updating the grounded horizontal direction vector and setting up the position of the character (based on the ground clamping distance).
	/// </summary>
	void ProbeGround( bool stableGroundProbing , bool depenetrateFromSlope )
	{
		LayerMask groundedLayerMask = layerMaskSettings.profile.obstacles | layerMaskSettings.profile.oneWayPlatforms;

		collisionHitInfo.Reset();

		
		collisionHitInfo = characterCollisions.ProbeGroundCollision(
			groundSettings.groundClampingDistance ,
			groundedLayerMask
		);		
			
		if( collisionHitInfo.collision && collisionHitInfo.distance == 0 )
			return;

		StableGroundProbing( stableGroundProbing , depenetrateFromSlope );

		if( !stableGroundProbing )
			UnstableGroundProbing();
		
		

	}


	/// <summary>
	/// Sets the grounded state (along with the collision information) based on the result of the ProbeGround collision test.
	/// </summary>
	void FillGroundData( bool grounded , bool stable )
	{

		if(!grounded)
		{			
			lastGroundedVelocity = collisionInfo.groundMovementDirection * velocity.x;

			collisionInfo.Reset();
			collisionInfo.bottom = false;			
			collisionInfo.groundMovementDirection = characterBody.bodyTransform.Right;	
			return;
		}
		
		

		collisionInfo.bottom = true;

		CalculateVerticalDirection();

		collisionInfo.verticalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( currentVerticalDirection , collisionHitInfo.normal , characterBody.bodyTransform.Forward );
		collisionInfo.verticalSlopeAngle = Mathf.Abs( collisionInfo.verticalSlopeSignedAngle );
		collisionInfo.verticalSlopeAngleSign = Mathf.Sign( collisionInfo.verticalSlopeSignedAngle );
	
		collisionInfo.groundMovementDirection = Vector3.Cross( collisionHitInfo.normal , Vector3.forward );
		collisionInfo.groundNormal = collisionHitInfo.normal;
		collisionInfo.groundObject = collisionHitInfo.gameObject;
		collisionInfo.groundLayer = collisionHitInfo.gameObject.layer;
		collisionInfo.groundContactPoint = collisionHitInfo.point;
										
		collisionInfo.onStableGround = stable;

		if(!stable)
			ResetHorizontalData();			
		

	}

	/// <summary>
	/// Sets the grounded state (along with the collision information) based on the result of the ProbeGroundRays collision test.
	/// </summary>
	void SetGroundDataSlopeAlignment( bool grounded , bool stable , GroundDetectionRay ray )
	{

		if( !grounded )
		{			
			collisionInfo.Reset();
			collisionInfo.bottom = false;
			collisionInfo.groundMovementDirection = characterBody.bodyTransform.Right;	
			return;
		}
		
		collisionInfo.bottom = true;

		CalculateVerticalDirection();

		collisionInfo.verticalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( currentVerticalDirection , ray.normal , characterBody.bodyTransform.Forward );
		collisionInfo.verticalSlopeAngle = Mathf.Abs( collisionInfo.verticalSlopeSignedAngle );
		collisionInfo.verticalSlopeAngleSign = Mathf.Sign( collisionInfo.verticalSlopeSignedAngle );

		collisionInfo.groundMovementDirection = Vector3.Cross( ray.normal , Vector3.forward );
		collisionInfo.groundNormal = ray.normal;
		//info.groundObject = ray.gameObject;
		//info.groundLayer = ray.gameObject.layer;
		collisionInfo.groundContactPoint = ray.point;
										
		collisionInfo.onStableGround = stable;

		if(!stable)
			ResetHorizontalData();			
		
		
	}

	

	/// <summary>
	/// Resets the left, right and wall collision information.
	/// </summary>
	void ResetHorizontalData()
	{
		collisionInfo.right = collisionInfo.left = false;
		collisionInfo.wallSignedAngle = 0;
		collisionInfo.wallAngle = 0;
		collisionInfo.wallAngleSign = 1;
		collisionInfo.wallObject = null;
	}

	/// <summary>
	/// Fills the collision information, based on the horizontal collision test.
	/// </summary>
	void FillHorizontalCollisionData( bool grounded )
	{		
		if(!collisionHitInfo.collision)
		{
			ResetHorizontalData();
			return;
		}


		collisionInfo.wallSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( characterBody.bodyTransform.Up , collisionHitInfo.normal , characterBody.bodyTransform.Forward );
		collisionInfo.wallAngle = Mathf.Abs( collisionInfo.wallSignedAngle );
		collisionInfo.wallAngleSign = Mathf.Sign( collisionInfo.wallSignedAngle );
		collisionInfo.wallObject = collisionHitInfo.gameObject;

		// Against Right Wall
		if( collisionInfo.wallAngleSign == 1 )
		{
			if( !collisionInfo.right )
			{
				if(grounded)
				{
					if(OnGroundedRightCollision != null)
						OnGroundedRightCollision();
				}else
				{
					if(OnNotGroundedRightCollision != null)
						OnNotGroundedRightCollision();
				}

				if(OnRightCollision != null)
					OnRightCollision();
			} 

			collisionInfo.right = true;
		}
		else // Against Left Wall
		{
			if( !collisionInfo.left )
			{				
				if(grounded)
				{
					if(OnGroundedLeftCollision != null)
						OnGroundedLeftCollision();
				}else
				{
					if(OnNotGroundedLeftCollision != null)
						OnNotGroundedLeftCollision();
				}

				if(OnLeftCollision != null)
					OnLeftCollision();
			}				

			collisionInfo.left = true;
		}
	}
	
	/// <summary>
	/// Depenetrates the character from a steep slope when moving on the ground.
	/// </summary>
	/// <param name="penetration">The vertical penetration magnitude.</param>
	/// <param name="normal">The surface normal.</param>
	/// <returns>returns the correction vector used to depenetrate the character.</returns>
	Vector3 DepenetrateFromSteepSlope( float penetration , Vector3 normal )
	{	
		penetration = Mathf.Abs( penetration );	

		CalculateVerticalDirection();

		float newSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( 
			currentVerticalDirection , 
			normal , 
			characterBody.bodyTransform.Forward 
		);

		float oldSlopeAngle = collisionInfo.verticalSlopeAngle;
		float newSlopeAngle = Mathf.Abs( newSlopeSignedAngle );
		
		float depenetrationSign = - Mathf.Sign( newSlopeSignedAngle );
		
		float alpha = newSlopeAngle - oldSlopeAngle;
		float beta = 90 - Vector3.Angle( 
			characterBody.bodyTransform.Up , 
			normal
		);

		float correctionDistance = alpha != 0 ? 
			penetration * ( Mathf.Sin( beta * Mathf.Deg2Rad) / Mathf.Sin( alpha * Mathf.Deg2Rad) ) : 0; 

		Vector3 correctionVector = depenetrationSign * correctionDistance * collisionInfo.groundMovementDirection;
		characterBody.bodyTransform.Translate( correctionVector , Space.World );

		return correctionVector;		

	}

	/// <summary>
	/// It performs the ground alignment collision test and stores the information in the groundAlignInfo struct.
	/// </summary>
	void GroundRaysCollisionTest()
	{
		LayerMask groundedLayerMask = layerMaskSettings.profile.obstacles | layerMaskSettings.profile.oneWayPlatforms;

		groundAlignInfo.Reset();
		
		groundAlignInfo = characterCollisions.GroundRaysCollisions(
			characterBody.StepOffset , 
			groundAlignmentSettings.detectionDistance ,
			groundSettings.maxSlopeAngle ,
			groundedLayerMask
		);		
			

		
	}


	/// <summary>
	/// Align the player to the ground slope.
	/// </summary>
	void AlignToTheGround()
	{				
		Vector3 pivot = Vector3.zero;
		float rotationSignedAngle = 0;

		if( !IsGrounded )
			return;

		
		if( ( groundAlignInfo.leftRay.normal == groundAlignInfo.rightRay.normal ) && ( groundAlignInfo.leftRay.normal == characterBody.bodyTransform.Up ) )
			return;

		
		if( IsOnStableGround )
		{						
			if( 	groundAlignInfo.leftRay.verticalSlopeAngle > groundSettings.maxSlopeAngle ||
				groundAlignInfo.rightRay.verticalSlopeAngle > groundSettings.maxSlopeAngle )
			{
				AlignCharacterTowardsUp();
				return;
			}

			if( !groundAlignmentSettings.canAlignWithOneRay )
			{
				if( !groundAlignInfo.leftRay.collision || !groundAlignInfo.rightRay.collision )
				{				
					AlignCharacterTowardsUp();
					return;
				}
			}
		}
		else
		{
			if( !groundAlignInfo.leftRay.collision && !groundAlignInfo.rightRay.collision )			
				return;
			
		}

		if( groundAlignInfo.leftRay.collision && groundAlignInfo.rightRay.collision )
		{
			bool useRightPivot = groundAlignInfo.leftRay.distance > groundAlignInfo.rightRay.distance ;
			Vector2 leftToRight = groundAlignInfo.rightRay.point - groundAlignInfo.leftRay.point;

			rotationSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( characterBody.bodyTransform.Right , leftToRight , characterBody.bodyTransform.Forward );
			pivot = useRightPivot ? characterBody.bottomRightCollision : characterBody.bottomLeftCollision;
			
		} 


		if( !groundAlignInfo.leftRay.collision && groundAlignInfo.rightRay.collision )
		{
			rotationSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( characterBody.bodyTransform.Up , groundAlignInfo.rightRay.normal , characterBody.bodyTransform.Forward );
			pivot = characterBody.bottomRightCollision;
			
		} 
		
		if ( groundAlignInfo.leftRay.collision && !groundAlignInfo.rightRay.collision )
		{						
			rotationSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( characterBody.bodyTransform.Up , groundAlignInfo.leftRay.normal , characterBody.bodyTransform.Forward );
			pivot = characterBody.bottomLeftCollision;
			
		}	
		

		characterBody.bodyTransform.RotateAround( pivot , Vector3.forward , rotationSignedAngle );
			
	}

	void StableGroundProbing( bool setGroundData , bool depenetrateFromSlope )
	{
		CalculateVerticalDirection();

		float verticalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( currentVerticalDirection , collisionHitInfo.normal , characterBody.bodyTransform.Forward );
		float verticalSlopeAngle = Mathf.Abs( verticalSlopeSignedAngle );
	
		
		bool isStableOnGround = verticalSlopeAngle <= groundSettings.maxSlopeAngle;
		
		float distance = collisionHitInfo.distance - characterBody.StepOffset;


		if( collisionHitInfo.collision )
		{

			if( collisionInfo.onStableGround )	// previously on stable ground
			{
				if( distance > 0 )
				{
					if( isStableOnGround )
					{				
						characterBody.bodyTransform.Translate( Vector2.down * distance );
						if(setGroundData)
							FillGroundData( true , true );	
					}
					else
					{	
						if( verticalSlopeAngle == 90 )					
							return;


						if(setGroundData)
							FillGroundData( false , false );		
					}
				}
				else if( distance < 0 )	//a.k.a penetration
				{	
					if(isStableOnGround)
					{

						characterBody.bodyTransform.Translate( Vector2.down * distance );
						if(setGroundData)
							FillGroundData( true , true );
					}
					else
					{
						if( verticalSlopeAngle == 90 )					
							return;

						if( depenetrateFromSlope )
							DepenetrateFromSteepSlope( distance , collisionHitInfo.normal );
					}
				}

			}
			else	// previously on unstable ground ( onStableGround = false )
			{
				if( distance > 0 )
				{		
					if(isStableOnGround)
					{
						if(setGroundData)
							FillGroundData( true , true );
						characterBody.bodyTransform.Translate( Vector2.down * distance );
					}
					else
					{
						if( verticalSlopeAngle == 90 )					
							return;

						characterBody.bodyTransform.Translate( Vector2.down * distance );
						if(setGroundData)
							FillGroundData( true , false );
					}
				}
				else if( distance < 0 ) //a.k.a penetration
				{	
					
					if(isStableOnGround)
					{
						characterBody.bodyTransform.Translate( Vector2.down * distance );
						if(setGroundData)
							FillGroundData( true , true );
						
					}
					else
					{				
						if( verticalSlopeAngle == 90 )					
							return;		
						
						characterBody.bodyTransform.Translate( Vector2.down * distance );
						if(setGroundData)
							FillGroundData( true , false  );							
					}
		

				}

			}						
		}
		else
		{		
			if(setGroundData)				
				FillGroundData( false , true  );

		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// Ground Movement --------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	
	void StableGroundMovement( Vector3 deltaPosition )
	{
		MoveHorizontallyOnGround( deltaPosition );		
		ProbeGround( true , true );
	}

	void StableGroundAlignmentMovement( Vector3 deltaPosition )
	{		
		MoveHorizontallyOnGround( deltaPosition );
		
		GroundRaysCollisionTest();

		float leftRayPenetration = groundAlignInfo.leftRay.distance - characterBody.StepOffset;
		float rightRayPenetration = groundAlignInfo.rightRay.distance - characterBody.StepOffset;

		if( groundAlignInfo.leftRay.collision && !groundAlignInfo.leftRay.stable )
		{
			if( leftRayPenetration < 0 )
				DepenetrateFromSteepSlope( leftRayPenetration , groundAlignInfo.leftRay.normal );
		}
		else if( groundAlignInfo.rightRay.collision && !groundAlignInfo.rightRay.stable )
		{
			if( rightRayPenetration < 0 )
				DepenetrateFromSteepSlope( rightRayPenetration , groundAlignInfo.rightRay.normal );
		}
		else
		{
			AlignToTheGround();
		}
		
		ProbeGround( true , false );

	}

	void UnstableGroundMovement( Vector3 deltaPosition )
	{
		CalculateSlideVelocity( ref deltaPosition );
		MoveHorizontallyOnGround( deltaPosition );
		ProbeGround( true , true );
		
	}

	void UnstableGroundAlignmentMovement( Vector3 deltaPosition )
	{
		CalculateSlideVelocity( ref deltaPosition );
		MoveHorizontallyOnGround( deltaPosition );
		ProbeGround( false , false );
		ProbeGroundRays( deltaPosition );
				
	}

	void CalculatePostVelocity( bool wasStable )
	{
		if( IsGrounded )
		{
			if( !wasStable && collisionInfo.onStableGround )
			{
				//Unstable --> Stable
				if( !velocitySettings.unstableToStableContinuity )
				{
					ResetVelocity();
				}
			
			}

		}
		else
		{
			if( wasStable )
			{
				//Grounded --> Not Grounded
				if( velocitySettings.groundedToNotGroundedContinuity )
				{
					SetVelocity( lastGroundedVelocity , Space.World );
				}
				else
				{
					SetVelocityX( 0f );
				}

			}
			else
			{
				//Unstable --> Not Grounded
				if( velocitySettings.unstableToNotGroundedContinuity )
				{
					SetVelocity( lastSlidingVelocity , Space.World );
				}
				else
				{					
					ResetVelocity();
				}
			}

		}
		
		
	}



	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	
	void UnstableGroundProbing()
	{
		
		CalculateVerticalDirection();
		
		
		float distance = collisionHitInfo.distance - characterBody.StepOffset;
		characterBody.bodyTransform.Translate( Vector2.down * distance );


		
		GroundRaysCollisionTest();
		AlignToTheGround();


		if( IsOnRightVerticalSlope )
		{		

			collisionInfo.groundMovementDirection = Vector3.Cross( groundAlignInfo.rightRay.normal , Vector3.forward );

			collisionInfo.verticalSlopeSignedAngle = groundAlignInfo.rightRay.verticalSlopeSignedAngle;
			collisionInfo.verticalSlopeAngle = Mathf.Abs( collisionInfo.verticalSlopeSignedAngle );

			if( groundAlignInfo.rightRay.verticalSlopeAngle <= groundSettings.maxSlopeAngle  )
				collisionInfo.onStableGround = true;

		}
		else
		{			

			collisionInfo.groundMovementDirection = Vector3.Cross( groundAlignInfo.leftRay.normal , Vector3.forward );

			collisionInfo.verticalSlopeSignedAngle = groundAlignInfo.leftRay.verticalSlopeSignedAngle;
			collisionInfo.verticalSlopeAngle = Mathf.Abs( collisionInfo.verticalSlopeSignedAngle );

			if( groundAlignInfo.leftRay.verticalSlopeAngle <= groundSettings.maxSlopeAngle  )
				collisionInfo.onStableGround = true;
		}

	}


	void ProbeGroundRays( Vector3 deltaPosition )
	{
		GroundRaysCollisionTest();

		GroundDetectionRay testingRay = new GroundDetectionRay();

		if( !groundAlignInfo.leftRay.collision && !groundAlignInfo.rightRay.collision)
		{
			SetGroundDataSlopeAlignment( false , true , testingRay );
			return;
		}

		if( !groundAlignInfo.leftRay.collision && groundAlignInfo.rightRay.collision)
		{			
			testingRay = groundAlignInfo.rightRay;
		}
		else if( groundAlignInfo.leftRay.collision && !groundAlignInfo.rightRay.collision)
		{
			testingRay = groundAlignInfo.leftRay;
		}
		else
		{
			testingRay = deltaPosition.x > 0 ? groundAlignInfo.leftRay : groundAlignInfo.rightRay;
		}

		CalculateVerticalDirection();
		
		float verticalSlopeSignedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( currentVerticalDirection , testingRay.normal , characterBody.bodyTransform.Forward );
		float verticalSlopeAngle = Mathf.Abs( verticalSlopeSignedAngle );
		
				
		
		bool isStableOnGround = verticalSlopeAngle <= groundSettings.maxSlopeAngle;
		
		float distance = testingRay.distance - characterBody.StepOffset;

		if(testingRay.collision)
		{
			if( collisionInfo.onStableGround )	// previously on stable ground
			{
				if( distance >= 0 )
				{
					if( isStableOnGround )
					{				
						characterBody.bodyTransform.Translate( Vector2.down * distance );
						SetGroundDataSlopeAlignment( true , true , testingRay);	
					}
					else
					{	
						if( verticalSlopeAngle == 90 )					
							return;

						SetGroundDataSlopeAlignment( false , false , testingRay);		
					}
				}
				else
				{	
					if(isStableOnGround)
					{

						characterBody.bodyTransform.Translate( Vector2.down * distance );
						SetGroundDataSlopeAlignment( true , true , testingRay );
					}
					else
					{
						if( verticalSlopeAngle == 90 )					
							return;

						DepenetrateFromSteepSlope( distance , collisionHitInfo.normal );
					}
				}

			}
			else	// previously on unstable ground ( onStableGround = false )
			{
				if( distance >= 0 )
				{		
					if(isStableOnGround)
					{
						SetGroundDataSlopeAlignment( true , true , testingRay );
						characterBody.bodyTransform.Translate( Vector2.down * distance );
					}
					else
					{
						if( verticalSlopeAngle == 90 )					
							return;

						characterBody.bodyTransform.Translate( Vector2.down * distance );
						SetGroundDataSlopeAlignment( true , false , testingRay );
					}
				}
				else
				{	
					
					if(isStableOnGround)
					{
						characterBody.bodyTransform.Translate( Vector2.down * distance );
						SetGroundDataSlopeAlignment( true , true , testingRay );
						
					}
					else
					{				
						if( verticalSlopeAngle == 90 )					
							return;		
						
						characterBody.bodyTransform.Translate( Vector2.down * distance );
						SetGroundDataSlopeAlignment( true , false , testingRay );							
					}
		

				}
			}

			
		}
		else
		{						
			SetGroundDataSlopeAlignment( false , true , testingRay );

		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// Slope Alignment -------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	void SlopeAlignReverse()
	{		
		CalculateVerticalDirection();

		float signedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( characterBody.bodyTransform.Up , currentVerticalDirection , Vector3.forward);
		
		if( signedAngle < 0 )
		{
			characterBody.bodyTransform.RotateAround( characterBody.bottomRight  , characterBody.bodyTransform.Forward , signedAngle );
		}
		else
		{
			characterBody.bodyTransform.RotateAround( characterBody.bottomLeft , characterBody.bodyTransform.Forward , signedAngle );
		}
		
	}
	
	// -----------------------------------------------------------------------------------------------------------------
	// Dynamic Ground -------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Checks and updates the dynamic ground information of the character.
	/// </summary>
	void CheckDynamicGround()
	{
		if( dynamicGroundSettings.isEnabled )
		{
			if( IsGrounded && Lightbug.CoreUtilities.Utilities.BelongsToLayerMask( collisionInfo.groundLayer , layerMaskSettings.profile.dynamicGround ) )
			{	
				if( collisionInfo.groundObject != null )
				{
					if( collisionInfo.groundObject.transform != currentDynamicGroundInfo.targetTransform )
					{
						currentDynamicGroundInfo.UpdateTarget( collisionInfo.groundObject.transform );
						previousDynamicGroundInfo = currentDynamicGroundInfo;
					}
				}
				
				if( currentDynamicGroundInfo.targetTransform == null || 
				  ( currentDynamicGroundInfo.targetTransform != previousDynamicGroundInfo.targetTransform) )
				{
					currentDynamicGroundInfo.UpdateTarget( collisionInfo.groundObject.transform );
					previousDynamicGroundInfo = currentDynamicGroundInfo;
				}
			}
			else
			{
				currentDynamicGroundInfo.Reset();
				previousDynamicGroundInfo.Reset();

			}
		}
	}


	/// <summary>
	/// Method that performs the movement and rotation of the character while it is onto dynamic ground.
	/// </summary>
	void DynamicGroundMovement()
	{

		currentDynamicGroundInfo.UpdateInfo();

		
		if( previousDynamicGroundInfo.targetTransform != null )
		{
			float rotationAngle = currentDynamicGroundInfo.rotation.eulerAngles.z - previousDynamicGroundInfo.rotation.eulerAngles.z;

						
			characterBody.bodyTransform.RotateAround( 
				previousDynamicGroundInfo.position , 
				Vector3.forward , 
				rotationAngle
			);		
			

			characterBody.bodyTransform.RotateAround( 
				collisionInfo.groundContactPoint , 
				Vector3.forward , 
				- rotationAngle
			);	
			
		}

		
		Vector2 deltaPos = currentDynamicGroundInfo.position - previousDynamicGroundInfo.position;						
		characterBody.bodyTransform.Translate( deltaPos , Space.World );	
		
		previousDynamicGroundInfo = currentDynamicGroundInfo;
				
	}
		


	// --------------------------------------------------------------------------------------------------------------------------
	// Facing direction  --------------------------------------------------------------------------------------------------------
	// --------------------------------------------------------------------------------------------------------------------------
		
			
	/// <summary>
	/// It makes the character switch between left and right facing direction.
	/// </summary>
	public void LookToTheOppositeSide()
	{
		isFacingRight = !isFacingRight;		
	}

	/// <summary>
	/// It makes the character look to the right (positive horizontal direction).
	/// </summary>
	public void LookToTheRight()
	{
		isFacingRight = true;
	}

	/// <summary>
	/// It makes the character look to the left (negative horizontal direction).
	/// </summary>
	public void LookToTheLeft()
	{
		isFacingRight = false;
	}

		
	// --------------------------------------------------------------------------------------------------------------------------
	// Debug --------------------------------------------------------------------------------------------------------------------
	// --------------------------------------------------------------------------------------------------------------------------

	void OnDrawGizmos()
	{
		if( !debugSettings.drawGizmos )
			return;

		
		if( characterBody == null )
			characterBody = GetComponent<CharacterBody>();
				

		if( debugSettings.drawGroundAlignmentGizmos )
		{
			Gizmos.color = Color.green;

            Lightbug.CoreUtilities.Utilities.DrawArrowHead( 
				characterBody.bottomLeftCollision_StepOffset , 
				characterBody.bottomLeftCollision_StepOffset - characterBody.bodyTransform.Up * (characterBody.StepOffset + groundAlignmentSettings.detectionDistance ) ,
				Color.gray ,
				0.15f 
			);

            Lightbug.CoreUtilities.Utilities.DrawArrowGizmo( 
				characterBody.bottomRightCollision_StepOffset , 
				characterBody.bottomRightCollision_StepOffset - characterBody.bodyTransform.Up * (characterBody.StepOffset + groundAlignmentSettings.detectionDistance ) ,
				Color.gray ,
				0.15f  
			);

		}
		
		if( debugSettings.drawGroundMovementGizmos )
		{
            Lightbug.CoreUtilities.Utilities.DrawArrowGizmo( characterBody.bodyTransform.Position , characterBody.bodyTransform.Position + 1.5f * GroundMovementDirection , Color.magenta );
		}
		
		
		if( debugSettings.drawVerticalAlignmentGizmos )
		{
			CalculateVerticalDirection();		

			if( verticalAlignmentSettings.mode == VerticalAlignmentMode.Object )
			{
				if( verticalAlignmentSettings.verticalReferenceObject != null )
				{
                    Lightbug.CoreUtilities.Utilities.DrawCross( verticalAlignmentSettings.verticalReferenceObject.position , 0.5f , Color.red );
                    Lightbug.CoreUtilities.Utilities.DrawArrowGizmo( 
						characterBody.bodyTransform.Position , 
						characterBody.bodyTransform.Position + currentVerticalDirection * 2  ,
						Color.white 
					);
				}
			}
			else
			{
                Lightbug.CoreUtilities.Utilities.DrawArrowGizmo( 
					characterBody.bodyTransform.Position , 
					characterBody.bodyTransform.Position + currentVerticalDirection * 2  ,
					Color.white 
				);
			}
		}


		if( debugSettings.drawGroundClampingGizmos )
		{
			Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
			Matrix4x4 gizmoMatrix = Matrix4x4.TRS( transform.position, transform.rotation, transform.localScale);

			Gizmos.matrix *= gizmoMatrix;

			Vector3 delta = Vector3.down * 0.5f * groundSettings.groundClampingDistance;

					
			// Gizmos.DrawWireCube( deltaCenter , new Vector3( CharacterSize.x , CharacterSize.y , 0.001f ) );

			Gizmos.color = new Color( 0f , 1f , 0f , 0.25f );
			Gizmos.DrawCube( 
				delta  ,
				new Vector3( characterBody.horizontalArea , groundSettings.groundClampingDistance , characterBody.depth )
			);

			Gizmos.matrix = oldGizmosMatrix;

		}


		
	}

	

}

}

