using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DoorNet.Shared.Networking;

namespace DoorNet.Shared.Modules
{
	/// <summary>
	/// Defines a module, modules have their Initialise method called when their respective side is initialised
	/// </summary>
	public class DoorNetModuleAttribute : Attribute
	{
		public Side Side;

		public DoorNetModuleAttribute(Side side)
		{
			Side = side;
		}
	}
}
