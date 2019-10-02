using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( menuName = "Kinematic 2D/Implementation/Movement/Vertical Movement Profile" ) , System.Serializable ]
public class VerticalMovementProfile : ScriptableObject
{

	[Header("Jumping and Falling")]	


	[Range_NoSlider(true)] 
	[Tooltip("Time to reach the jump height.")]
	public float jumpDuration = 0.4f;

	[Range_NoSlider(true)] 
	[Tooltip("the jump height measured from the ground.")]
	public float jumpHeight = 2.5f;

	[Range_NoSlider( true )] 
	[Tooltip("This value will be multiplied to the character vertical velocity everytime a new vertical movement profile is loaded (this is used by the movement areas)")]
	public float entrySpeedMultiplier = 1f;
	
	[Range_NoSlider( true )]
	[Tooltip("This value will be multiplied to the gravity calculated (from the jump height and duration), affecting the descending movement. " + 
	"Useful for simulating different jumping and falling behaviours.")]
	public float descendingGravityMultiplier = 10f;

	

}
