using Spring.Components;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace Spring.Debug
{
	public static class DebugSettings
	{
		public static DebugProperty GrabController_GrabRaycast = new DebugProperty(Color.Red, true, 2.0f);
		public static DebugProperty GrabController_Grabbable = new DebugProperty(Color.Green);
		public static DebugProperty GrabController_Grabbed = new DebugProperty(Color.Cyan);
		public static DebugProperty GrabController_GrabbedRotate = new DebugProperty(Color.Yellow);
		public static DebugProperty GrabController_GrabbedForce = new DebugProperty(Color.Cyan);
	}

	public struct DebugProperty
	{
		[Title("Colour")]
		public Color mColour { get; set; } = Color.Red;

		[Title("Show?")]
		public bool mShow { get; set; } = true;

		[Title("Line Thickness")]
		public float mLineThickness { get; set; } = 1.0f;

		[JsonConstructor]
		public DebugProperty(Color mColour, bool mShow = true, float mLineThickness = 1.0f)
		{
			this.mColour = mColour;
			this.mShow = mShow;
			this.mLineThickness = mLineThickness;
		}
	}

	public class DebugSettingsEditor : Component
	{
		[Property, Title("Grabbable Check Raycast")]
		public DebugProperty mGrabRaycast { get => DebugSettings.GrabController_GrabRaycast; set => DebugSettings.GrabController_GrabRaycast = value; }
		
		[Property, Title("Grabbable Indicator")]
		public DebugProperty mGrababbleObject { get => DebugSettings.GrabController_Grabbable; set => DebugSettings.GrabController_Grabbable = value; }
		
		[Property, Title("Grabbed Indicator")]
		public DebugProperty mGrabbedObject { get => DebugSettings.GrabController_Grabbed; set => DebugSettings.GrabController_Grabbed = value; }

		[Property, Title("Grabbed Rotate Indicator")]
		public DebugProperty mGrabbedRotateObject { get => DebugSettings.GrabController_GrabbedRotate; set => DebugSettings.GrabController_GrabbedRotate = value; }

		[Property, Title("Desired Grabbed Force")]
		public DebugProperty mGrababbedForce { get => DebugSettings.GrabController_GrabbedForce; set => DebugSettings.GrabController_GrabbedForce = value; }

	}

	

	public static class SpringGizmo
	{
		public static void DrawLineBBox(in BBox pBox, DebugProperty pDebugProperty)
		{
			if (!pDebugProperty.mShow)
				return;

			Gizmo.Draw.Color = pDebugProperty.mColour;
			Gizmo.Draw.LineThickness = pDebugProperty.mLineThickness;
			Gizmo.Draw.LineBBox(pBox);
		}

		public static void DrawLine(in Vector3 pStart, in Vector3 pEnd, DebugProperty pDebugProperty)
		{
			if (!pDebugProperty.mShow)
				return;

			Gizmo.Draw.Color = pDebugProperty.mColour;
			Gizmo.Draw.LineThickness = pDebugProperty.mLineThickness;
			Gizmo.Draw.Line(pStart, pEnd);
		}
	}
}
