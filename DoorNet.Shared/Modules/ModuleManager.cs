using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using BobNet;

namespace DoorNet.Shared.Modules
{
	/// <summary>
	/// Static class for loading DoorNet modules
	/// </summary>
	public static class ModuleManager
	{
		/// <summary>
		/// Initialises all modules that match the side given in all currently loaded assemblies.
		/// The caller's assembly's modules are initialised first.
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
					if (!moduleAttribute.IsBoth && moduleAttribute.Side != side)
						continue; //wrong side

					foreach (MethodInfo method in type.GetMethods())
						foreach (Attribute methodAttribute in method.GetCustomAttributes(typeof(DoorNetModuleInitialiserAttribute), false))
						{
							//todo: throw error if no default constructor or if target method is not parameterless
							ParameterInfo[] paramInfo = method.GetParameters();
							if (paramInfo.Length > 0 && !(paramInfo.Length == 1 && paramInfo[0].ParameterType == typeof(Side))) //pain
								continue;

							var initAttribute = (DoorNetModuleInitialiserAttribute)methodAttribute;
							if (initAttribute.IsSided && initAttribute.Side != side) //wrong side
								continue;

							if (method.IsStatic) //invoke statically if static
							{
								if (paramInfo.Length == 0)
									method.Invoke(null, new object[0]); //call marked method
								else //paramInfo is (Side callingSide)
									method.Invoke(null, new object[] { side });
							}
							else
							{
								if (method.IsConstructor)
								{
									if (paramInfo.Length == 0)
										Activator.CreateInstance(type); //call marked method
									else //paramInfo is (Side callingSide)
										Activator.CreateInstance(type, new object[] { side });
								}
								else //if constructor is marked we would've already called it on creation
								{
									object instance = Activator.CreateInstance(type);
									if (paramInfo.Length == 0)
										method.Invoke(instance, new object[0]); //call marked method
									else //paramInfo is (Side callingSide)
										method.Invoke(instance, new object[] { side });
								}
							}
						}
				}
		}
	}
}