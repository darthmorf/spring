using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spring.Utils
{
    public static class MathsExtensions
    {
		public static void SetPitch(this ref Rotation pRotation, float pPitch)
		{
			Angles eulerAngles = pRotation.Angles();
			eulerAngles.pitch = pPitch;
			pRotation = Rotation.From(eulerAngles);
		}

		public static void SetYaw(this ref Rotation pRotation, float pYaw)
		{
			Angles eulerAngles = pRotation.Angles();
			eulerAngles.yaw = pYaw;
			pRotation = Rotation.From(eulerAngles);
		}

		public static void SetRoll(this ref Rotation pRotation, float pRoll)
		{
			Angles eulerAngles = pRotation.Angles();
			eulerAngles.roll = pRoll;
			pRotation = Rotation.From(eulerAngles);
		}
	}
}
