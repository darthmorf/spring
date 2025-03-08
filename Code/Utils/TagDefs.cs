using Sandbox.Diagnostics;

namespace Spring.Utils
{
	// TODO - make this more generic so we can use it for other enum<->string mappings e.g. input
	public static class TagDefs
    {
		public enum Tag
		{
			Grabbable
		}

		private static readonly Dictionary<Tag, string> TagMap = new()
		{
			{ Tag.Grabbable, "grabbable" }
		};

		public static string AsString(this Tag pTag)
		{
			if (TagMap.TryGetValue(pTag, out var tagString))
			{
				return tagString;
			}

			Assert.True(false, $"Unknown tag '{pTag}'!");
			return "";
		}
	}


	public class TagApplier : Component
	{
		[Property, Title("Tags")]
		TagDefs.Tag[] tags = { };

		protected override void OnAwake()
		{
			foreach (TagDefs.Tag tag in tags)
			{
				GameObject.Tags.Add(tag.AsString());
			}
		}
	}
}
