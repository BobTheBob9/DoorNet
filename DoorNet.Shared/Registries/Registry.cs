using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;

namespace DoorNet.Shared.Registries
{
	public class Registry<T>
	{
		public ReadOnlyCollection<string> Entries
		{
			get => _Entries.AsReadOnly();
		}
		public ReadOnlyCollection<T> Items
		{
			get => _Items.AsReadOnly();
		}

		protected List<string> _Entries = new List<string>();
		protected List<T> _Items = new List<T>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="item"></param>
		public virtual void CreateEntry(string id, T item)
		{
			if (_Entries.Contains(id))
				throw new InvalidOperationException("Cannot create an entry that already exists");

			_Entries.Add(id);
			_Items.Add(item);
		}

		public virtual T GetItem(string id)
		 => _Items[_Entries.IndexOf(id)];

		public void SyncroniseChannels(SyncableStringArrayChannel channel)
		{
			//send strings to server, reorganise channels on recieve
			void onClientRecieve(object data, NetClient client)
			{
				SyncableStringArray array = (SyncableStringArray)data;

				if (!array.SendingIndexes)
					return;

				List<int> invalidIndexes = new List<int>();
				string[] newEntries = new string[_Entries.Count];
				int j = 0;
				for (int i = 0; i < array.Indexes.Length; i++)
				{
					if (array.Indexes[i] == -1)
					{
						invalidIndexes.Add(i);
						continue;
					}

					newEntries[j] = _Entries[i];
					j++;
				}

				for (int i = 0; i < invalidIndexes.Count; i++)
					newEntries[i + j] = _Entries[invalidIndexes[i]];

				_Entries = new List<string>(newEntries);

				channel.OnRecieveSerialized -= onClientRecieve;
			}

			channel.OnRecieveSerialized += onClientRecieve;

			SyncableStringArray sentArray = new SyncableStringArray(new Guid(), _Entries.ToArray());
			channel.SendSerialized(SendMode.Tcp, sentArray, channel.Manager.Server);
		}

		public void ListenForChannelSyncronisation(SyncableStringArrayChannel channel)
		{
			channel.OnRecieveSerialized += (object data, NetClient client) =>
			{
				SyncableStringArray array = (SyncableStringArray)data;
				if (array.SendingIndexes)
					return;

				//recieve strings, send indexes
				if (array.Items.Length < _Entries.Count)
				{
					client.Disconnect("Missing items from registry");
					return;
				}

				//todo: this will allow for necessary/unnecessary items later but atm i just cannot be fucked y'know 
				foreach (var entry in array.Items)
					if (!_Entries.Contains(entry.String))
					{
						client.Disconnect("Missing items from registry");
						return;
					}

				int[] indexes = new int[array.Items.Length];
				for (int i = 0; i < indexes.Length; i++)
				{
					if (!_Entries.Contains(array.Items[i].String))
						indexes[i] = -1;
					else
						indexes[i] = _Entries.IndexOf(array.Items[i].String);
				}

				channel.SendSerialized(SendMode.Tcp, new SyncableStringArray(new Guid(), indexes), client);
			};
		}
	}
}
