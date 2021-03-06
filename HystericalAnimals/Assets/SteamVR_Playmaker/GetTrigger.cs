// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;
using Valve.VR;

namespace HutongGames.PlayMaker.SteamVR_FSM
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
	[Tooltip("Gets the pressed state of the trigger Button and stores it in a Bool Variable.")]
public class GetTrigger : FsmStateAction
	{        
        private EVRButtonId triggerButton = EVRButtonId.k_EButton_SteamVR_Trigger;
        private bool triggerButtonPressed = false;
        private Vector2 triggerbuttonPos;           
        private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)ChooseController.index); }  }       

        [RequiredField]
        [Tooltip("Choose the controller.")]
        public SteamVR_TrackedObject ChooseController;

        [UIHint(UIHint.Variable)]
        public FsmFloat Curse;

        [Tooltip("Event to send if the Bool variable is True.")]
        public FsmEvent TouchDownIsTrue;

        [Tooltip("Event to send if the Bool variable is False.")]
        public FsmEvent TouchDownIsFalse;

        [Tooltip("The intensity of the vibration touch. Do not exceed 3999.")]
        public FsmInt intensityTouch;

        [Tooltip("Event to send if the Bool variable is True.")]
        public FsmEvent PressIsTrue;

        [Tooltip("Event to send if the Bool variable is False.")]
        public FsmEvent PressIsFalse;

        [UIHint(UIHint.Variable)]
        [Tooltip("The Bool variable to store value.")]
        public FsmBool storeTouchValue;

        [UIHint(UIHint.Variable)]
        [Tooltip("The Bool variable to store value.")]
        public FsmBool storePressedValue;
        
        [Tooltip("The intensity of the vibration. Do not exceed 3999.")]
        public FsmInt intensity;

        private ushort vibrat;
        private ushort vibratTouch;

        public bool everyFrame;

        public override void Reset()
        {
            ChooseController = null ;
            storePressedValue = false;
            storeTouchValue = false;
            everyFrame = false;
            intensity = 1000;
            intensityTouch = 200;   

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

            triggerButtonPressed = controller.GetPress(triggerButton);
            triggerbuttonPos = controller.GetAxis(triggerButton);
            vibrat = (ushort)intensity.Value;
            vibratTouch = (ushort)intensityTouch.Value;
            Curse.Value = triggerbuttonPos.x;
            
            
           if (triggerbuttonPos.x > 0)
            {
                storeTouchValue.Value = true;
                controller.TriggerHapticPulse(vibratTouch);
            }
            else
            {
                storeTouchValue.Value = false;
            }

            Fsm.Event(storeTouchValue.Value ? TouchDownIsTrue : TouchDownIsFalse);

            if (triggerButtonPressed)
            {
                storePressedValue.Value = true;
                controller.TriggerHapticPulse(vibrat);
            }
            else
            {
                storePressedValue.Value = false;
            }

            Fsm.Event(storePressedValue.Value ? PressIsTrue : PressIsFalse);            

        }

    }
}
