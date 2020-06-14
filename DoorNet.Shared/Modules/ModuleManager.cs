using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using DoorNet.Shared.Networking;

namespace DoorNet.Shared.Modules
{
	/// <summary>
	/// Manages DoorNet modules
	/// </summary>
	public static class ModuleManager
	{
		/// <summary>
		/// Initialises all modules that match the side given in all currently loaded assemblies 
		/// The caller's assembly's modules are initialised first
		/// </summary>
		/// <param name="side"></param>
		public static void LoadModules(Side side)
		{
			Assembly caller = Assembly.GetCallingAssembly();
			//init types of caller first
			InstantiateTypes(side, caller);

			//then other assemblies
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
				if (asm != caller)
					InstantiateTypes(side, asm);
		}

		private static void InstantiateTypes(Side side, Assembly asm)
		{
			foreach (Type type in asm.GetTypes())
				foreach (Attribute attribute in type.GetCustomAttributes(typeof(DoorNetModuleAttribute), false))
				{
					var moduleAttribute = (DoorNetModuleAttribute)attribute;
					if (moduleAttribute.Side != side)
						continue;

					MethodInfo method = type.GetMethod("Initialise");

					if (method.IsStatic)
						method.Invoke(null, new object[0]);
					else
					{
						object instance = Activator.CreateInstance(type);
						method.Invoke(instance, new object[0]);
					}
				}
		}
	}
}
