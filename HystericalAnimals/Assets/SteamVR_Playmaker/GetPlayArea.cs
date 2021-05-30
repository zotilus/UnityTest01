// (c) Copyright Dithernet 2016. All rights reserved.


using Valve.VR;


namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("SteamVR_FSM")]
	[Tooltip("Get the x,z size of the paly area defined in SteamVR.")]
	public class GetPlayArea : FsmStateAction
	{
        
        [RequiredField]
        [Tooltip("Choose the CameraRig.")]
        public SteamVR_PlayArea cameraRig;

        [UIHint(UIHint.Variable)]
        public FsmFloat sizeX;

        [UIHint(UIHint.Variable)]
        public FsmFloat sizeZ;

        [UIHint(UIHint.Variable)]
        public FsmFloat sizeY;

        private float x;
        private float z;

        public bool everyFrame;


        public override void OnEnter()
        {
            if (!everyFrame)

            {
                Finish();
            }

        }

        public override void OnUpdate()
		{
             sizeX.Value = x;
             sizeZ.Value = z;
           
             OpenVR.Chaperone.GetPlayAreaSize(ref x, ref z);
             sizeY.Value = cameraRig.wireframeHeight; 

        }			
	}
}