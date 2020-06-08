using System;

namespace DoorNet.Shared.Networking.Exceptions
{
	public class NetManagerInUseException : Exception
	{
		public NetManagerInUseException() : base() { }
		public NetManagerInUseException(string message) : base(message) { }
		public NetManagerInUseException(string message, Exception inner) : base(message, inner) { }
	}
}
