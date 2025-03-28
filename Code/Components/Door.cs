using Sandbox;
using Sandbox.Diagnostics;
using Spring.Input;
using Spring.Utils;

namespace Spring.Components
{
    public class Door : Component
    {
        // Config Params
        [Property, Category("Angles"), Title("Open Out Angle")]
        private float mOpenOutAngle = 90f;
        [Property, Category("Angles"), Title("Closed Angle")]
        private float mClosedAngle = 0f;
        [Property, Category("Angles"), Title("Open In Angle")]
        private float mOpenInAngle = -90f;

        [Property, Title("Stopped Velocity")]
        private float mStoppedVelocity = 0.1f;
        [Property, Title("Min Duration")]
        private float mMinDuration = 0.1f;

        [Property, Title("Sticky")]
        private bool mSticky = false;

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

     //   private Timest
    

        protected override void OnAwake()
        {
            mHinge = this.MustGetComponentInChildren<HingeJoint>();
            mRigidBody = this.MustGetComponent<Rigidbody>();
        }

        protected override void OnUpdate()
        {
            if (ShouldOpenClose())
            {
                // TODO - we need to actually do a raycast from the player to determine if they're in range. ATM this will just trigger all doors everywhere...
                // Perhaps we can abstract some of the logic out of GrabController to have a generic "RangeActivator" class ?
                if (!DoorInMotion())
                {
                    mCurrentState = mCurrentState == State.Closed ? State.OpeningOut : State.Closing;
                    mMoveStartTime = Time.Now;
                }
            }

            // We're assuming here that Frequency & Damping Ratio have been set on the hinge already.
            // TODO - Determine next state based on current door angle + player position.
            // e.g. if door is closed, it should open away from player. If door is almost open, it should close, etc.
        
            if (mCurrentState == State.OpeningOut)
            {
                mHinge.Motor = HingeJoint.MotorMode.TargetAngle;
                mHinge.TargetAngle = mOpenOutAngle;
            }
            else if (mCurrentState == State.Closing)
            {
                mHinge.Motor = HingeJoint.MotorMode.TargetAngle;
                mHinge.TargetAngle = mClosedAngle;
            }

            float elapsedTime = Time.Now - mMoveStartTime;

            // If the door has finished opening/closing, we disable the motor so that any physics interactions can happen nicely
            if (DoorInMotion() && mRigidBody.AngularVelocity.Length < mStoppedVelocity && elapsedTime > mMinDuration && !mSticky)
            {
                FinishMotionState();
                mHinge.Motor = HingeJoint.MotorMode.Disabled;
            }
        }

        private bool ShouldOpenClose()
        {
            return Sandbox.Input.Pressed(InputDef.openDoor.ToString());
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
