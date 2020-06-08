using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

using DoorNet.Shared.Networking.Extensions;
using DoorNet.Shared.Networking.Exceptions;

//todo: handle socket errors properly

namespace DoorNet.Shared.Networking
{
	public partial class NetManager
	{
		public const int TCP_PACKET_HEADER_SIZE = sizeof(ushort) * 2; //ushort for handler id + ushort for len
		public const int UDP_PACKET_HEADER_SIZE = sizeof(ushort); //ushort for handler id

		public ClientEventContainer ClientEvents;
		public ServerEventContainer ServerEvents;

		public NetHandle Server {
			get
			{
				if (Side != ManagerType.Client)
					throw new IncorrectManagerTypeException("NetManager.Server can only be accessed from client NetManagers");

				return Clients[0];
			}
		}

		public List<NetHandle> Clients = new List<NetHandle>();
		public bool InUse = false;
		public int Port = 27272;
		public ManagerType Side;

		public NetHandler SyncronisePacketListHandler;
		public NetHandler ClientDroppedHandler;
		public NetHandler UdpHandshakeHandler;

		private List<NetHandler> Handlers = new List<NetHandler>();
		private UdpClient Udp = new UdpClient();
		private TcpClient TcpClient = new TcpClient();
		private TcpListener TcpServer;
		private Thread UdpThread;
		private Thread TcpThread;
		private bool UdpReady = false;

		private List<UdpThreadPacket> UdpThreadPackets = new List<UdpThreadPacket>();
		private List<TcpThreadPacket> TcpThreadPackets = new List<TcpThreadPacket>();

		private BinaryFormatter InternalFormatter = new BinaryFormatter();
		private byte[][] SyncronisePackets;
		private int SyncPacketsRecieved = 0;
		private Guid? UdpHandshakeGuid = null;

		/// <summary>
		/// Constructs a NetManager as either a client or server, port should only be specified for servers
		/// </summary>
		/// <param name="type">Whether the NetManager is a client or server</param>
		/// <param name="port">If the NetManager is a server, the port it should listen on</param>
		public NetManager(ManagerType type, int port = -1)
		{
			Side = type;

			if (Side == ManagerType.Server && port == -1)
				throw new IncorrectManagerTypeException("You must provide a port for server NetManagers");
			else if (Side == ManagerType.Server)
				Port = port;

			int maxChunkSize = 4096 - TCP_PACKET_HEADER_SIZE;
			if (type == ManagerType.Client)
			{
				//handle for clients
				SyncronisePacketListHandler = Handle("DoorNet::Shared::Networking::SyncronisePacketListWithHost");
				SyncronisePacketListHandler.Listen((byte[] data, NetHandle sender) => 
				{
					//assemble complete list of sync packets, deserialize, use to order preexisting list of handlers, dc if unknown packets are found
					if (SyncronisePackets == null)
					{
						//recieving first sync packet
						ushort numPackets = (ushort)BitConverter.ToInt16(data, 0);

						SyncronisePackets = new byte[numPackets][];
					}

					SyncronisePackets[SyncPacketsRecieved] = data;

					if (SyncPacketsRecieved == SyncronisePackets.Length - 1)
					{
						//actually do stuff with the data now, since we've got all of it
						//we need to flatten our array of array of packets to get a single complete stream of data to deserialize
						//calculate length of flattened array
						byte[] flattenedArray = new byte[((SyncronisePackets.Length - 1) * maxChunkSize) + SyncronisePackets[SyncPacketsRecieved].Length]; //check if this actually works later
						
						for (int i = 0, k = 0; i < SyncronisePackets.Length; i++) //flatten the array
						{
							for (int j = 0; j < SyncronisePackets[i].Length; j++)
							{
								flattenedArray[j + k] = SyncronisePackets[i][j];
								k += j;
							}
						}

						//deserialize data from flattened array
						PacketListSyncronisationPacket packet;
						using (var ms = new System.IO.MemoryStream(flattenedArray))
							packet = (PacketListSyncronisationPacket)InternalFormatter.Deserialize(ms);

						var oldHandlers = Handlers;
						Handlers = new List<NetHandler>();

						//do stuff with the deserialized packet
						for (int i = 0; i < packet.Names.Length; i++)
						{
							bool found = false;
							foreach (NetHandler handler in oldHandlers)
							{
								if (packet.Names[i] == handler.StringID)
								{
									found = true;
									Handlers.Add(handler);

									break;
								}
							}

							if (!found)
							{
								if (!packet.NecessaryIndexes[i])
								{
									//this packet isn't entirely necessary, create a placeholder handler with an empty method
									NetHandler newHandler = new NetHandler((ushort)Handlers.Count, packet.Names[i], this);
									Handlers.Add(newHandler);
								}
								else
								{
									ClientDropSelf(false, $"Unknown necessary packet \"{packet.Names[i]}\"! Mods or mod versions may be missing or out of date!");
									return;
								}
							}
						}
					}
				});
			}
			else
			{
				//handle for servers
				SyncronisePacketListHandler = Handle("DoorNet::Shared::Networking::SyncronisePacketListWithHost");
				SyncronisePacketListHandler.Listen((byte[] data, NetHandle sender) => 
				{
					//serialize Handler list to structs, serialize into packets of size <= 4096, cache said packets into SyncronisePackets if not already done, send
					//first packet should be prefixed by the number of packets that should be recieved, others should just be big 'ol streams of bytes

					if (SyncronisePackets == null)
					{
						//go assemble a struct
						string[] ids = new string[Handlers.Count];
						bool[] necessaries = new bool[Handlers.Count];
						for (int i = 0; i < Handlers.Count; i++)
						{
							ids[i] = Handlers[i].StringID;
							necessaries[i] = Handlers[i].Necessary;
						}

						var packet = new PacketListSyncronisationPacket
						{
							Names = ids,
							NecessaryIndexes = necessaries
						};

						//serialize assembled struct
						byte[] serialized;
						using (var ms = new System.IO.MemoryStream())
						{
							InternalFormatter.Serialize(ms, packet);
							serialized = ms.GetBuffer();
						}

						//calculate the number of packets needed to store it
						ushort numPackets = (ushort)Math.Ceiling((double)(serialized.Length + sizeof(ushort)) / maxChunkSize);
						byte[] numPacketsSerialized = BitConverter.GetBytes(numPackets); //serialize the number of packets
						serialized = serialized.Prepend(numPacketsSerialized); //prepend it to data

						SyncronisePackets = new byte[numPackets][];

						for (int i = 0, j = 0; i < numPackets; i++, j += maxChunkSize)
						{
							//segment our serialized data into chunks
							int nextSize;
							if (j + maxChunkSize <= serialized.Length)
								nextSize = maxChunkSize;
							else
								nextSize = serialized.Length - j;

							byte[] nextArray = new byte[nextSize];
							Array.Copy(serialized, j, nextArray, 0, nextSize);

							SyncronisePackets[i] = nextArray;
						}
					}

					foreach (byte[] packet in SyncronisePackets)
						SyncronisePacketListHandler.SendTo(sender, packet, SendMode.Tcp); //send off all segments of our data
				});
			}

			if (type == ManagerType.Client)
			{
				ClientDroppedHandler = Handle("DoorNet::Shared::Networking::ClientDropped");
				ClientDroppedHandler.Listen((byte[] data, NetHandle sender) =>
				{
					string reason = Encoding.Unicode.GetString(data);
					ClientDropSelf(false, reason);
				});
			}
			else
			{
				ClientDroppedHandler = Handle("DoorNet::Shared::Networking::ClientDropped");
			}

			UdpHandshakeHandler = Handle("DoorNet::Shared::Networking::UdpHandshake");
			if (type == ManagerType.Client)
			{
				UdpHandshakeHandler.Listen((byte[] data, NetHandle sender) =>
				{
					if (data[0] == 1)
					{
						bool prevUdpReady = UdpReady;
						UdpHandshakeGuid = null;
						UdpReady = true;


						if (!prevUdpReady)
							ClientEvents.InvokeOnConnect();

						return;
					}

					byte[] guidData = new byte[16]; //i really hope this is the right size for a guid
					Array.Copy(data, 1, guidData, 0, guidData.Length);
					UdpHandshakeGuid = new Guid(guidData);
				});
			}

			Udp = new UdpClient(Port);
			UdpThread = new Thread(UdpThreadListen);

			if (type == ManagerType.Client)
			{
				TcpClient = new TcpClient();
				TcpThread = new Thread(TcpThreadClientListen);
			}
			else
			{
				TcpServer = new TcpListener(IPAddress.Any, Port);
				TcpThread = new Thread(TcpThreadServerListen);
			}

			ClientEvents = new ClientEventContainer();
			ServerEvents = new ServerEventContainer();
		}

		/// <summary>
		/// Registers a new NetHandler with a given ID and returns that handler
		/// If the handler already exists, it returns the existing handler
		/// Throws if it tries to generate a new NetHandler while the NetManager is in use (i.e. listening or connected)
		/// </summary>
		/// <param name="id">The ID of the generated handler</param>
		/// <returns></returns>
		public NetHandler Handle(string id)
		{
			//check if handler already exists, return it if it does
			foreach (NetHandler handler in Handlers)
				if (handler.StringID == id)
					return handler;

			if (InUse) //can't create new handlers if we're already doing stuff
				throw new NetManagerInUseException("New Handlers can only be registered while a NetManager is not in use (i.e. connected or accepting connections)");

			//create a handler
			ushort newIndex = (ushort)Handlers.Count;
			NetHandler newHandler = new NetHandler(newIndex, id, this);
			Handlers.Add(newHandler);

			return newHandler;
		}

		/// <summary>
		/// Handles any packets that were recieved on the network thread, should be called often
		/// </summary>
		public void PollEvents()
		{
			lock (UdpThreadPackets)
			{
				foreach (UdpThreadPacket packet in UdpThreadPackets)
				{
					//resolve handler
					NetHandler foundHandler = null;
					foreach (NetHandler handler in Handlers)
					{
						if (handler.ShortID == packet.Channel)
						{
							foundHandler = handler;
							break;
						}
					}

					if (foundHandler == null)
						continue;

					//resolve handle
					NetHandle foundHandle = null;
					if (Side == ManagerType.Client)
						foundHandle = Clients[0]; //should be the connected handle
					else
						foreach (NetHandle handle in Clients)
						{
							if (handle.EP?.Equals(packet.Sender) ?? false) //needs the null check to prevent nullrefs
							{
								foundHandle = handle;
								break;
							}
						}

					if (foundHandle == null)
					{
						//for udp handshake
						if (foundHandler == UdpHandshakeHandler) 
						{
							Guid recievedGuid = new Guid(packet.Data); //generate guid with recieved data
							foreach (NetHandle handle in Clients)
								if (recievedGuid == handle.UdpHandshakeGuid)
								{
									foundHandle = handle;
									break;
								}
							
							if (foundHandle != null && recievedGuid == foundHandle.UdpHandshakeGuid)
							{
								foundHandle.EP = packet.Sender;
								foundHandle.HasUdp = true;

								//make ok packet
								byte[] newPacket = new byte[] { 1 };
								foundHandler.SendTo(foundHandle, newPacket, SendMode.Tcp);

								ServerEvents.InvokeOnClientConnect(foundHandle);
							}
						}
						else
							continue;
					}
					else
						foundHandler.RecieveData(packet.Data, foundHandle);
				}
				UdpThreadPackets.Clear();
			}

			lock (TcpThreadPackets)
			{
				foreach (TcpThreadPacket packet in TcpThreadPackets)
				{
					//resolve handler
					NetHandler foundHandler = null;
					foreach (NetHandler handler in Handlers)
					{
						if (handler.ShortID == packet.Channel)
						{
							foundHandler = handler;
							break;
						}
					}

					if (foundHandler == null)
						continue;

					//resolve handle
					NetHandle foundHandle = null;
					if (Side == ManagerType.Client)
						foundHandle = Clients[0];
					else
						foreach (NetHandle handle in Clients)
						{
							if (handle.Client == packet.Client)
							{
								foundHandle = handle;
								break;
							}
						}

					if (foundHandle == null)
						continue;

					foundHandler.RecieveData(packet.Data, foundHandle);
				}
				TcpThreadPackets.Clear();
			}

			if (Side == ManagerType.Client && UdpHandshakeGuid != null && !UdpReady)
			{
				UdpHandshakeHandler.SendTo(Clients[0], UdpHandshakeGuid.Value.ToByteArray(), SendMode.Udp);
			}
		}

		/// <summary>
		/// Sends a packet to a given NetHandle
		/// NetHandle.SendTo should generally be used instead of this method
		/// </summary>
		/// <param name="reciever">The NetHandle the data is being sent to</param>
		/// <param name="handlerID">The ID of the NetHandler it is being sent over</param>
		/// <param name="data">The data sent</param>
		/// <param name="sendMode">Whether the data should be sent over TCP or UDP</param>
		public void SendRaw(NetHandle reciever, ushort handlerID, byte[] data, SendMode sendMode)
		{
			if (sendMode == SendMode.Tcp)
			{
				//tcp header contains packet length, then channel id
				byte[] length = BitConverter.GetBytes((ushort)(data.Length));
				byte[] id = BitConverter.GetBytes(handlerID);
				byte[] sentData = data.Prepend(id).Prepend(length); //length first, then id, then data

				NetworkStream stream;

				if (Side == ManagerType.Server)
					stream = reciever.Client.GetStream();
				else
					stream = TcpClient.GetStream();

				stream.Write(sentData, 0, sentData.Length);
				stream.Flush();
			}
			else
			{
				if ((Side == ManagerType.Server && !reciever.HasUdp) || (Side == ManagerType.Client && !UdpReady && handlerID != UdpHandshakeHandler.ShortID)) //hell check fix later
					return;

				//udp header contains channel id
				byte[] id = BitConverter.GetBytes(handlerID);
				byte[] sentData = data.Prepend(id); //id first, then data

				Udp.Send(sentData, sentData.Length, reciever.EP);
			}
		}

		/// <summary>
		/// Starts a server, starting the network threads and begins listening for packets
		/// Can only be called on Server NetManagers
		/// </summary>
		public void StartServer()
		{
			if (Side != ManagerType.Server)
				throw new IncorrectManagerTypeException("StartServer() can only be called on server NetManagers");

			InUse = true;

			//start listeners
			TcpServer.Start();

			TcpThread.Start();
			UdpThread.Start();

			ServerEvents.InvokeOnServerStart();
		}

		/// <summary>
		/// Stops a server, stopping the network threads and stops listening for packets
		/// Can only be called on Server NetManagers
		/// </summary>
		public void StopServer()
		{
			if (Side != ManagerType.Server)
				throw new IncorrectManagerTypeException("StopServer() can only be called on server NetManagers");

			ServerEvents.InvokeOnServerClose();

			InUse = false;

			//stop threads
			TcpThread.Abort();
			UdpThread.Abort();

			while (Clients.Count != 0)
				DropClient(Clients[0], "Server shutting down");

			TcpServer.Stop();
		}

		/// <summary>
		/// Connects to a server
		/// Can only be called on Client NetManagers
		/// </summary>
		/// <param name="hostEp">The EP of the host you're connecting to</param>
		public void Connect(IPEndPoint hostEp)
		{
			if (Side != ManagerType.Client)
				throw new IncorrectManagerTypeException("Connect() can only be called on client NetManagers");

			//connect to server
			TcpClient.Connect(hostEp);

			InUse = true;

			//construct fake nethandle for host
			NetHandle hostClient = new NetHandle 
			{
				IsConnectedHost = true,
				Name = "SYSTEM",
				EP = hostEp
			};

			Clients.Add(hostClient);

			TcpThread.Start();
			UdpThread.Start();

			//TODO: make stuff for if it fails later
		}

		/// <summary>
		/// Drops a client from the server, sending the reason to the client
		/// Can only be called on Server NetManagers
		/// </summary>
		/// <param name="client">The client to be dropped from the server</param>
		/// <param name="reason">The reason for them being dropped</param>
		public void DropClient(NetHandle client, string reason = "")
		{
			if (Side == ManagerType.Client)
				throw new IncorrectManagerTypeException("DropClient() can only be called on server NetManagers");

			ServerEvents.InvokeOnClientDisconnect(client, reason);

			byte[] packet = Encoding.Unicode.GetBytes(reason);
			ClientDroppedHandler.SendTo(client, packet, SendMode.Tcp);

			client.InUse = false;

			Clients.Remove(client);
			client.ClientPollThread.Abort();
			client.Client.Close();
		}

		/// <summary>
		/// Causes a client to drop itself from the server it is connected to
		/// Can only be called on Client NetManagers
		/// </summary>
		/// <param name="causedBySelf">Whether the disconnect was caused by the client, or forced by the server</param>
		/// <param name="reason">The reason the client disconnected</param>
		public void ClientDropSelf(bool causedBySelf = true, string reason = "")
		{
			if (Side == ManagerType.Server)
				throw new IncorrectManagerTypeException("ClientDropSelf() can only be called on client NetManagers");

			InUse = false;
			Clients.Clear();

			TcpClient.Close();
			Udp.Close();

			UdpThread.Abort();
			TcpThread.Abort();

			ClientEvents.InvokeOnDisconnect(false, reason);
		}

		private void UdpThreadListen()
		{
			while (InUse)
			{
				//recieve data from any client
				IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = new byte[0];

				try
				{
					data = Udp.Receive(ref ep);
				} 
				catch (Exception ex)
				{
					Console.WriteLine(ex.InnerException); //fix this later
				}

				//get header
				ushort handler = BitConverter.ToUInt16(data, 0);

				//get data bytes
				byte[] dataWithoutHeaders = new byte[data.Length - UDP_PACKET_HEADER_SIZE];
				Array.Copy(data, UDP_PACKET_HEADER_SIZE, dataWithoutHeaders, 0, dataWithoutHeaders.Length);

				UdpThreadPacket packet = new UdpThreadPacket
				{
					Channel = handler,
					Data = dataWithoutHeaders,
					Sender = ep
				};

				lock (UdpThreadPackets)
					UdpThreadPackets.Add(packet);

				Thread.Sleep(50);
			}
		}

		private void TcpThreadClientListen()
		{
			NetworkStream clientStream = TcpClient.GetStream();
			while (InUse)
			{
				if (!clientStream.DataAvailable)
					continue;

				byte[] data = new byte[4096];
				int bytesRead = clientStream.Read(data, 0, data.Length);
				int bytesUsed = 0;

				while (bytesUsed != bytesRead)
				{
					ushort length = BitConverter.ToUInt16(data, 0);
					ushort channelId = BitConverter.ToUInt16(data, sizeof(ushort));

					byte[] headerlessData = new byte[length];
					Array.Copy(data, sizeof(ushort) * 2, headerlessData, 0, length);

					TcpThreadPacket packet = new TcpThreadPacket
					{
						Channel = channelId,
						Data = headerlessData
					};

					lock (TcpThreadPackets)
						TcpThreadPackets.Add(packet);

					bytesUsed += length + (sizeof(ushort) * 2);
				}

				Thread.Sleep(50);
			}
		}

		private void TcpThreadServerListen()
		{
			while (InUse)
			{
				TcpClient newClient = TcpServer.AcceptTcpClient();

				NetHandle newHandle = new NetHandle
				{
					IsConnectedHost = false,
					Client = newClient
				};

				Thread clientThread = new Thread(TcpThreadServerListenClients);
				newHandle.ClientPollThread = clientThread;

				lock (Clients)
					Clients.Add(newHandle);

				clientThread.Start(newHandle);

				lock (newClient)
				{
					newHandle.UdpHandshakeGuid = Guid.NewGuid();
					byte[] sentData = newHandle.UdpHandshakeGuid.ToByteArray().Prepend((byte)0);

					UdpHandshakeHandler.SendTo(newHandle, sentData, SendMode.Tcp);
				}

				Thread.Sleep(50);
			}
		}

		private void TcpThreadServerListenClients(object clientObj)
		{
			NetHandle client = (NetHandle)clientObj;
			NetworkStream clientStream = client.Client.GetStream();
			while (client.InUse && InUse)
			{
				if (!clientStream.DataAvailable)
					continue;

				byte[] data = new byte[4096];
				int bytesRead = clientStream.Read(data, 0, data.Length);
				int bytesUsed = 0;

				while (bytesUsed != bytesRead)
				{
					ushort length = BitConverter.ToUInt16(data, bytesUsed);
					ushort channelId = BitConverter.ToUInt16(data, bytesUsed + sizeof(ushort));

					byte[] headerlessData = new byte[length];
					Array.Copy(data, bytesUsed + (sizeof(ushort) * 2), headerlessData, 0, length);

					TcpThreadPacket packet = new TcpThreadPacket
					{
						Channel = channelId,
						Data = headerlessData,
						Client = client.Client
					};

					lock (TcpThreadPackets)
						TcpThreadPackets.Add(packet);

					bytesUsed += length + (sizeof(ushort) * 2);
				}

				Thread.Sleep(50);
			}
		}
	}
}