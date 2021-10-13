using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendlyFireAutoban
{
	public class Teamkiller
	{
		public int PlayerId;
		public string Name;
		public string UserId;
		public string IpAddress;
		// Must keep track of team and role for when the player is sent to spectator and events are still running
		public short Team;
		public short Role;
		// For kdsafe
		public int Kills;
		public int Deaths;
		public List<Teamkill> Teamkills = new List<Teamkill>();
		//public Timer Timer;
		public int TimerCountdown = 0;

		public Teamkiller(int playerId, string name, string userId, string ipAddress)
		{
			this.PlayerId = playerId;
			this.Name = name;
			this.UserId = userId;
			this.IpAddress = ipAddress;
		}

		public float GetKDR()
		{
			return Deaths == 0 ? 0 : (float)Kills / Deaths;
		}

		public override bool Equals(object obj)
		{
			var teamkiller = obj as Teamkiller;
			return teamkiller != null &&
				   PlayerId == teamkiller.PlayerId;
		}

		public override int GetHashCode()
		{
			var hashCode = -1156428363;
			hashCode = hashCode * -1521134295 + PlayerId.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IpAddress);
			return hashCode;
		}

		public override string ToString()
		{
			return PlayerId + " " + Name + " " + UserId + " " + IpAddress;
		}
	}
}
