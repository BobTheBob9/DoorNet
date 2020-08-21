using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

using Harmony;
using UnityEngine;

namespace DoorNet.Shared.Helpers
{
	public static class InstantiationHelpers
	{
		private static MethodInfo SetActiveMethod = typeof(InstantiationHelpers).GetMethod("SetObjectActive", BindingFlags.Public | BindingFlags.Static);

		/// <summary>
		/// Patches all calls to Object.Instantiate<GameObject> in a method to immediately set their returned gameobjects active
		/// </summary>
		/// <param name="harmony"></param>
		/// <param name="method"></param>
		public static void PatchInstantiators(HarmonyInstance harmony, MethodInfo method)
		 => harmony.Patch(method, transpiler: new HarmonyMethod(typeof(InstantiationHelpers).GetMethod("TranspileInstantiators", BindingFlags.NonPublic | BindingFlags.Static)));

		public static GameObject SetObjectActive(GameObject gm /*directly after an instantiate call, this will be the first element on the stack*/)
		{
			gm.SetActive(true);
			return gm; //returning the gameobject puts the gm returned from instantiate where it used to be on the stack before our fuckery
		}

		private static IEnumerable<CodeInstruction> TranspileInstantiators(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			List<int> indexes = new List<int>();
			
			for (int i = 0; i < codes.Count; i++)
				if (codes[i].opcode == OpCodes.Call)
				{
					MethodInfo method = (MethodInfo)codes[i].operand;
					System.Type[] genericTypes = method.GetGenericArguments();
					if (genericTypes.Length == 1 && genericTypes[0] == typeof(GameObject) && method.DeclaringType == typeof(Object) && method.Name == "Instantiate")
						indexes.Insert(0, i);
				}

			for (int i = 0; i < indexes.Count; i++)
				codes.Insert(indexes[i] + 1, new CodeInstruction(OpCodes.Call, SetActiveMethod));

			return codes;
		}
	}
}
