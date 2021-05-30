// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;
using Valve.VR;


namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SteamVR_FSM")]
    [Tooltip("Recenters the HMD by key.")]
    public class RecenterHMDByKey : FsmStateAction
    {
       
        [RequiredField]
        public KeyCode key;
        public override void Reset()
        {
            key = KeyCode.None;
        }

        public override void OnUpdate()
        {
            bool keyDown = Input.GetKeyDown(key);
            if (keyDown)
            {
              OpenVR.System.ResetSeatedZeroPose();
            
            }
            
           
        }

    }
}