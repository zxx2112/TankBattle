using UnityEngine;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Implementation
{

public enum AIBehaviourType
{
	Sequence ,
	Follow	
}

[System.Serializable]
public class CharacterAIAction
{
	[Range_NoSlider( true )]
	public float duration = 1;

	[ CustomClassDrawer ]
	public CharacterActionInfo action = new CharacterActionInfo();
}

[System.Obsolete("This component is obsolete" )]
[AddComponentMenu("Kinematic2D/Implementation/Brain/AI Brain")]
public class CharacterAIBrain : CharacterBrain
{
	public AIBehaviourType behaviourType;

	[Header("Sequence Behaviour")]
	
	[SerializeField]
	CharacterAISequenceBehaviour sequenceBehaviour = null;

	[Header("Follow Behaviour")]

	[Tooltip("This field is used when the character needs to reach a \"target\" GameObject.")]
	[SerializeField] Transform targetObject = null;

	[SerializeField] float reachDistance = 1f;

	[Tooltip("The wait time between actions updates.")]
	[Range_NoSlider(true)] [SerializeField] float refreshTime = 0.5f;
	

	int currentActionIndex = 0;

	float waitTime = 0f;
	float time = 0f;

	public override bool IsAI()
	{
		return true;
	}

	void Awake()
	{		
		switch( behaviourType )
		{
			case AIBehaviourType.Sequence:

				if(sequenceBehaviour == null)
				{
					Debug.Log("Missing sequence behaviour!");
					return;
				}					

			break;

			case AIBehaviourType.Follow:

				if(targetObject == null)
				{
					Debug.Log("Missing target object!");
					return;
				}

				waitTime = refreshTime;				

			break;
		}

	}

	

	protected override void Update()
	{		
		
		if( time >= waitTime )
		{
			switch( behaviourType )
			{
				case AIBehaviourType.Sequence:

					UpdateSequenceBehaviour();					

				break;

				case AIBehaviourType.Follow:

					UpdateFollowBehaviour();					

				break;
			}
			
			time = 0;			
		}
		else
		{
			time += Time.deltaTime;
		}

		
		
		
	}

	// Sequence Behaviour --------------------------------------------------------------------------------------------------

	void UpdateSequenceBehaviour()
	{
		if(sequenceBehaviour == null)
		{
			return;
		}

		characterAction.Reset();
		characterAction = sequenceBehaviour.ActionSequence[currentActionIndex].action;		
		waitTime = sequenceBehaviour.ActionSequence[currentActionIndex].duration;

		if( currentActionIndex == ( sequenceBehaviour.ActionSequence.Count - 1 ) )
			currentActionIndex = 0;
		else
			currentActionIndex++;
	}


	// Follow Behaviour --------------------------------------------------------------------------------------------------

	void UpdateFollowBehaviour()
	{
		if( targetObject == null )
			return;
		
		characterAction.Reset();

		
		float signedAngle = Lightbug.CoreUtilities.Utilities.SignedAngle( transform.up , Vector3.up , Vector3.forward );
		Vector3 delta = targetObject.position - transform.position;
		Vector3 rotatedDelta = Quaternion.AngleAxis( signedAngle , Vector3.forward ) * delta;

				
		if( Mathf.Abs( rotatedDelta.x ) <= reachDistance )
		{			
			characterAction.Reset();
			return;
		}
		else
		{
			if( rotatedDelta.x > 0 )
			{
				characterAction.right = true;
			}
			else
			{
				characterAction.left = true;
			}

		}		

		
	}

	

	

	
	
	
}

}