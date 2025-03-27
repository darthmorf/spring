using Sandbox;
using Sandbox.Diagnostics;
using Spring.Input;

namespace Spring.Utils
{
	public class HotkeyToggler : Component
	{
		[Property, Title("Enabled State")]
		public bool mEnabled = false;

		[Property, Title("Hotkey")]
		public InputDef mHotkey;

		[Property, Title("Components")]
		public List<Component> mComponents;

		protected override void OnAwake()
		{
			ApplyState();
		}

		protected override void OnUpdate()
		{
			if (Sandbox.Input.Pressed(mHotkey.ToString()))
				Toggle();
		}

		public void Toggle()
		{
			mEnabled = !mEnabled;
			ApplyState();
		}

		private void ApplyState()
		{
			Assert.True(mComponents != null);

			foreach (Component component in mComponents)
			{
				component.Enabled = mEnabled;
			}
		}
	}
}
