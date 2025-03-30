using Sandbox.Diagnostics;
using Spring.Input;
using Spring.Debug;
using Spring.Utils;
using System;
using Spring.UI.Screen;
using Sandbox;

namespace Spring.Components
{
    class GrabController : Component
	{
		// Config Properties
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
		private Rotation mGrabStartObjectRotation = Rotation.Identity;
		private Rotation mGrabStartCameraRotation = Rotation.Identity;
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
			if (mCurrentState == State.EmptyHanded)
			{
				// If we have something to grab and want to, grab it. Interact logic is handled by InteractableController + Grabbable classes
				if (WantsToGrabOrDrop() && IsLookingAtObject() && !StandingOnGrababble())
					Grab(mGrabObject);
			}
			else if (mCurrentState == State.Grabbing || mCurrentState == State.GrabbedRotate)
			{
				// If we want to drop it, or we're now standing on the object, drop it
				if (WantsToGrabOrDrop() || StandingOnGrababble())
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

		public void OnLookStart(GameObject pLookingAtObject)
		{
			if (mCurrentState == State.EmptyHanded) // If we're already grabbing an object, we don't want something else stealing the grab
			{ 
				UpdateGrabbable(pLookingAtObject);
			}
		}

		public void OnLookEnd()
		{
			if (mCurrentState == State.EmptyHanded) // If we're already grabbing an object, ignore our camera temporarily looking away
			{
				UpdateGrabbable(null);
			}
		}

		private void ShowGrabbable()
		{
			UIController.mUIPromptController.SetPromptEnabled(InputDef.grab, !StandingOnGrababble());
			UIController.mUIPromptController.SetPromptVisible(InputDef.grab, IsLookingAtObject());
			UIController.mUIPromptController.SetPromptVisible(InputDef.resetGrabbedRotation, mCurrentState == State.Grabbing && mGrabObject.MustGetComponent<Grabbable>().mCanBeReset);

			UIController.mUIPromptController.SetTextOverride(InputDef.resetGrabbedRotation, Localiser.GetString("ACTION_RESET_ROTATION"));

			if (mCurrentState == State.EmptyHanded)
				UIController.mUIPromptController.SetTextOverride(InputDef.grab, Localiser.GetString("ACTION_GRAB"));
			else
				UIController.mUIPromptController.SetTextOverride(InputDef.grab, Localiser.GetString("ACTION_DROP"));

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

			// Cache off the object's start rotation so that we can rotate it properly as the camera rotates
			mGrabStartObjectRotation = mGrabRigidBody.WorldRotation;
			mGrabStartCameraRotation = mPlayerController.EyeAngles.ToRotation();

			Vector3 source = GetSourcePos();
			mGrabbedDistance = source.Distance(mGrabObject.WorldPosition);
		}

		// Updates the current grabbable item based of raycast in InteractibleController
		private void UpdateGrabbable(GameObject pGameObject)
		{
			if (pGameObject != null)
			{
				mGrabObject = pGameObject;
				Assert.NotNull(mGrabObject);

				mGrabRigidBody = mGrabObject.MustGetComponent<Rigidbody>();
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

				// We don't want to apply the camera's pitch to the object
				cameraRotation.SetPitch(0f);
				mGrabStartCameraRotation.SetPitch(0f);

				// Apply relative rotation
				Rotation cameraRotationDelta = Rotation.Difference(cameraRotation, mGrabStartCameraRotation);
				Rotation desiredRotation = cameraRotationDelta.Inverse * mGrabStartObjectRotation;
				mGrabRigidBody.SmoothRotate(desiredRotation, mResetRotationSpeed, Time.Delta);
			}
		}

		private void RotateGrabbedOject()
		{
			Rotation desiredRotation = mPlayerController.EyeAngles.ToRotation();
			float delta = MathF.Abs(desiredRotation.Angle() - mGrabObject.WorldRotation.Angle());

			// If we can reset, then update the cached positions to reflect the desired rotation
			if (delta > mMinResetRotation)
			{
				// We always want the pitch to be relative to 0, not the camera
				desiredRotation.SetPitch(0);
				mGrabStartObjectRotation = desiredRotation;
				mGrabStartCameraRotation = mPlayerController.EyeAngles.ToRotation();
			}
			else
			{
				mCurrentState = State.Grabbing;
			}
		}

		private bool WantsToGrabOrDrop()
		{
			return Sandbox.Input.Pressed(InputDef.grab.ToString());
		}

		private bool WantsToRotateGrabbed()
		{
			if (!mGrabObject.MustGetComponent<Grabbable>().mCanBeReset)
				return false;

			return Sandbox.Input.Pressed(InputDef.resetGrabbedRotation.ToString());
		}

		private bool IsLookingAtObject()
		{
			return mGrabObject != null;
		}

		private bool StandingOnGrababble()
		{
			return IsLookingAtObject() && mPlayerController.GroundObject == mGrabObject;	
		}

		private void Drop()
		{
			mGrabRigidBody.AngularDamping = mCachedAngularDamping;
			mGrabStartObjectRotation = Rotation.Identity;
			mGrabStartCameraRotation = Rotation.Identity;
			mCurrentState = State.EmptyHanded;
		}

		Vector3 GetSourcePos()
		{
			return mPlayerController.EyePosition;
		}
	}

	// Simple class that forwards the interactable events from the grabbed object back to the GrabbableController
	class Grabbable : Component, IInteractable
	{
		// Config Params
		[Property, Title("Resettable")]
		public bool mCanBeReset = true;

		// State
		GrabController mCurrentGrabController;

		void IInteractable.OnLookStart(ref InteractEvent pEvent) 
		{
			mCurrentGrabController = pEvent.mIteractor.MustGetComponent<GrabController>();
			mCurrentGrabController.OnLookStart(pEvent.mIteractee);
		}

		void IInteractable.OnLookEnd(ref InteractEvent pEvent)
		{
			if (mCurrentGrabController != null)
				mCurrentGrabController.OnLookEnd();
		}
	}
}
