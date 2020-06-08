using DoorNet.Shared.Networking.Extensions;

namespace DoorNet.Shared.Networking
{
	public class NetHandler
	{
		public delegate void Handler(byte[] data, NetHandle sender);

		public bool Necessary     { get;  private set; }
		public ushort ShortID	  { get; internal set; }
		public string StringID	  { get;  private set; }
		public NetManager Manager { get;  private set; }

		private Handler[] Handlers = new Handler[0];

		internal NetHandler(ushort uid, string sid, NetManager manager)
		{
			ShortID = uid;
			StringID = sid;
			Manager = manager;
		}

		public void Listen(Handler handler) =>
			Handlers = Handlers.Append(handler);
		

		public void RecieveData(byte[] data, NetHandle sender)
		{
			foreach (Handler handler in Handlers)
				handler(data, sender);
		}

		public void SendTo(NetHandle reciever, byte[] data, SendMode sendMode)
		{
			Manager.SendRaw(reciever, ShortID, data, sendMode);
		}

		public void Broadcast(byte[] data, SendMode sendMode)
		{
			foreach (NetHandle client in Manager.Clients)
				SendTo(client, data, sendMode);
		}
	}
}
