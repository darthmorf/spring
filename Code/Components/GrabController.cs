using Sandbox;
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

		// Config Properties
		[Property, Category("Grab"), Title("Range")]
		private float mPickupRange = 100f;

		// State
		private enum State
		{
			EmptyHanded,
			Grabbing
		}
		private State mCurrentState;
		private GameObject mGrabbedObject = null;
		private GameObject mLookingAtObject = null;
		private Vector3 mCachedEyePosition = Vector3.Zero;
		private Rotation mCachedCameraRotation = Rotation.Identity;

		// Cached Components
		private PlayerController mPlayerController;


		// Methods
		protected override void OnAwake()
		{
			mPlayerController = Components.GetInAncestorsOrSelf<PlayerController>();
		}

		protected override void OnUpdate()
		{
			if (mCurrentState == State.EmptyHanded)
			{
				if (CanGrab())
					ShowGrabbable(mLookingAtObject); // It's important that we call this in OnUpdate rather than OnFixedUpdate otherwise we get flickering.
			}
		}

		protected override void OnFixedUpdate()
		{
			if (mCurrentState == State.EmptyHanded)
			{
				FindGrabbable();

				if (WantsToGrab() && CanGrab())
					Grab(mLookingAtObject);
			}
		}

		private bool WantsToGrab()
		{
			return Input.Down("grab"); // TODO - make a class to manage these strings much like TagDefs.
		}

		private bool CanGrab()
		{
			return mLookingAtObject != null;
		}

		private void FindGrabbable()
		{
			Vector3 source = mPlayerController.EyePosition;
			Rotation cameraRotation = mPlayerController.EyeAngles.ToRotation();

			// If the camera hasn't moved, don't bother raycasting again
			if (mCachedEyePosition == source && mCachedCameraRotation == cameraRotation)
				return;

			mCachedEyePosition = source;
			mCachedCameraRotation = cameraRotation;

			Vector3 lookDirection = cameraRotation.Forward * mPickupRange;
			Vector3 destination = source + lookDirection;
			SceneTrace trace = Scene.Trace.Ray(source, destination);
			trace = trace.WithTag(TagDefs.Tag.Grabbable.AsString());

			SpringGizmo.DrawLine(source, destination, dGrabRaycast);
			SceneTraceResult traceResult = trace.IgnoreStatic().Run();

			if (traceResult.Hit)
			{
				GameObject hitObject = traceResult.GameObject;
				Assert.NotNull(hitObject);

				mLookingAtObject = hitObject;
				return;
			}

			mLookingAtObject = null;
		}

		private void ShowGrabbable(GameObject rTarget)
		{
			Assert.NotNull(rTarget);
			Rigidbody rigidbody = rTarget.GetComponent<Rigidbody>();

			SpringGizmo.DrawLineBBox(rigidbody.PhysicsBody.GetBounds(), dGrabbable);
		}

		private void Grab(GameObject rTarget)
		{
			Assert.NotNull(rTarget);
			Rigidbody rigidbody = rTarget.GetComponent<Rigidbody>();

			// TODO ...
		}
	}
}
