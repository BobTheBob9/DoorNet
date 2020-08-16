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
			int maxPrio = 0;
			var methods = new Dictionary<int, List<Tuple<Type, MethodInfo, DoorNetModuleInitialiserAttribute>>>(); //pain
			List<int> lastPriorityIndex = new List<int>();
			foreach (Type type in asm.GetTypes())
			{
				var moduleAttribute = type.GetCustomAttribute<DoorNetModuleAttribute>();
				if (moduleAttribute == null || (!moduleAttribute.IsBoth && moduleAttribute.Side != side))
					continue;

				foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
				{
					//todo: throw error if no default constructor or if target method is not parameterless
					//also like 10-20% of this probably shits itself with multiple initialisers per class but i do not fucking care rn i just want this shit to work
					var methodAttribute = method.GetCustomAttribute<DoorNetModuleInitialiserAttribute>();
					if (methodAttribute == null)
						continue;

					if (!methods.ContainsKey(methodAttribute.Priority))
					{
						methods.Add(methodAttribute.Priority, new List<Tuple<Type, MethodInfo, DoorNetModuleInitialiserAttribute>>());
						if (methodAttribute.Priority > maxPrio)
							maxPrio = methodAttribute.Priority;
					}

					methods[methodAttribute.Priority].Add(new Tuple<Type, MethodInfo, DoorNetModuleInitialiserAttribute>(type, method, methodAttribute));
				}
			}

			for (int i = 0; i < maxPrio + 1; i++)
				if (methods.ContainsKey(i))
					foreach (var tuple in methods[i])
					{
						ParameterInfo[] paramInfo = tuple.Item2.GetParameters();
						if ((paramInfo.Length > 0 && !(paramInfo.Length == 1 && paramInfo[0].ParameterType == typeof(Side))) || (tuple.Item3.IsSided && tuple.Item3.Side != side)) //pain
							continue;

						object[] methodArgs = paramInfo.Length == 0 ? new object[0] : new object[] { side };

						if (tuple.Item2.IsConstructor)
							Activator.CreateInstance(tuple.Item1, methodArgs);
						else
						{
							object instance = null;
							if (!tuple.Item2.IsStatic)
								instance = Activator.CreateInstance(tuple.Item1);

							tuple.Item2.Invoke(instance, methodArgs);
						}
					}
		}
	}
}