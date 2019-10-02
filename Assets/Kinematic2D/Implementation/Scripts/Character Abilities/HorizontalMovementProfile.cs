using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( menuName = "Kinematic 2D/Implementation/Movement/Horizontal Movement Data" ) , System.Serializable ]
public class HorizontalMovementProfile : ScriptableObject
{
	[Header("Movement")]
	
	[Tooltip( "Walk speed in units per second.")]
	[Range_NoSlider(true)] 
	public float walkSpeed = 5f;

	[Tooltip( "Time for the character to reach the desired walk speed.")]
	[Range_NoSlider(true)] 
	public float startDuration = 0.2f;	

	[Tooltip( "Time for the character to stop moving.")]
	[Range_NoSlider(true)] 
	public float stopDuration = 0.2f;
   
	[Tooltip( "Air control = 0 -> no control while the character is not grounded." + 
	"Air control = 1 -> full control while the character is not grounded.")]
	[Range_NoSlider( 0f , 1f )] 
	public float airControl = 0.7f; 

	[Tooltip("This value will be multiplied to the character horizontal velocity everytime a new horizontal movement profile is loaded (this is used by the movement areas)")]
	[Range_NoSlider( true )] 
	public float entrySpeedMultiplier = 1f;

	

	

	
	
}
