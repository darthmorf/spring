using Spring.Components;

namespace Spring.Debug
{
	public class DebugSettingsEditor : Component
	{
		[Property, Title("Grabbable Raycast")]
		public DebugProperty mGrabRaycast { get => GrabController.dGrabRaycast; set => GrabController.dGrabRaycast = value; }
		
		[Property, Title("Grabbable Indicator")]
		public DebugProperty mGrababbleObject { get => GrabController.dGrabbable; set => GrabController.dGrabbable = value; }
	}

	public class DebugProperty
	{
		[Property, Title("Show?")]
		public bool mShow = true;

		[Property, Title("Color")]
		public Color mColor = Color.Red;

		[Property, Title("Line Thickness")]
		public float mLineThickness = 1.0f;


		public DebugProperty(Color mColor, bool mShow = true, float mLineThickness = 1.0f)
		{
			this.mShow = mShow;
			this.mColor = mColor;
			this.mLineThickness = mLineThickness;
		}
	}

	public static class SpringGizmo
	{
		public static void DrawLineBBox(in BBox pBox, DebugProperty pDebugProperty)
		{
			if (!pDebugProperty.mShow)
				return;

			Gizmo.Draw.Color = pDebugProperty.mColor;
			Gizmo.Draw.LineThickness = pDebugProperty.mLineThickness;
			Gizmo.Draw.LineBBox(pBox);
		}

		public static void DrawLine(in Vector3 pStart, in Vector3 pEnd, DebugProperty pDebugProperty)
		{
			if (!pDebugProperty.mShow)
				return;

			Gizmo.Draw.Color = pDebugProperty.mColor;
			Gizmo.Draw.LineThickness = pDebugProperty.mLineThickness;
			Gizmo.Draw.Line(pStart, pEnd);
		}
	}
}
