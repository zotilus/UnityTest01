// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;
using Valve.VR;

namespace HutongGames.PlayMaker.SteamVR_FSM
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
	[Tooltip("Gets the touched state of the touchpad Button and stores it in a Bool Variable.")]
public class GetTouchpadPosition : FsmStateAction
	{
        
        private EVRButtonId touchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
  
        private bool touchPadTouched = false;
        private Vector2 touchPadPosition;
        

        private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)ChooseController.index); } }       

        [RequiredField]
        [Tooltip("Choose the controller.")]
        public SteamVR_TrackedObject ChooseController;

        [UIHint(UIHint.Variable)]
        [Tooltip("The Bool variable to store value.")]
        public FsmBool storeValue;

        [Tooltip("Event to send if the Bool variable is True.")]
        public FsmEvent TouchIsTrue;

        [Tooltip("Event to send if the Bool variable is False.")]
        public FsmEvent TouchIsFalse;

        [Tooltip("The intensity of the vibration. Do not exceed 3999.")]
        public FsmInt intensity;
        private ushort vibrat;

        [UIHint(UIHint.Variable)]
        public FsmVector2 vector;

        [UIHint(UIHint.Variable)]
        public FsmFloat x;

        [UIHint(UIHint.Variable)]
        public FsmFloat y;


        public bool everyFrame;        

        public override void Reset()
        {
            ChooseController = null;
            storeValue = false;
            everyFrame = false;
            vector = null;
            x = null;
            y = null;
            intensity = 200;

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


             touchPadTouched = controller.GetTouch(touchpadButton);
             touchPadPosition = controller.GetAxis(touchpadButton);
             vibrat = (ushort)intensity.Value;
             vector.Value = touchPadPosition;
             x.Value = touchPadPosition.x;
             y.Value = touchPadPosition.y;


            if (touchPadTouched)
             {
                 storeValue.Value = true;
                 controller.TriggerHapticPulse(vibrat);

             }
             else
             {
                
                storeValue.Value = false;

            }

                       Fsm.Event(storeValue.Value ? TouchIsTrue : TouchIsFalse);

        }



    }
}
