// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;
using Valve.VR;

namespace HutongGames.PlayMaker.SteamVR_FSM
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
	[Tooltip("Gets the pressed state of the menu Button and stores it in a Bool Variable.")]
public class GetMenu : FsmStateAction
	{
        private EVRButtonId menuButton = EVRButtonId.k_EButton_ApplicationMenu;   
        private bool menuButtonDown;
        private bool menuButtonUp;
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

            menuButtonDown = controller.GetPressDown(menuButton);
            menuButtonUp = controller.GetPressUp(menuButton);
            vibrat = (ushort)intensity.Value;


            if (menuButtonDown)
            {
                storeValue.Value = true;
                controller.TriggerHapticPulse(vibrat);
            }

            if (menuButtonUp)
             {
                storeValue.Value = false;
             }

                      Fsm.Event(storeValue.Value ? isTrue : isFalse);

        }
        
    }
}
