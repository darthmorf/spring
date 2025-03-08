using Sandbox.Diagnostics;
using Spring.Debug;
using Spring.Utils;

namespace Spring.Components
{
    class GrabController : Component
    {
		// Debug Properties
		public static DebugProperty dGrabRaycast = new DebugProperty(Color.Red, true, 2.0f);
		public static DebugProperty dGrabbable = new DebugProperty(Color.Green);
		public static DebugProperty dGrabbed = new DebugProperty(Color.Cyan);
		public static DebugProperty dGrabbedForce = new DebugProperty(Color.Cyan);

		// Config Properties
		[Property, Category("Grab"), Title("Range")]
		private float mGrabRange = 100f;
		[Property, Category("Grab"), Title("Force")]
		private float mGrabForce = 100f;
		[Property, Category("Grab"), Title("Min Force Distance")]
		private float mMinForceDistance = 1f;
		[Property, Category("Grab"), Title("Grabbed Angular Damping")]
		private float mGrabbedAngularDamping = 20f;

		// State
		private enum State
		{
			EmptyHanded,
			Grabbing
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
			if (mCurrentState == State.EmptyHanded)
			{
				if (CanGrab())
					ShowGrabbable(mGrabObject, false);
			}
			else if (mCurrentState == State.Grabbing)
			{
				ShowGrabbable(mGrabObject, true);
			}
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
			else if (mCurrentState == State.Grabbing)
			{
				// Otherwise if we want to drop it, drop it
				if (WantsToGrabOrDrop())
				{
					Drop();
					return;
				}

				// Else we need to update the position of the grabbed item
				UpdateGrab(mGrabObject);
			}
		}

		private void ShowGrabbable(GameObject pTarget, bool pGrabbed)
		{
			// TODO - non debug draw
			SpringGizmo.DrawLineBBox(mGrabRigidBody.PhysicsBody.GetBounds(), pGrabbed ? dGrabbed : dGrabbable);
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
			trace = trace.WithTag(TagDefs.Tag.Grabbable.AsString());

			SpringGizmo.DrawLine(source, destination, dGrabRaycast);
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

		private void UpdateGrab(GameObject pTarget)
		{
			// Calculate vector from camera to desired object centre
			Vector3 source = GetSourcePos();
			Rotation cameraRotation = mPlayerController.EyeAngles.ToRotation();
			Vector3 lookDirection = cameraRotation.Forward * mGrabbedDistance;
			Vector3 destination = source + lookDirection;

			// Calculate the vector from the current object's position to the desired position
			Vector3 distance = destination - pTarget.WorldPosition;

			SpringGizmo.DrawLine(pTarget.WorldPosition, destination, dGrabbedForce);

			// If the distance between the object's location and the desired location is big enough, move it towards our desired location
			if (distance.Length > mMinForceDistance)
			{
				mGrabRigidBody.Velocity = distance * mGrabForce;
			}
		}

		private bool WantsToGrabOrDrop()
		{
			return Input.Pressed("grab"); // TODO - make a class to manage these strings much like TagDefs.
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
