using UnityEngine;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Core
{

public enum YawRotationMode
{
	FacingPositiveZ ,
	FacingNegativeZ ,
	Positive ,
	Negative ,
	Random
}

public enum FacingDirectionMode
{
	NegativeLocalScale ,
	Rotation
}

public class CharacterGraphics : MonoBehaviour
{
	

	[Header("Roll")]

	[SerializeField] 
	bool enableRoll = true;

	[SerializeField] 
	bool lerpRoll = true;

	[SerializeField] 
	float groundedRollSlerpFactor = 20;

	[SerializeField] 
	float notGroundedRollSlerpFactor = 10;

	[Header("Facing Direction")]

	[SerializeField] 
	FacingDirectionMode facingDirectionMode = FacingDirectionMode.NegativeLocalScale;

	[Header("Yaw")]	

	[SerializeField] 
	bool enableYaw = true;

	[SerializeField] 
	float groundedYawSlerpFactor = 20;

	[SerializeField] 
	float notGroundedYawSlerpFactor = 10;	

	[SerializeField] 
	YawRotationMode yawMode = YawRotationMode.FacingNegativeZ;
	
	[Header("Smash/Squeeze")]

	[SerializeField] 
	bool enableSmashSqueeze = false;

	[Tooltip("How much the character will reduce its width (and increse its height) when the smash/squeeze effect is maximum.")]
	[SerializeField] 
	[Range( 0f , 1f )]
	float smashSqueezeRate = 0.15f;

	[Tooltip("The maximum vertical speed magnitude detected. Whenever the character's vertical speed is equal " + 
	"to the maxVerticalSpeed value the smash/squeeze effect is maximum.")]
	[SerializeField]
	[Range_NoSlider(true)]
	float maxVerticalSpeed = 10f;


	CharacterMotor characterMotor = null;
	Transform characterTransform = null;

	Vector3 positionOffset;

	Vector3 initialScale = Vector3.zero;

	bool isFacingRight = true;
	bool positiveYaw = true;

	float currentYawAngle = 0;

	float yawRotationAmountLeft = 0;

	void Start()
	{		
		characterTransform = transform.parent;
		characterMotor = characterTransform.GetComponent<CharacterMotor>();
		
		positionOffset = transform.localPosition;

		initialScale = transform.localScale;

		transform.parent = null;

		isFacingRight = characterMotor.IsFacingRight;

		if( !isFacingRight )
		{
			if( facingDirectionMode == FacingDirectionMode.NegativeLocalScale )
				transform.localScale = new Vector3( - transform.localScale.x , transform.localScale.y , transform.localScale.z);
			else if( facingDirectionMode == FacingDirectionMode.Rotation )
				transform.Rotate( 0 , 180  , 0 );
		}
	}

	

	void LateUpdate()
	{
		if( enableSmashSqueeze )
			Squeeze();
		
		if( facingDirectionMode == FacingDirectionMode.NegativeLocalScale )
			Flip();
		else
		{
			if( enableYaw )
				Yaw();
		}
		
		if( enableRoll )
			Roll();

		

		transform.position = characterTransform.position + 
		positionOffset.x * transform.right + 
		positionOffset.y  * transform.up +
		positionOffset.z * transform.forward;
		
	}

	void Flip()
	{
		
		if( characterMotor.IsFacingRight == isFacingRight )
			return;
		
		isFacingRight = characterMotor.IsFacingRight;

		transform.localScale = new Vector3( - transform.localScale.x , transform.localScale.y , transform.localScale.z);
		
	}

	void SetYawRotation()
	{
		if( characterMotor.IsFacingRight == isFacingRight )
			return;

		isFacingRight = characterMotor.IsFacingRight;
		

		switch( yawMode )
		{
			case YawRotationMode.FacingNegativeZ:

				if( isFacingRight )
					positiveYaw = false;
				else
					positiveYaw = true;

				break;

			case YawRotationMode.FacingPositiveZ:

				if( isFacingRight )
					positiveYaw = true;
				else
					positiveYaw = false;

				break;

			case YawRotationMode.Positive:

				positiveYaw = true;
				break;

			case YawRotationMode.Negative:

				positiveYaw = false;
				break;

			case YawRotationMode.Random:

				float seed = Random.Range( 0 , 2 );
				positiveYaw = seed == 1 ;

			break;
		}

		float clampedEulerAngleY = transform.eulerAngles.y - ( (int)( transform.eulerAngles.y / 360 ) ) * 360f;
		currentYawAngle = clampedEulerAngleY;


		if( isFacingRight)
		{
			if( positiveYaw )
				yawRotationAmountLeft = 360 - clampedEulerAngleY;
			else
				yawRotationAmountLeft = - clampedEulerAngleY;

		}
		else
		{
			if( positiveYaw )
				yawRotationAmountLeft = clampedEulerAngleY <= 180 ? 180 - clampedEulerAngleY : 540 - currentYawAngle;
			else
				yawRotationAmountLeft = clampedEulerAngleY <= 180 ? -180 - clampedEulerAngleY : 180 - currentYawAngle;

		}			
		
	}

	void Yaw()
	{
		SetYawRotation();

				
		float rotationAmount = Mathf.Lerp(
			0 , 
			yawRotationAmountLeft , 
			( characterMotor.IsGrounded ? groundedYawSlerpFactor : notGroundedYawSlerpFactor ) * Time.deltaTime
		);
		

		transform.Rotate( 0 , rotationAmount  , 0 );

		yawRotationAmountLeft -= rotationAmount;
	}

	void Roll()
	{
		float targetRollAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( transform.up , characterTransform.up , characterTransform.forward);	
				
		float rollAngle = lerpRoll ? Mathf.Lerp(
			0 , 
			targetRollAngle , 
			( characterMotor.IsGrounded ? groundedRollSlerpFactor : notGroundedRollSlerpFactor ) * Time.deltaTime
		) : targetRollAngle;		

		transform.rotation = Quaternion.AngleAxis( rollAngle , characterTransform.forward ) * transform.rotation;
				

	}

	void Squeeze()
	{
		float verticalSpeed = Mathf.Abs( characterMotor.Velocity.y );

		float squeezeAmountX = 1 - smashSqueezeRate * Mathf.Clamp01( verticalSpeed / maxVerticalSpeed );
		float squeezeAmountY = 1 + smashSqueezeRate * Mathf.Clamp01( verticalSpeed / maxVerticalSpeed );

		float signX = Mathf.Sign( transform.localScale.x );

		transform.localScale = new Vector3(
			signX * initialScale.x * squeezeAmountX ,
			initialScale.y * squeezeAmountY , 
			initialScale.z 
		);

	}
	
}

}
