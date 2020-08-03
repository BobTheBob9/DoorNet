using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Harmony;

namespace DoorNet.Client.Menus
{
	using static GameClient;

	public static partial class CustomConsole
	{
		public delegate void Command(string[] args);

		internal static Dictionary<string, Command> Commands;

		public static void RegisterCommand(string name, Command command)
		 => Commands.Add(name.ToLower(), command);

		internal static void PatchConsole()
		 => Harmony.Patch(typeof(ac_Console).GetMethod("Submit"), postfix: new HarmonyMethod(typeof(CustomConsole).GetMethod("Patch")));

		private static void Patch(string ConsoleSubmission)
		{
			string[] fullCommand = ConsoleSubmission.Split(' ');
			if (fullCommand.Length == 0)
				return;

			string commandName = fullCommand[0].ToLower();
			if (!Commands.ContainsKey(commandName))
				return;

			string[] args = new string[fullCommand.Length - 1];
			Array.Copy(fullCommand, 1, args, 0, args.Length);

			Commands[commandName](args);
		}
	}
}
