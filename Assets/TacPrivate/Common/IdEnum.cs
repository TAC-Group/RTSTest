// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2024 Sergej Jakovlev
// You can use this code for educational purposes only;
// this code or its modifications cannot be used for commercial purposes
// or in proprietary libraries without permission from the author

using System;

namespace Tac
{
	public static class Extensions
	{
		public static Id Id(this Enum en)
		{
			Type type = en.GetType();
			var memInfo = type.GetMember(en.ToString());
			var attributes = memInfo[0].GetCustomAttributes(typeof(IdAttribute), false);
			Id id = ((IdAttribute)attributes[0]).Value;
			return id;
		}
	}

	public class IdAttribute : Attribute
	{
		public Id Value;
		public IdAttribute(string argValue)
		{
			Value = new Id(argValue);
		}
	}

	public class Id
	{
		private int id1;
		private int id2;

		public int Id1 { get { return id1; } set { id1 = value; Value = Value; } }
		public int Id2 { get { return id2; } set { id2 = value; Value = Value; } }

		public string Value
		{
			get { return id1.ToString() + "." + id2.ToString(); }
			set
			{
				int index = value.IndexOf('.');
				id1 = int.Parse(value.Substring(0, index));
				id2 = int.Parse(value.Substring(index + 1, value.Length - index - 1));
			}
		}

		public Id(string argValue)
		{ 
			Value = argValue;
		}

		public Id(int argId1, int argId2)
		{
			Id1 = argId1;
			Id2 = argId2;
		}
	}

}
