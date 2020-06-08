using System;

namespace DoorNet.Shared.Networking.Exceptions
{
	public class IncorrectManagerTypeException : Exception
	{
		public IncorrectManagerTypeException() : base() { }
		public IncorrectManagerTypeException(string message) : base(message) { }
		public IncorrectManagerTypeException(string message, Exception inner) : base(message, inner) { }
	}
}
