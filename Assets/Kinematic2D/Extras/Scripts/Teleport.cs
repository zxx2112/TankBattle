using UnityEngine;
using Lightbug.CoreUtilities;


namespace Lightbug.Kinematic2D.Extras
{

using Lightbug.Kinematic2D.Core;

public class Teleport : MonoBehaviour
{

	public LayerMask layerMask;
	public Transform targetTransform;
	[SerializeField] bool resetCharacterVelocity = true;

	void OnTriggerEnter2D(Collider2D other)
	{
        if ( !Lightbug.CoreUtilities.Utilities.BelongsToLayerMask( other.gameObject.layer , layerMask) )
			return;

		CharacterMotor characterMotor = other.GetComponent<CharacterMotor>();
		if( characterMotor != null )
			DoAction(characterMotor);
		
	}

	void OnTriggerEnter(Collider other)
	{
		if( !Lightbug.CoreUtilities.Utilities.BelongsToLayerMask( other.gameObject.layer , layerMask) )
			return;

		CharacterMotor characterMotor = other.GetComponent<CharacterMotor>();
		if( characterMotor != null )
			DoAction(characterMotor);
		
	}

	
	void DoAction( CharacterMotor characterMotor)
	{
		characterMotor.Teleport( targetTransform.position , targetTransform.rotation );
			
		if( resetCharacterVelocity )
			characterMotor.ResetVelocity();
	}
}

}