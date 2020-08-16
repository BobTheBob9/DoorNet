using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BobNet;

namespace DoorNet.Shared.Networking
{
	public class RatelimitPreprocessor : NetChannel.Preprocessor
	{
		internal static object RecieveLock = new object();

		public TimeSpan Cooldown { get; private set; }
		public DateTime LastRecieve { get; private set; } = DateTime.MinValue;
 
		private RatelimitPreprocessor() { } 
		public RatelimitPreprocessor(TimeSpan cooldown)
		{
			Cooldown = cooldown;
		}

		public override bool Process(ref byte[] data, SendMode sendMode, NetClient sender)
		{
			lock (RecieveLock)
			{
				if (DateTime.Now.Subtract(Cooldown) > LastRecieve)
				{
					LastRecieve = DateTime.Now;
					return true;
				}
			}

			return false;
		}
	}
}
