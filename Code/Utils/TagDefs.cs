using Sandbox.Diagnostics;

namespace Spring.Utils
{
	public enum Tag
	{
		Door
	}

	// Applies a list of tags to a component OnAwake - this avoids typing arbitrary strings for the tags
	public class TagApplier : Component
	{
		[Property, Title("Tags")]
		Tag[] tags = { };

		protected override void OnAwake()
		{
			foreach (Tag tag in tags)
			{
				GameObject.Tags.Add(tag.ToString());
			}
		}
	}
}
