// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
    [Tooltip("Change the model of the controller.")]
    public class ChangeModelController : FsmStateAction
    {
        [RequiredField]
        [Tooltip("Choose the model controller.")]
        public FsmOwnerDefault ChooseController;
        [Tooltip("Choose the model of the controller. Do not place model in the model parent.")]
        public FsmGameObject newModel;

        public override void Reset()
        {
        ChooseController = null;
        newModel = null;
        }

        public override void OnUpdate()
        {
            var go = Fsm.GetOwnerDefaultTarget(ChooseController);
            var model = go.transform.Find("Model").gameObject;
            var child = Fsm.GetOwnerDefaultTarget(ChooseController);
            var count = child.transform.childCount;

            if (count == 0)
            {
                return;
            }                          
                  Object.Destroy(model); 
                  Finish();
            }
      }
}
