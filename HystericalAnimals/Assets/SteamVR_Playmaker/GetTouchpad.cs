// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;
using Valve.VR;

namespace HutongGames.PlayMaker.SteamVR_FSM
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
	[Tooltip("Gets the pressed state of the touchpad Button and stores it in a Bool Variable.")]
public class GetTouchpad : FsmStateAction
	{
        
        private EVRButtonId touchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
        private bool touchpadButtonDown;
        private bool touchpadButtonUp;     
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

        [Tooltip("The intensity of the vibration. Do not exceed 3999.")]
        public FsmInt intensity;
        private ushort vibrat;


        public bool everyFrame;        

        public override void Reset()
        {
            ChooseController = null;
            storeValue = false;
            everyFrame = false;
            intensity = 1000;

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

            touchpadButtonDown = controller.GetPressDown(touchpadButton);
            touchpadButtonUp = controller.GetPressUp(touchpadButton);
            vibrat = (ushort)intensity.Value;


            if (touchpadButtonDown)
            {
                storeValue.Value = true;
                controller.TriggerHapticPulse(vibrat);

            }

            if (touchpadButtonUp)
             {
                storeValue.Value = false;
            

             }

                      Fsm.Event(storeValue.Value ? isTrue : isFalse);

        }

    }
}
