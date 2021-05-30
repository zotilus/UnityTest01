// (c) Copyright Dithernet 2016. All rights reserved.

using Valve.VR;


namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("SteamVR_FSM")]
	[Tooltip("Recenters the HMD.")]
	public class RecenterHMD : FsmStateAction
	{
        
        
		public override void OnUpdate()
		{
            OpenVR.System.ResetSeatedZeroPose();
            Finish();

        }			
	}
}