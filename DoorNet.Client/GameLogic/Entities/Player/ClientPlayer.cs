using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;
using UnityEngine;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

namespace DoorNet.Client.GameLogic
{
	using static GameClient;

	[DoorNetModule(Side.Client)]
	public class ClientPlayer : MonoBehaviour
	{
		public static ushort EntityID { get; private set; }
		public static ClientPlayer Instance { get; private set; }
		public static GameObject PlayerObject { get; private set; }

		public static NetChannel NameChannel { get; private set; }
		public static NetChannel ActionChannel { get; private set; }
		public static NetChannel PositionChannel { get; private set; }
		public static NetChannel RotationChannel { get; private set; }
		public static NetChannel ShootingChannel { get; private set; }
		public static NetChannel ClientPlayerCreationChannel { get; private set; }

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			NameChannel = NetworkManager.CreateChannel("DoorNet::Player::Name", new StringChannel(Encoding.Unicode));
			ActionChannel = NetworkManager.CreateChannel("DoorNet::Player::Action");
			PositionChannel = NetworkManager.CreateChannel("DoorNet::Player::Position", new Vector3Channel());
			RotationChannel = NetworkManager.CreateChannel("DoorNet::Player::Rotation", new QuaternionChannel());
			ShootingChannel = NetworkManager.CreateChannel("DoorNet::Player::Shooting", new QuaternionChannel());
			ClientPlayerCreationChannel = NetworkManager.CreateChannel("DoorNet::CreateClientPlayer", new UShortChannel());

			ClientPlayerCreationChannel.OnRecieveSerialized += (object data, NetClient sender) =>
			{
				EntityID = (ushort)data;
				PlayerObject = GameManager.GM.Player;
				Instance = PlayerObject.AddComponent<ClientPlayer>();

				NameChannel.SendSerialized(SendMode.Tcp, "fuck idk figure out names later", NetworkManager.Server);
			};

			PositionChannel.OnRecieveSerialized += (object data, NetClient sender) =>
			{

			};

			RotationChannel.OnRecieveSerialized += (object data, NetClient sender) =>
			{

			};
		}

		private void FixedUpdate()
		{
			//temp code redo later
			PositionChannel.SendSerialized(SendMode.Udp, transform.position, NetworkManager.Server);
			RotationChannel.SendSerialized(SendMode.Udp, transform.rotation, NetworkManager.Server);
		}
	}
}
