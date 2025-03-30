using Sandbox;
using Sandbox.Diagnostics;
using Spring.Input;
using Spring.UI.Screen;
using Spring.Utils;

namespace Spring.Components
{
    public class Door : Component, IInteractable
    {
        // Config Params
        [Property, Category("Angles"), Title("Open Out Angle")]
        private float mOpenOutAngle = 90f;
        [Property, Category("Angles"), Title("Closed Angle")]
        private float mClosedAngle = 0f;
        [Property, Category("Angles"), Title("Open In Angle")]
        private float mOpenInAngle = -90f;
		[Property, Category("Angles"), Title("Closed threshold Angle")]
		private float mClosedThresholdAngle = 2f;

		[Property, Title("Stopped Velocity")]
        private float mStoppedVelocity = 0.1f;
        [Property, Title("Min Duration")]
        private float mMinDuration = 0.1f;

        [Property, Title("Sticky")]
        private bool mSticky = false;

		[Property, Title("Monodirectional")]
		private bool mMonoDirectional = false; // This assumes that the 'open in' angle has been set to 0

		// Cached Components
		HingeJoint mHinge;
        Rigidbody mRigidBody;

        // State
        private enum State
        {
            OpenOut,
            OpeningOut,
            Closed,
            Closing,
            OpenIn,
            OpeningIn
        }

        [Property, Category("State"), Title("Current State"), ReadOnly]
        private State mCurrentState = State.Closed;
        private float mMoveStartTime = -1;
    

        protected override void OnAwake()
        {
            mHinge = this.MustGetComponentInChildren<HingeJoint>();
            mRigidBody = this.MustGetComponent<Rigidbody>();
        }

        protected override void OnFixedUpdate()
        {
			if (DoorInMotion())
			{
				if (mCurrentState == State.OpeningOut)
				{
					mHinge.Motor = HingeJoint.MotorMode.TargetAngle;
					mHinge.TargetAngle = mOpenOutAngle;
				}
				else if (mCurrentState == State.OpeningIn)
				{
					mHinge.Motor = HingeJoint.MotorMode.TargetAngle;
					mHinge.TargetAngle = mOpenInAngle;
				}
				else if (mCurrentState == State.Closing)
				{
					mHinge.Motor = HingeJoint.MotorMode.TargetAngle;
					mHinge.TargetAngle = mClosedAngle;
				}

				float elapsedTime = Time.Now - mMoveStartTime;

				// If the door has finished opening/closing, we disable the motor so that any physics interactions can happen nicely
				if (mRigidBody.AngularVelocity.Length < mStoppedVelocity && elapsedTime > mMinDuration && !mSticky)
				{
					FinishMotionState();
					mHinge.Motor = HingeJoint.MotorMode.Disabled;
				}
			}
        }


		private State DetermineOpenCloseDirection(Vector3 pActivatorPos)
		{
			float currentAngle = mHinge.Angle;
			State desiredState = mCurrentState;

			if ((currentAngle <= mClosedThresholdAngle && currentAngle >= -mClosedThresholdAngle))
			{
				// Rotate activator's position relative to the door's rotation
				Vector3 relativeActivatorPos = pActivatorPos.RotateAround(WorldPosition, WorldRotation.Inverse);

				if (relativeActivatorPos.x <= WorldPosition.x || mMonoDirectional)
					desiredState = State.OpeningOut;
				else
					desiredState = State.OpeningIn;
			}
			else if (currentAngle > mClosedThresholdAngle) // Opening out
			{
				if (currentAngle > mOpenOutAngle / 2) // If we're more than half open, assume we want to close
					desiredState = State.Closing;
				else
					desiredState = State.OpeningOut;
			}
			else if (currentAngle < -mClosedThresholdAngle) // Opening in
			{
				if (currentAngle < mOpenInAngle / 2) // If we're more than half open, assume we want to close
					desiredState = State.Closing;
				else
					desiredState = State.OpeningIn;
			}

			return desiredState;
		}

		void IInteractable.OnLookFixedUpdate(ref InteractEvent pEvent)
		{
			if (ShouldOpenClose())
			{
				OpenClose(DetermineOpenCloseDirection(pEvent.mIteractor.WorldPosition));
			}
		}

		void IInteractable.OnLookUpdate(ref InteractEvent pEvent)
		{
			State potentialState = DetermineOpenCloseDirection(pEvent.mIteractor.WorldPosition);
			string text = "";

			if (potentialState == State.Closing)
				text = Localiser.GetString("ACTION_CLOSE");
			else
				text = Localiser.GetString("ACTION_OPEN");

			UIController.mUIPromptController.SetTextOverride(InputDef.openDoor, text);
		}

		void IInteractable.OnLookStart(ref InteractEvent pEvent)
		{
			UIController.mUIPromptController.SetPromptVisible(InputDef.openDoor, true);
		}

		void IInteractable.OnLookEnd(ref InteractEvent pEvent)
		{
			UIController.mUIPromptController.SetPromptVisible(InputDef.openDoor, false);
		}

		private bool ShouldOpenClose()
        {
            return Sandbox.Input.Pressed(InputDef.openDoor.ToString());
        }

		private void OpenClose(State pNewState)
		{
			Assert.True(pNewState == State.OpeningIn || pNewState == State.OpeningOut || pNewState == State.Closing); // Should always be set to a "motion" state
			mCurrentState = pNewState;
			mMoveStartTime = Time.Now;
		}

        private bool DoorInMotion()
        {
            return mCurrentState == State.OpeningIn || mCurrentState == State.OpeningOut || mCurrentState == State.Closing;
        }

        private void FinishMotionState()
        {
            mCurrentState = (mCurrentState == State.Closing ? State.Closed : (mCurrentState == State.OpeningIn ? State.OpenIn : State.OpenOut));
        }
    }
}
