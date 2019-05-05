using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FriendlyFireAutoban
{

	class Teamkiller
	{
		public int PlayerId;
		public string Name;
		public string SteamId;
		public string IpAddress;
		public List<Teamkill> Teamkills = new List<Teamkill>();
		//public Timer Timer;

		public Teamkiller(int playerId, string name, string steamId, string ipAddress)
		{
			this.PlayerId = playerId;
			this.Name = name;
			this.SteamId = steamId;
			this.IpAddress = ipAddress;
		}

		public override bool Equals(object obj)
		{
			var teamkiller = obj as Teamkiller;
			return teamkiller != null &&
				   PlayerId == teamkiller.PlayerId &&
				   Name == teamkiller.Name &&
				   SteamId == teamkiller.SteamId &&
				   IpAddress == teamkiller.IpAddress;
		}

		public override int GetHashCode()
		{
			var hashCode = -1156428363;
			hashCode = hashCode * -1521134295 + PlayerId.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SteamId);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IpAddress);
			return hashCode;
		}

		public override string ToString()
		{
			return PlayerId + " " + Name + " " + SteamId + " " + IpAddress;
		}
	}
}
