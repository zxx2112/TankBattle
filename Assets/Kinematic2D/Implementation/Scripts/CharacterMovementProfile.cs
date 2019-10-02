using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lightbug.Kinematic2D.Implementation
{


[CreateAssetMenu( menuName = "Kinematic 2D/Implementation/Movement/Movement Profile" )]
public class CharacterMovementProfile : ScriptableObject
{	

	[Header("Horizontal Data")]
	public HorizontalMovementProfile horizontalMovementData;

	[Header("Vertical Data")]
	public VerticalMovementProfile verticalMovementData;

}


}
