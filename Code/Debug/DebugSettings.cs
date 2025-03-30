using Spring.Components;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace Spring.Debug
{
	public static class DebugSettings
	{
		public static DebugProperty InteractableController_GrabRaycast = new DebugProperty(Color.Red, true, 2.0f);
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
		[Property, Group("Interactable Check Raycast"), InlineEditor]
		public DebugProperty mInteractRaycast { get => DebugSettings.InteractableController_GrabRaycast; set => DebugSettings.InteractableController_GrabRaycast = value; }
		
		[Property, Group("Grabbable Indicator"), InlineEditor]
		public DebugProperty mGrababbleObject { get => DebugSettings.GrabController_Grabbable; set => DebugSettings.GrabController_Grabbable = value; }
		
		[Property, Group("Grabbed Indicator"), InlineEditor]
		public DebugProperty mGrabbedObject { get => DebugSettings.GrabController_Grabbed; set => DebugSettings.GrabController_Grabbed = value; }

		[Property, Group("Grabbed Rotate Indicator"), InlineEditor]
		public DebugProperty mGrabbedRotateObject { get => DebugSettings.GrabController_GrabbedRotate; set => DebugSettings.GrabController_GrabbedRotate = value; }

		[Property, Group("Desired Grabbed Force"), InlineEditor]
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
