using BobNet;

namespace DoorNet.Server.GameLogic
{
	public static class NetPlayerExtensions
	{
		public static NetPlayer GetPlayer(this NetClient client)
		{
			foreach (NetPlayer player in NetPlayer.Players)
			{
				if (player.Client == client)
					return player;
			}

			return null;
		}
	}
}
