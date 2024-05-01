// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2022 Sergej Jakovlev
// You can use this code for educational purposes only;
// this code or its modifications cannot be used for commercial purposes
// or in proprietary libraries without permission from the author

namespace Tac
{
	public static class XYZ_
	{
		public static bool IsX(XYZ argXYZ)
		{
			bool ret = false;
			switch (argXYZ)
			{
				case XYZ.X:
				case XYZ.XY:
				case XYZ.XZ:
				case XYZ.XYZ:
					ret = true;
					break;
			}
			return ret;
		}
		public static bool IsY(XYZ argXYZ)
		{
			bool ret = false;
			switch (argXYZ)
			{
				case XYZ.Y:
				case XYZ.XY:
				case XYZ.YZ:
				case XYZ.XYZ:
					ret = true;
					break;
			}
			return ret;
		}
		public static bool IsZ(XYZ argXYZ)
		{
			bool ret = false;
			switch (argXYZ)
			{
				case XYZ.Z:
				case XYZ.YZ:
				case XYZ.XZ:
				case XYZ.XYZ:
					ret = true;
					break;
			}
			return ret;
		}

	}
	public enum XYZ
	{
		None,
		X,
		Y,
		Z,
		XZ,
		YZ,
		XY,
		XYZ
	}
}
