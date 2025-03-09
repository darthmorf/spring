using Sandbox.Diagnostics;
using Spring.Input;
using Spring.Debug;
using Spring.Utils;
using System;

namespace Spring.Components
{
    class GrabController : Component
    {
		// Config Properties
		[Property, Category("Grab"), Title("Range")]
		private float mGrabRange = 100f;
		[Property, Category("Grab"), Title("Force")]
		private float mGrabForce = 100f;
		[Property, Category("Grab"), Title("Min Force Distance")]
		private float mMinForceDistance = 1f;
		[Property, Category("Grab"), Title("Grabbed Angular Damping")]
		private float mGrabbedAngularDamping = 20f;
		[Property, Category("Grab"), Title("Min Reset Rotation")]
		private float mMinResetRotation = 0.05f;
		[Property, Category("Grab"), Title("Reset Rotation Speed")]
		private float mResetRotationSpeed = 0.1f;

		// State
		private enum State
		{
			EmptyHanded,
			Grabbing,
			GrabbedRotate
		}
		private State mCurrentState;
		private Vector3 mCachedEyePosition = Vector3.Zero;
		private Rotation mCachedCameraRotation = Rotation.Identity;
		private GameObject mGrabObject = null;
		private Rigidbody mGrabRigidBody = null;
		private float mGrabbedDistance = 0f;
		private float mCachedAngularDamping = 0f;

		// Cached Components
		private PlayerController mPlayerController;


		// Methods
		protected override void OnAwake()
		{
			mPlayerController = Components.GetInAncestorsOrSelf<PlayerController>();
			Assert.NotNull(mPlayerController);
		}

		protected override void OnUpdate()
		{
			// It's important that we call drawing logic in OnUpdate rather than OnFixedUpdate otherwise we get flickering
			ShowGrabbable();
		}

		protected override void OnFixedUpdate()
		{
			// If we're not currently grabbing anything, find something to grab
			if (mCurrentState == State.EmptyHanded)
			{
				FindGrabbable();

				// and grab it (if we want to)
				if (WantsToGrabOrDrop() && CanGrab())
					Grab(mGrabObject);
			}
			else if (mCurrentState == State.Grabbing || mCurrentState == State.GrabbedRotate)
			{
				// Otherwise if we want to drop it, drop it
				if (WantsToGrabOrDrop())
				{
					Drop();
				}
				// If we need to rotate to "neutral" rotation, temporarily swap to that state
				else if (WantsToRotateGrabbed())
				{
					mCurrentState = State.GrabbedRotate;
				}
				else
				{
					// Else we need to update the position of the grabbed item
					UpdateGrab();

					// If we need to rotate, also rotate
					if (mCurrentState == State.GrabbedRotate)
						RotateGrabbedOject();
				}
			}
		}

		private void ShowGrabbable()
		{
			if (mGrabRigidBody == null)
				return;

			// TODO - non debug draw
			SpringGizmo.DrawLineBBox(mGrabRigidBody.PhysicsBody.GetBounds(), 
				mCurrentState == State.EmptyHanded ? DebugSettings.GrabController_Grabbable : 
					(mCurrentState == State.Grabbing ? DebugSettings.GrabController_Grabbed : DebugSettings.GrabController_GrabbedRotate));
		}

		private void Grab(GameObject pTarget)
		{
			Assert.NotNull(pTarget);

			mGrabObject = pTarget;
			mCurrentState = State.Grabbing;

			// It's important we setup some angular damping or the picked up object will end up spinning around
			mCachedAngularDamping = mGrabRigidBody.AngularDamping;
			mGrabRigidBody.AngularDamping = mGrabbedAngularDamping;

			Vector3 source = GetSourcePos();
			mGrabbedDistance = source.Distance(mGrabObject.WorldPosition);
		}

		// Attempts to find an object with the Grabbable tag via raycast
		private void FindGrabbable()
		{
			Vector3 source = GetSourcePos();
			Rotation cameraRotation = mPlayerController.EyeAngles.ToRotation();

			// If the camera hasn't moved, don't bother raycasting again
			if (mCachedEyePosition == source && mCachedCameraRotation == cameraRotation)
				return;

			mCachedEyePosition = source;
			mCachedCameraRotation = cameraRotation;

			// Calculate vector from camera to "look at point" for the raycast
			Vector3 lookDirection = cameraRotation.Forward * mGrabRange;
			Vector3 destination = source + lookDirection;
			SceneTrace trace = Scene.Trace.Ray(source, destination);
			trace = trace.WithTag(Tag.Grabbable.ToString());

			SpringGizmo.DrawLine(source, destination, DebugSettings.GrabController_GrabRaycast);
			SceneTraceResult traceResult = trace.IgnoreStatic().Run();

			// If we hit something, update object references
			if (traceResult.Hit)
			{
				mGrabObject = traceResult.GameObject;
				Assert.NotNull(mGrabObject);

				mGrabRigidBody = mGrabObject.GetComponent<Rigidbody>();
				Assert.NotNull(mGrabRigidBody);
			}
			// Otherwise clear the references
			else
			{
				mGrabObject = null;
				mGrabRigidBody = null;
			}
		}

		private void UpdateGrab()
		{
			// Calculate vector from camera to desired object centre
			Vector3 source = GetSourcePos();
			Rotation cameraRotation = mPlayerController.EyeAngles.ToRotation();
			Vector3 lookDirection = cameraRotation.Forward * mGrabbedDistance;
			Vector3 destination = source + lookDirection;

			// Calculate the vector from the current object's position to the desired position
			Vector3 distance = destination - mGrabObject.WorldPosition;

			SpringGizmo.DrawLine(mGrabObject.WorldPosition, destination, DebugSettings.GrabController_GrabbedForce);

			// If the distance between the object's location and the desired location is big enough, move it towards our desired location
			if (distance.Length > mMinForceDistance)
			{
				mGrabRigidBody.Velocity = distance * mGrabForce;
			}
		}

		private void RotateGrabbedOject()
		{
			Rotation desiredRotation = mPlayerController.EyeAngles.ToRotation();
			float delta = MathF.Abs(desiredRotation.Angle() - mGrabObject.WorldRotation.Angle());

			// If we're rotated, do a tick of smooth rotate towards 0 rotation, otherwise go back to grabbed state
			if (delta > mMinResetRotation)
				mGrabRigidBody.SmoothRotate(desiredRotation, mResetRotationSpeed, Time.Delta);
			else
				mCurrentState = State.Grabbing;
		}

		private bool WantsToGrabOrDrop()
		{
			return Sandbox.Input.Pressed(InputDefs.grab.ToString());
		}

		private bool WantsToRotateGrabbed()
		{
			return Sandbox.Input.Pressed(InputDefs.resetGrabbedRotation.ToString());
		}

		private bool CanGrab()
		{
			return mGrabObject != null;
		}

		private void Drop()
		{
			mGrabRigidBody.AngularDamping = mCachedAngularDamping;
			mGrabRigidBody = null;
			mGrabObject = null;
			mCurrentState = State.EmptyHanded;
		}

		Vector3 GetSourcePos()
		{
			return mPlayerController.EyePosition;
		}
	}
}
