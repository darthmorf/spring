using Sandbox;
using Sandbox.Diagnostics;
using Spring.Utils;

namespace Spring.Components
{
	public class SecurityCameraComponent : Component
	{
		// Config Params
		[Property, Category("Animation Settings"), Title("Animate?")]
		bool mAnimate = false;

		[Property, Category("Render Settings"), Title("Output Decals")]
		List<Decal> mDecals = new List<Decal>();
		[Property, Category("Render Settings"), Title("Output Texture Size")]
		Vector2 mTextureSize = new Vector2(1280, 720);

		// Cached Components
		CameraComponent mCamera;
		HingeJoint mHinge;

		// State
		Texture mRenderTexture;

		protected override void OnAwake()
		{
			InitRender();
			InitAnimation();
		}

		protected override void OnUpdate()
		{
			UpdateRender();
		}

		protected override void OnFixedUpdate()
		{
			UpdateAnimation();
		}

		private void InitRender()
		{
			mCamera = this.MustGetComponentInChildren<CameraComponent>();
			mRenderTexture = Texture.CreateRenderTarget($"SECURITY_CAM_{GameObject.Id}_TEXTURE", ImageFormat.RGBA8888, mTextureSize);

			foreach (Decal decal in mDecals)
			{
				DecalDefinition def = new DecalDefinition();
				def.ColorTexture = mRenderTexture;
				decal.Decals.Add(def);
			}
		}

		private void InitAnimation()
		{
			mHinge = this.MustGetComponentInChildren<HingeJoint>();
			mHinge.TargetAngle = mHinge.MaxAngle;

			if (mAnimate)
				mHinge.Motor = HingeJoint.MotorMode.TargetAngle;
		}

		private void UpdateRender()
		{
			mCamera.RenderToTexture(mRenderTexture);
		}

		private void UpdateAnimation()
		{
			if (mHinge.Angle.AlmostEqual(mHinge.MaxAngle, 2.0f))
				mHinge.TargetAngle = mHinge.MinAngle;
			else if (mHinge.Angle.AlmostEqual(mHinge.MinAngle, 2.0f))
				mHinge.TargetAngle = mHinge.MaxAngle;
		}
	}
}
