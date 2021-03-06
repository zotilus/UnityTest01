// (c) Copyright Dithernet 2016. All rights reserved.


using UnityEngine;

namespace HutongGames.PlayMaker.SteamVR_FSM
{

    [ActionCategory("SteamVR_FSM")]
[Tooltip("Gets the rotation of a controller and stores it in a Vector3 Variable or each Axis in a Float Variable.")]
public class GetControllerRotation : FsmStateAction
{
	[RequiredField]
   [Tooltip("Choose the controller.")]
    public FsmOwnerDefault ChooseController;
	[UIHint(UIHint.Variable)]
	public FsmQuaternion quaternion;
	[UIHint(UIHint.Variable)]
	[Title("Euler Angles")]
	public FsmVector3 vector;
	[UIHint(UIHint.Variable)]
	public FsmFloat xAngle;
	[UIHint(UIHint.Variable)]
	public FsmFloat yAngle;
	[UIHint(UIHint.Variable)]
	public FsmFloat zAngle;
	public Space space;
	public bool everyFrame;

	public override void Reset()
	{
        ChooseController = null;
		quaternion = null;
		vector = null;
		xAngle = null;
		yAngle = null;
		zAngle = null;
		space = Space.World;
		everyFrame = false;
	}

	public override void OnEnter()
	{
		DoGetRotation();

		if (!everyFrame)
		{
			Finish();
		}		
	}

	public override void OnUpdate()
	{
		DoGetRotation();
	}

	void DoGetRotation()
	{
		var go = Fsm.GetOwnerDefaultTarget(ChooseController);
		if (go == null)
		{
			return;
		}

		if (space == Space.World)
		{
			quaternion.Value = go.transform.rotation;

			var rotation = go.transform.eulerAngles;

			vector.Value = rotation;
			xAngle.Value = rotation.x;
			yAngle.Value = rotation.y;
			zAngle.Value = rotation.z;
		}
		else
		{
			var rotation = go.transform.localEulerAngles;

			quaternion.Value = Quaternion.Euler(rotation);

			vector.Value = rotation;
			xAngle.Value = rotation.x;
			yAngle.Value = rotation.y;
			zAngle.Value = rotation.z;
		}
	}


}
}