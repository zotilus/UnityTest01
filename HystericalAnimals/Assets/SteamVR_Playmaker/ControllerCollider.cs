// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR_FSM")]
    [Tooltip("Set a collider on head controller")]
    [HelpUrl("")]
    public class ControllerCollider : ComponentAction<Rigidbody>
    {
        [RequiredField]
        [CheckForComponent(typeof(SphereCollider))]
        [Tooltip("The GameObject to apply the collider to.")]
        public FsmOwnerDefault controller;


        FsmFloat radius = 0.05f;
        FsmFloat y = -0.03f;
        Vector3 center;

        public override void Reset()
        {
            controller = null;
            radius = 0.05f;          
            y = -0.03f;
           

        }

        public override void OnEnter()
        {
            DoChange();
           
        }

        public override void OnFixedUpdate()
        {
            DoChange();
        }

        void DoChange()
        {
            var go = Fsm.GetOwnerDefaultTarget(controller);
            SphereCollider collider = go.GetComponent<SphereCollider>();
            center.y = y.Value;
            collider.center = center;
            collider.radius = radius.Value;
            Finish();
        }
        
    }
}
