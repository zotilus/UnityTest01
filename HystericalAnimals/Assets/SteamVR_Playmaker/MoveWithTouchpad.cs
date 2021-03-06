// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;
using Valve.VR;

namespace HutongGames.PlayMaker.SteamVR_FSM
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
	[Tooltip("Gets the touched state of the touchpad Button and stores it in a Bool Variable.")]
public class MoveWithTouchpad : FsmStateAction
	{
        
        private EVRButtonId touchpadButton = EVRButtonId.k_EButton_SteamVR_Touchpad;
  
        private bool touchPadTouched = false;
        private Vector2 touchPadPos;        

        private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)ChooseController.index); } }       

        [RequiredField]
        [Tooltip("Choose the controller.")]
        public SteamVR_TrackedObject ChooseController;

                     
        private FsmEvent TouchIsTrue;       
        private FsmEvent TouchIsFalse;

        [Tooltip("The intensity of the vibration. Do not exceed 3999.")]
        public FsmInt intensity;
        private ushort vibrat;
        [UIHint(UIHint.Variable)]
        public FsmFloat x;
        [UIHint(UIHint.Variable)]
        public FsmFloat y;
        [UIHint(UIHint.Variable)]
        public FsmFloat z;
        [UIHint(UIHint.Variable)]
        public FsmVector3 vector;

        [Tooltip("The moving speed multiplier.")]
        public FsmFloat speed;

        [Tooltip("Translate in local or world space.")]
        public Space space;

        [Tooltip("Translate over one second")]
        public bool perSecond;

        public bool everyFrame;        

        public override void Reset()
        {
            ChooseController = null;
            everyFrame = true;
            perSecond = true;
            vector = null;
            x = new FsmFloat { UseVariable = true };
            y = new FsmFloat { UseVariable = true };
            z = new FsmFloat { UseVariable = true };
            intensity = 200;
            speed = 1;

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
            touchPadPos = controller.GetAxis(touchpadButton);
            vibrat = (ushort)intensity.Value;
            vector.Value = touchPadPos;
            x.Value = touchPadPos.x;
            z.Value = touchPadPos.y;


            if (touchPadTouched)
             {

                var go = ChooseController.transform.parent.gameObject;
                if (go == null)
                {
                    return;
                }
                                
                var translate = vector.IsNone ? new Vector3(x.Value, y.Value, z.Value) : vector.Value;
                translate.x = x.Value;
                translate.y = 0;
                translate.z = z.Value;
                var vitesse = speed.Value;
                                
                if (!perSecond)
                {
                    controller.TriggerHapticPulse(vibrat);
                    go.transform.Translate(translate * vitesse, space);
                }
                else
                {
                    controller.TriggerHapticPulse(vibrat);
                    go.transform.Translate(translate * Time.deltaTime * vitesse, space);
                }

            }
             
        } 
    }
}
