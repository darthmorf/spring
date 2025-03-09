using Sandbox.Diagnostics;
using Spring.Input;
using System;

namespace Spring.UI.Screen
{
	public class UIInputPrompt
	{
		public InputAction mInputAction { get; set; }
		public InputDef mInputDef { get; set; }
		public bool mVisible { get; set; } = false;
		public bool mEnabled { get; set; } = true;

		public UIInputPrompt(InputAction mInputAction, InputDef mInputDef, bool mVisible = false, bool mEnabled = true)
		{
			this.mInputAction = mInputAction;
			this.mInputDef = mInputDef;
			this.mVisible = mVisible;
			this.mEnabled = mEnabled;
		}

		// Hash of object is used to determine if we need to redraw UI or not
		public int GetHash()
		{
			return System.HashCode.Combine(mInputAction, mInputDef, mVisible, mEnabled);
		}
	}

	public class UIPromptController
	{
		public List<UIInputPrompt> mInputPrompts;

		public bool mDebugDrawAll = false;

		public void Init()
		{
			mInputPrompts = new List<UIInputPrompt>();
			IEnumerable<InputAction> inputActions = Sandbox.Input.GetActions();

			foreach (InputAction inputAction in inputActions)
			{
				InputDef inputDef;
				Assert.True(Enum.TryParse(inputAction.Name, out inputDef));
				UIController.mUIPromptController.mInputPrompts.Add(new UIInputPrompt(inputAction, inputDef));
			}
		}

		public void SetPromptVisible(InputDef pInputType, bool pVisible)
		{
			foreach (UIInputPrompt uiInputPrompt in mInputPrompts)
			{
				if (uiInputPrompt.mInputDef == pInputType)
				{
					uiInputPrompt.mVisible = pVisible;
					return;
				}
			}
		}

		public void SetPromptEnabled(InputDef pInputType, bool pEnabled)
		{
			foreach (UIInputPrompt uiInputPrompt in mInputPrompts)
			{
				if (uiInputPrompt.mInputDef == pInputType)
				{
					uiInputPrompt.mEnabled = pEnabled;
					return;
				}
			}
		}

		public Texture GetGlyph(InputDef pInputType)
		{
			Texture text = Sandbox.Input.GetGlyph(pInputType.ToString(), InputGlyphSize.Small, false);
			return text;
		}

		// Hash of object is used to determine if we need to redraw UI or not
		public int GetHash()
		{
			int hash = 0;
			foreach (UIInputPrompt uiInputPrompt in mInputPrompts)
			{
				hash = System.HashCode.Combine(hash, uiInputPrompt.GetHash());
			}
			return hash;
		}
	}
}
