using Sandbox.Rendering;

namespace Spring.UI.HUD
{
    public class HUD : Component
	{
		// Config Properties
		[Property, Category("Crosshair"), Title("Draw?")] 
		bool mDrawCrosshair = true;
		[Property, Category("Crosshair"), Title("Size")]
		int mCrosshairSize = 4;


		// Methods
		protected override void OnUpdate()
		{
			if (Scene.Camera == null)
				return;

			HudPainter hud = Scene.Camera.Hud;

			if (mDrawCrosshair)
				hud.DrawCircle( new Vector2( Screen.Width * 0.5f, Screen.Height * 0.5f ), mCrosshairSize, Color.White );
		}
    }
}
