using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spring.Utils
{
    public static class Localiser
    {
		// TODO - Support some kind of loc/ui table??
		static Dictionary<string, string> mStrings = new Dictionary<string, string>
		{
			{ "ACTION_GRAB",			"Grab" },
			{ "ACTION_DROP",			"Drop" },
			{ "ACTION_RESET_ROTATION",	"Reset Rotation" },
			{ "ACTION_OPEN",			"Open" },
			{ "ACTION_CLOSE",			"Close" },
		};

		public static string GetString(string pKey)
		{
			return mStrings[pKey] ?? pKey;
		}
    }
}
