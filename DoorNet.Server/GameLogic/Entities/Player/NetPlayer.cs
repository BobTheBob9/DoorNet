using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using BobNet;
using UnityEngine;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

namespace DoorNet.Server.GameLogic
{
	using static GameServer;

	/// <summary>
	/// Represents a serverside player
	/// </summary>
	[DoorNetModule(Side.Server)]
	public class NetPlayer : MonoBehaviour
	{
		public static ReadOnlyCollection<NetPlayer> Players 
		{
			get => _Players.AsReadOnly();
		}
		public static NetChannel NameChannel { get; private set; }
		public static NetChannel ActionChannel { get; private set; }
		public static NetChannel PositionChannel { get; private set; }
		public static NetChannel RotationChannel { get; private set; }
		public static NetChannel ShootingChannel { get; private set; }
		public static NetChannel ClientPlayerCreationChannel { get; private set; }

		private static List<NetPlayer> _Players = new List<NetPlayer>();

		public string Name { get; private set; }
		public NetClient Client { get; private set; }
		public NetEntity Entity { get; private set; }

		public NetPlayerInventory Inventory { get; private set; }
		public int Health { get; private set; }

		[DoorNetModuleInitialiser]
		private static void Initialise() 
		{
			GameObject fakePlayerPrefab = new GameObject("DoorNet::Player");
			fakePlayerPrefab.SetActive(false);
			fakePlayerPrefab.AddComponent<NetPlayer>();

			NetEntityRegistry.Instance.CreateEntry("DoorNet::Player", fakePlayerPrefab);

			NameChannel = NetworkManager.CreateChannel("DoorNet::Player::Name", new StringChannel(Encoding.Unicode));
			ActionChannel = NetworkManager.CreateChannel("DoorNet::Player::Action");
			PositionChannel = NetworkManager.CreateChannel("DoorNet::Player::Position", new Vector3Channel());
			RotationChannel = NetworkManager.CreateChannel("DoorNet::Player::Rotation", new QuaternionChannel());
			ShootingChannel = NetworkManager.CreateChannel("DoorNet::Player::Shooting", new QuaternionChannel());
			ClientPlayerCreationChannel = NetworkManager.CreateChannel("DoorNet::CreateClientPlayer", new UShortChannel());

			//PositionChannel.CreatePreprocessor(new RatelimitPreprocessor(TimeSpan.FromSeconds(5))); //temp
			//RotationChannel.CreatePreprocessor(new RatelimitPreprocessor(TimeSpan.FromSeconds(5))); //also temp

			OnClientJoin += CreatePlayer;
			OnClientLeave += (NetClient client, string reason) =>
			{
				NetPlayer player = client.GetPlayer();
				Chat.Send($"{player.Name} left the game");
				Destroy(player);
			};

			NameChannel.OnRecieveSerialized += (object data, NetClient sender) =>
			{
				NetPlayer player = sender.GetPlayer();
				if (player != null && player.Name != string.Empty)
				{
					player.Name = (string)data;
					Chat.Send($"{player.Name} joined the game");
					Chat.SendTo(sender, "Welcome to bob's cool server lol (this text should be changed to a configurable motd later!!!", string.Empty);
				}
			};

			ActionChannel.OnRecieveRaw += (byte[] data, NetClient sender) =>
			{
				if (data.Length != 1)
					return;

				sender.GetPlayer()?.DoAction((NetPlayerAction)data[0]);
			};

			//todo: possibly ratelimit these
			PositionChannel.OnRecieveSerialized += (object data, NetClient sender) =>
			{
				Vector3 pos = (Vector3)data;
				NetPlayer player = sender.GetPlayer();

				if (player != null)
				{
					player.transform.position = pos;
					player.Entity.UpdatePosition();
				}
			};

			RotationChannel.OnRecieveSerialized += (object data, NetClient sender) =>
			{
				Quaternion rot = (Quaternion)data;
				NetPlayer player = sender.GetPlayer();
				if (player != null)
				{
					player.transform.rotation = rot;
					player.Entity.UpdateRotation();
				}
			};

			ShootingChannel.OnRecieveSerialized += (object data, NetClient sender) =>
			{
				Quaternion rot = (Quaternion)data;
				NetPlayer player = sender.GetPlayer();
				if (player != null)
				{
					player.transform.rotation = rot;
					player.Entity.UpdateRotation();

					player.Inventory.Weapons[player.Inventory.SelectedIndex].ShootPrimary();
				}
			};
		}

		internal static void CreatePlayer(NetClient client)
		{
			GameObject obj = Instantiate(NetEntityRegistry.Instance.GetItem("DoorNet::Player"));
			NetPlayer player = obj.GetComponent<NetPlayer>();
			player.Client = client;

			NetClient[] clientsToSendTo = new NetClient[NetworkManager.Clients.Count - 1];
			int i = 0;
			foreach (NetClient currentClient in NetworkManager.Clients)
				if (currentClient != client)
					clientsToSendTo[i++] = currentClient;

			player.Entity = NetEntity.CreateEntity(obj, clientsToSendTo);
			ClientPlayerCreationChannel.SendSerialized(SendMode.Tcp, player.Entity.ID, client);

			player.Inventory = new NetPlayerInventory(player);

			_Players.Add(player);
		}

		public void DoAction(NetPlayerAction action)
		{
			switch (action)
			{
				case NetPlayerAction.StartReload:
					{
						Inventory.Weapons[Inventory.SelectedIndex].Reload();
						break;
					}

				case NetPlayerAction.GenericInteract:
					{

						break;
					}
			}
		}

		public void Damage(int amount)
		{

		}

		private void OnDestroy()
		{
			Chat.Send($"{Name} left the game");
		}
	}
}
