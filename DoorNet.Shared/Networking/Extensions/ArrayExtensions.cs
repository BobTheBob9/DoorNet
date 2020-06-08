using System;

namespace DoorNet.Shared.Networking.Extensions
{
	public static class ArrayExtensions
	{
		public static T[] Prepend<T>(this T[] array, T prependedValue)
		{
			T[] newValues = new T[array.Length + 1];
			newValues[0] = prependedValue;
			Array.Copy(array, 0, newValues, 1, array.Length);

			return newValues;
		}

		public static T[] Prepend<T>(this T[] array, T[] prependedValues)
		{
			T[] newValues = new T[array.Length + prependedValues.Length];
			Array.Copy(prependedValues, newValues, prependedValues.Length);
			Array.Copy(array, 0, newValues, prependedValues.Length, array.Length);

			return newValues;
		}

		public static T[] Append<T>(this T[] array, T appendedValue)
		{
			T[] newValues = new T[array.Length + 1];
			Array.Copy(array, newValues, array.Length);
			newValues[newValues.Length - 1] = appendedValue;

			return newValues;
		}

		public static T[] Append<T>(this T[] array, T[] appendedValues)
		{
			T[] newValues = new T[array.Length + appendedValues.Length];
			Array.Copy(array, newValues, array.Length);
			Array.Copy(appendedValues, 0, newValues, array.Length, appendedValues.Length);

			return newValues;
		}
	}
}
