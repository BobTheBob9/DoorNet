using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;
using DoorNet.Shared.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DoorNet.Shared.Prefabs
{
	//centrifuge's gttod GSL already has support for this, but we've gotta reimplement it because the gsl is broken on recent centrifuge versions
	//big rip
	public static class GTTODPrefabs
	{
		private static bool Initialised = false;
		private static List<Scene> LoadedScenes = new List<Scene>();
		private static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

		public static void Initialise() 
		{
			if (Initialised)
				return;

			Initialised = true;

			SceneManager.sceneLoaded += AddAssetsFromScene;
		}

		public static GameObject GetPrefab(string name)
		 => Prefabs[name];

		public static string[] GetNames()
		{
			string[] names = new string[Prefabs.Count];
			Prefabs.Keys.CopyTo(names, 0);

			return names;
		}

		private static void AddAssetsFromScene(Scene scene, LoadSceneMode loadMode)
		{
			if (LoadedScenes.Contains(scene))
				return;

			LoadedScenes.Add(scene);

			GameObject[] prefabs = Resources.FindObjectsOfTypeAll<GameObject>();
			foreach (GameObject prefab in prefabs)
				if (!Prefabs.ContainsKey(prefab.name) && prefab.scene.name == null)
					Prefabs.Add(prefab.name, prefab);
		}
	}
}
