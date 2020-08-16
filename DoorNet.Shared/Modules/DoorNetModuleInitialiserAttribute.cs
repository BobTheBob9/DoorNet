using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;

namespace DoorNet.Shared.Modules
{
	/// <summary>
	/// Defines a module initialisation method, called on module classes.
	/// Initialisation methods must be parameterless.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
	public class DoorNetModuleInitialiserAttribute : Attribute
	{
		public Side Side;
		public bool IsSided = false;
		public int Priority = 0;

		public DoorNetModuleInitialiserAttribute(int priority = 0) 
		{
			Priority = priority;
		}

		public DoorNetModuleInitialiserAttribute(Side side, int priority = 0)
		{
			Side = side;
			IsSided = true;
			Priority = priority;
		}
	}
}
