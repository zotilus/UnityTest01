// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.SteamVR_FSM
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
	[Tooltip("Set the intensity and duration of the vibration.")]
public class Haptic : FsmStateAction
	{        
        private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)ChooseController.index); } }

        [RequiredField]
        [Tooltip("Choose the controller.")]
        public SteamVR_TrackedObject ChooseController;        

        [Tooltip("The intensity of the vibration. Do not exceed 3999.")]
        public FsmInt intensity;
        private ushort vibrat;

        [Tooltip("Duration of the vibration in seconds.")]
        public FsmFloat time;
        public bool realTime;
        private float startTime;
        private float timer;
        public bool everyFrame;        

        public override void Reset()
        {
            time = 1f;
            realTime = false;
            ChooseController = null;
            everyFrame = false;
            intensity = 1000;

        }
        public override void OnEnter()
        {
             if (!everyFrame)

            {
                Finish();
            }

            startTime = FsmTime.RealtimeSinceStartup;
            timer = 0f;

        }
        public override void OnUpdate()
        {
           if (controller == null)
            {
                Debug.Log("Le controlleur n'est pas initialisé");
                return;
            }


            if (realTime)
            {
                timer = FsmTime.RealtimeSinceStartup - startTime;
            }
            else
            {
                timer += Time.deltaTime;
            }

            vibrat = (ushort)intensity.Value;
            controller.TriggerHapticPulse(vibrat);

            if (timer >= time.Value)
            {
                Finish();

            }                          

        }

    }
}
