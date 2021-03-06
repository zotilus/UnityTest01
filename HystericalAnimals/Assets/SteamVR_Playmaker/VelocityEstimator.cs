// (c) Copyright Dithernet 2016. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.SteamVR_FSM
{
    [RequireComponent(typeof(SteamVR_TrackedController))]
    [ActionCategory("SteamVR_FSM")]
    [Tooltip("Gets the Position and velocity of a controller and stores it in a Vector3 Variables or each Axis in a Float Variable.")]
public class VelocityEstimator: FsmStateAction
{
        
     private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)ChooseController.index); } }

    [RequiredField]
    [Tooltip("Choose the controller.")]
    public SteamVR_TrackedObject ChooseController;
    [UIHint(UIHint.Variable)]
	public FsmVector3 vector;
	[UIHint(UIHint.Variable)]
	public FsmFloat x;
	[UIHint(UIHint.Variable)]
	public FsmFloat y;
	[UIHint(UIHint.Variable)]
	public FsmFloat z;
    [UIHint(UIHint.Variable)]
    public FsmVector3 velocity;
    [UIHint(UIHint.Variable)]
    public FsmFloat xVelocity;
    [UIHint(UIHint.Variable)]
    public FsmFloat yVelocity;
    [UIHint(UIHint.Variable)]
    public FsmFloat zVelocity;	
	[UIHint(UIHint.Variable)]
	public FsmVector3 angularVelocity;
	[UIHint(UIHint.Variable)]
	public FsmFloat xAngularVelocity;
	[UIHint(UIHint.Variable)]
	public FsmFloat yAngularVelocity;
	[UIHint(UIHint.Variable)]
	public FsmFloat zAngularVelocity;
	
    public Space space;

	public bool everyFrame;

	public override void Reset()
	{
        ChooseController = null;
		vector = null;
        velocity = null;
        angularVelocity = null;
        x = null;
		y = null;
		z = null;
        xVelocity = null;
        yVelocity = null;
        zVelocity = null;
        xAngularVelocity = null;
        yAngularVelocity = null;
        zAngularVelocity = null;
        space = Space.World;
		everyFrame = false;
	}

	public override void OnEnter()
	{
          
          DoGetPosition();

		if (!everyFrame)
		{
			Finish();
		}		
	}

	public override void OnUpdate()
	{
           DoGetPosition();
	}

	void DoGetPosition()
	{

        var go = ChooseController;
		if (go == null)
		{
			return;
		}

		var position = space == Space.World ? go.transform.position : go.transform.localPosition;
       
        vector.Value = position;
		x.Value = position.x;
		y.Value = position.y;
		z.Value = position.z;

        velocity.Value = controller.velocity;
        xVelocity.Value = controller.velocity.x;
        yVelocity.Value = controller.velocity.y;
        zVelocity.Value = controller.velocity.z;
		
		angularVelocity.Value = controller.angularVelocity;
		xAngularVelocity.Value = controller.angularVelocity.x;
		yAngularVelocity.Value = controller.angularVelocity.y;
		zAngularVelocity.Value = controller.angularVelocity.z;

        }
}
}
