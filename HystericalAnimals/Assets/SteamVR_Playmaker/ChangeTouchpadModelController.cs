// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
    [Tooltip("Change the touchpad of the controller model.")]
    public class ChangeTouchpadModelController : FsmStateAction
    {
        [RequiredField]
        [Tooltip("Choose the touchpad model controller.")]
        public FsmOwnerDefault ChooseController;
        [Tooltip("Choose the touchpad of the controller. Do not place touchpad model in the model parent.")]
        public FsmGameObject newTouchpad;

        public override void Reset()
        {
            ChooseController = null;
            newTouchpad = null;
        }

        public override void OnUpdate()
        {
            var go = Fsm.GetOwnerDefaultTarget(ChooseController);
            var count = go.transform.childCount;
            var count2 = go.transform.Find("Model").childCount;

            if (count == 0)
            {
                return;
            }

            if (count2 >= 16)
            {
                var trackpad = go.transform.Find("Model").Find("trackpad").gameObject;

                if (trackpad != null)
                {
                    Object.Destroy(trackpad);
                }

                Finish();
            }
        }
    }
}
