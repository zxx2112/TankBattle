using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lightbug.Kinematic2D.Core;

namespace Lightbug.Kinematic2D.Implementation
{	


public abstract class CharacterAbility : MonoBehaviour 
{
     //delegate
	public delegate void CharacterAbilityEvent();

     
     protected CharacterBody characterBody;
     protected CharacterBrain characterBrain;
     protected CharacterController2D characterController2D;   
     
     protected StateMachineController<MovementState> movementController; 
     protected StateMachineController<PoseState> poseController; 

     public bool isEnabled = true;
         

     protected virtual void Awake()
     {          
          characterBrain = GetComponent<CharacterBrain>();
          // characterMotor = GetComponent<CharacterMotor>();
          characterBody = GetComponent<CharacterBody>();
          characterController2D = GetComponent<CharacterController2D>();

          movementController = characterController2D.MovementController;
          poseController = characterController2D.PoseController;

     }

	public virtual void Process( float dt ){}

     public virtual string GetInfo()
	{ 
		return ""; 
	}
	
}

}
