using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lightbug.Kinematic2D.Implementation
{

/// <summary>
/// The character input signals sent by the CharacterInput2D script component.
/// </summary>
[System.Serializable]
public struct CharacterActionInfo 
{ 		
	
	public bool right;
	public bool left;
	public bool up;
	public bool down;

	public bool jumpPressed;
	public bool jumpReleased;

	public bool dashPressed;
	public bool dashReleased;

	public bool jetPack;

	public void Reset()
	{
		right = false;
		left = false;
		up = false;
		down = false;
		jumpPressed = false;
		jumpReleased = false;
		dashPressed = false;
		dashReleased = false;
		jetPack = false;
	}

	public bool isEmpty()
	{
		return !right &&
		!left &&
		!up &&
		!down &&
		!jumpPressed &&
		!jumpReleased &&
		!dashPressed &&
		!dashReleased &&
		!jetPack;
	}

}


public abstract class CharacterBrain : MonoBehaviour
{	
	
	protected CharacterActionInfo characterAction = new CharacterActionInfo();
	public CharacterActionInfo CharacterAction
	{
		get
		{
			return characterAction;
		}
	}
	 
	public abstract bool IsAI();	
	

	public void ResetActions()
	{
		characterAction.Reset();
	}

	public void SetAction( CharacterActionInfo characterAction )
	{
		this.characterAction = characterAction;
	}

	protected abstract void Update();

	void OnDisable()
	{
		ResetActions();
	}
}

}
