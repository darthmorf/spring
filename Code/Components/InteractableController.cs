using Sandbox.Diagnostics;
using Sandbox.Internal;
using Spring.Debug;
using Spring.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.Component;

namespace Spring.Components
{
	public interface IInteractable
	{
		void OnLookStart(ref InteractEvent pEvent) { }
		void OnLookUpdate(ref InteractEvent pEvent) { }
		void OnLookFixedUpdate(ref InteractEvent pEvent) { }
		void OnLookEnd(ref InteractEvent pEvent) { }
	}

	public class InteractEvent
	{
		public GameObject mIteractee;
		public GameObject mIteractor;
		public float mDistance;

		public InteractEvent(GameObject mIteractee, GameObject mIteractor, float mDistance)
		{
			this.mIteractee = mIteractee;
			this.mIteractor = mIteractor;
			this.mDistance = mDistance;
		}
	}


	class InteractableController : Component
	{
		// Config Params
		[Property, Category("Grab"), Title("Range")]
		private float mGrabRange = 100f;

		// Cached Components
		private PlayerController mPlayerController;

		// State
		private Vector3 mLastCameraPosition = Vector3.Zero;
		private Rotation mLastCameraRotation = Rotation.Identity;
		private GameObject mCurrentInteractingObject;
		private GameObject mLastInteractingObject;
		private IEnumerable<IInteractable> mInteractables;
		private InteractEvent mCurrentInteractEvent;
		private InteractEvent mLastInteractEvent;

		protected override void OnAwake()
		{
			mPlayerController = Components.GetInAncestorsOrSelf<PlayerController>();
			Assert.NotNull(mPlayerController);
		}

		protected override void OnFixedUpdate()
		{
			DoRayCheck();
			UpdateInteractEvents();
			SignalInteractions(true);

			mLastInteractEvent = mCurrentInteractEvent;
			mLastInteractingObject = mCurrentInteractingObject;
		}

		protected override void OnUpdate()
		{
			SignalInteractions(false);
		}

		private void DoRayCheck()
		{
			// Calculate vector from camera to desired object centre
			Vector3 source = GetSourcePos();
			Rotation cameraRotation = mPlayerController.EyeAngles.ToRotation();

			// If the camera hasn't moved, don't bother raycasting again
			if (mLastCameraPosition == source && mLastCameraRotation == cameraRotation)
				return;

			mLastCameraPosition = source;
			mLastCameraRotation = cameraRotation;

			// Calculate vector from camera to "look at point" for the raycast
			Vector3 lookDirection = cameraRotation.Forward * mGrabRange;
			Vector3 destination = source + lookDirection;
			SceneTrace trace = Scene.Trace.Ray(source, destination);

			SpringGizmo.DrawLine(source, destination, DebugSettings.InteractableController_GrabRaycast);
			SceneTraceResult traceResult = trace.IgnoreStatic().Run();

			// If we hit something, update object references
			if (traceResult.Hit)
			{
				mCurrentInteractingObject = traceResult.GameObject;
				Assert.NotNull(mCurrentInteractingObject);
				mInteractables = mCurrentInteractingObject.GetComponents<IInteractable>();

				// If the object isn't an interactible, ignore
				if (mInteractables.Count() == 0)
				{
					mCurrentInteractingObject = null;
				}

			}
			// Otherwise clear the references
			else
			{
				mCurrentInteractingObject = null;
			}
		}


		void UpdateInteractEvents()
		{
			if (mCurrentInteractingObject != null)
			{
				float currentDistance = (mCurrentInteractingObject.WorldPosition - this.WorldPosition).Length;
				mCurrentInteractEvent = new InteractEvent(mCurrentInteractingObject, this.GameObject, currentDistance);
			}
			else
			{
				mCurrentInteractEvent = null;
			}

			if (mLastInteractingObject != null)
			{
				float lastDistance = (mLastInteractingObject.WorldPosition - this.WorldPosition).Length;
				mLastInteractEvent = new InteractEvent(mLastInteractingObject, this.GameObject, lastDistance);
			}
			else
			{
				mLastInteractEvent = null;
			}
		}


		private void SignalInteractions(bool pFixedUpdate)
		{
			// Send Look start & look end events
			if (pFixedUpdate && mCurrentInteractingObject != mLastInteractingObject)
			{
				if (mLastInteractingObject != null)
				{
					foreach (IInteractable interactable in mInteractables)
						interactable.OnLookEnd(ref mLastInteractEvent);
				}

				if (mCurrentInteractingObject != null)
				{
					foreach (IInteractable interactable in mInteractables)
						interactable.OnLookStart(ref mCurrentInteractEvent);
				}
			}

			// Send updates
			if (mCurrentInteractingObject != null)
			{
				if (pFixedUpdate)
				{
					foreach (IInteractable interactable in mInteractables)
						interactable.OnLookFixedUpdate(ref mCurrentInteractEvent);
				}
				else
				{
					foreach (IInteractable interactable in mInteractables)
						interactable.OnLookUpdate(ref mCurrentInteractEvent);
				}
			}
		}

		Vector3 GetSourcePos()
		{
			return mPlayerController.EyePosition;
		}
	}
}
