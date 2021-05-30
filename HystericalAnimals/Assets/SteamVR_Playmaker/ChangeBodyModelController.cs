// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
    [Tooltip("Change the body of the controller model.")]
    public class ChangeBodyModelController : FsmStateAction
    {
        [RequiredField]
        [Tooltip("Choose the model controller.")]
        public FsmOwnerDefault ChooseController;
        [Tooltip("Choose the body of the controller. Do not place body Model in the model parent.")]
        public FsmGameObject newBodyModel;

        public override void Reset()
        {
            ChooseController = null;
            newBodyModel = null;
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
                var body = go.transform.Find("Model").Find("body").gameObject;

                if (body != null)
                {
                    Object.Destroy(body);
                }

                Finish();
            }
        }
    }
}
