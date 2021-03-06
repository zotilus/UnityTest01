// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;
using Valve.VR;

namespace HutongGames.PlayMaker.SteamVR_FSM
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
	[Tooltip("Gets the pressed state of the system Button and stores it in a Bool Variable.")]
public class GetSystem : FsmStateAction
	{
        private EVRButtonId systemButton = EVRButtonId.k_EButton_System;          
                       
        private bool systemButtonDown;
        private bool systemButtonUp;
        private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)ChooseController.index); } }
        
        [RequiredField]
        [Tooltip("Choose the controller.")]
        public SteamVR_TrackedObject ChooseController;

        [UIHint(UIHint.Variable)]
        [Tooltip("The Bool variable to store value.")]
        public FsmBool storeValue;

        [Tooltip("Event to send if the Bool variable is True.")]
        public FsmEvent isTrue;

        [Tooltip("Event to send if the Bool variable is False.")]
        public FsmEvent isFalse;


        public bool everyFrame;        

        public override void Reset()
        {
            ChooseController = null;
            storeValue = false;
            everyFrame = false;
       
        }
        public override void OnEnter()
        {
             if (!everyFrame)

            {
                Finish();
            }

        }
        public override void OnUpdate()
        {
           if (controller == null)
            {
                Debug.Log("Le controlleur n'est pas initialisé");
                return;
            }

            systemButtonDown = controller.GetPressDown(systemButton);
            systemButtonUp = controller.GetPressUp(systemButton);          


            if (systemButtonDown)
            {
                storeValue.Value = true;      

            }

            if (systemButtonUp)
             {
                storeValue.Value = false;              

             }
                      Fsm.Event(storeValue.Value ? isTrue : isFalse);

        }
        
    }
}
