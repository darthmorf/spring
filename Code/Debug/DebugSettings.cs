﻿using Spring.Components;
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
		public static DebugProperty GrabController_GrabbedForce = new DebugProperty(Color.Cyan);
	}

	public struct DebugProperty
	{
		[Property, Title("Color")]
		public Color mColour = Color.Red;

		[Property, Title("Show?")]
		public bool mShow = true;

		[Property, Title("Line Thickness")]
		public float mLineThickness = 1.0f;

		[JsonConstructor]
		public DebugProperty(Color mColor, bool mShow = true, float mLineThickness = 1.0f)
		{
			this.mColour = mColor;
			this.mShow = mShow;
			this.mLineThickness = mLineThickness;
		}
	}

	public class DebugSettingsEditor : Component
	{
		[Property, Title("Grabbable Raycast")]
		public DebugProperty mGrabRaycast { get => DebugSettings.GrabController_GrabRaycast; set => DebugSettings.GrabController_GrabRaycast = value; }
		
		[Property, Title("Grabbable Indicator")]
		public DebugProperty mGrababbleObject { get => DebugSettings.GrabController_Grabbable; set => DebugSettings.GrabController_Grabbable = value; }
		
		[Property, Title("Grabbable Indicator")]
		public DebugProperty mGrabbedObject { get => DebugSettings.GrabController_Grabbed; set => DebugSettings.GrabController_Grabbed = value; }
		
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
