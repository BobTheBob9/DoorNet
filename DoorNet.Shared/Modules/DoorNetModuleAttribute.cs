using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DoorNet.Shared.Networking;

namespace DoorNet.Shared.Modules
{
	/// <summary>
	/// Marks a class or struct as a module, modules have their initialisation method called when their respective side is initialised.
	/// Mark a method or constructor with a DoorNetModuleInitialiserAttribute to set it as their initialisation method.
	/// Modules must have a parameterless constructor.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class DoorNetModuleAttribute : Attribute
	{
		public Side Side;

		public DoorNetModuleAttribute(Side side)
		{
			Side = side;
		}
	}
}
