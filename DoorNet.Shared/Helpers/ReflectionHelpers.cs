using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace DoorNet.Shared.Helpers
{
	public static class ReflectionHelpers
	{
		private static Regex EnumeratorClassNameRegex = new Regex("<([A-Za-z_][A-Za-z0-9_]*)>d__\\d+");

		public static Type GetIEnumeratorClass(MethodInfo method)
		 => GetIEnumeratorClass(method.DeclaringType, method.Name);

		public static Type GetIEnumeratorClass(Type classType, string methodName)
		{
			foreach (Type nestedType in classType.GetNestedTypes(BindingFlags.NonPublic))
			{
				Match match = EnumeratorClassNameRegex.Match(nestedType.Name);
				if (match.Success && match.Groups[1].Value == methodName)
					return nestedType;
			}

			return null;
		}
	}
}
