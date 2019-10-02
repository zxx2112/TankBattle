using UnityEngine;
using Lightbug.CoreUtilities;

namespace Lightbug.Kinematic2D.Implementation
{

public class CharacterHybridBrain : CharacterBrain
{
    
    [SerializeField]
    bool isAI = false;


    // Human brain ----------------------------------------------------------------------------
    public CharacterInputData inputData;


    // AI brain -------------------------------------------------------------------------------
    public AIBehaviourType behaviourType;

	
	[SerializeField]
	CharacterAISequenceBehaviour sequenceBehaviour = null;


	[Tooltip("This field is used when the character needs to reach a \"target\" GameObject.")]
	[SerializeField] Transform targetObject = null;

	[SerializeField] float reachDistance = 1f;

	[Tooltip("The wait time between actions updates.")]
	[Range_NoSlider(true)] [SerializeField] float refreshTime = 0.5f;
	
    //------------------------------------------------------------------------------------------------
  
	int currentActionIndex = 0;

	float waitTime = 0f;
	float time = 0f;
	
    void Awake()
    {
        SetBrainType( isAI );
    }
    
    public void SetBrainType( bool AI )
    {
        if( !AI )
        {
            if( inputData == null )
            {
                Debug.Log( "The input data field is null" );
                return;
            }
            
        }
        else
        {
            SetAIBehaviour( behaviourType );        
        }

        this.isAI = AI;
        
        ResetActions();

    }

    public void SetAIBehaviour( AIBehaviourType type )
    {
        switch( type )
        {
            // Sequence
            case AIBehaviourType.Sequence:

                if(sequenceBehaviour == null)
                {
                    Debug.Log( "Follow behaviour is null" );
                    return;
                }

                currentActionIndex = 0;
                	

            break;

            // FOllow
            case AIBehaviourType.Follow:

			if(targetObject == null)
			{
				Debug.Log( "Sequence behaviour is null" );
				return;
			}                

				waitTime = refreshTime;

            break;
        }
	

        behaviourType = type;	

        time = 0;
    }
	

	public override bool IsAI()
	{
		return isAI;
	}
	
		
	/// <summary>
	/// Checks for human inputs and updates the action struct.
	/// </summary>
	protected override void Update()
	{

		if( isAI )
            UpdateAIBrain();
		else
            UpdateHumanBrain();		

	}

    #region Human

    void UpdateHumanBrain()
    {
		
        if( inputData == null || Time.timeScale == 0 )
			return;

		characterAction.right |= GetAxis( inputData.horizontalAxisName ) > 0;
		characterAction.left |= GetAxis( inputData.horizontalAxisName ) < 0;
		characterAction.up |= GetAxis( inputData.verticalAxisName ) > 0;
		characterAction.down |= GetAxis( inputData.verticalAxisName ) < 0;
		
		characterAction.jumpPressed |= GetButtonDown( inputData.jumpName );
		characterAction.jumpReleased |= GetButtonUp( inputData.jumpName );

		characterAction.dashPressed |= GetButtonDown( inputData.dashName );
		characterAction.dashReleased |= GetButtonUp( inputData.dashName );

		characterAction.jetPack |= GetButton( inputData.jetPackName );
    }

	
	protected virtual float GetAxis( string axisName , bool raw = true )
	{
		return raw ? Input.GetAxisRaw( axisName ) : Input.GetAxis( axisName );
	}

	protected virtual bool GetButton( string actionInputName )
	{
		return Input.GetButton( actionInputName );
	}

	protected virtual bool GetButtonDown( string actionInputName )
	{
		return Input.GetButtonDown( actionInputName );
	}

	protected virtual bool GetButtonUp( string actionInputName )
	{
		return Input.GetButtonUp( actionInputName );
	}

    #endregion


    #region AI
    
    void UpdateAIBrain()
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

    #endregion
}

}
