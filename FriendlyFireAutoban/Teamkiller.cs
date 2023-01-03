using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerRoles;

namespace FriendlyFireAutoban
{
	internal class Teamkiller
	{
		public int            PlayerId       { get; set; }
		public string         Nickname       { get; set; }
		public string         UserId         { get; set; }
		public string         IPAddress      { get; set; }
		// Must keep track of team and role for when the player is sent to spectator and events are still running
		public Team           Team           { get; set; }
		public RoleTypeId     PlayerRole     { get; set; }
		public int            Kills          { get; set; } = 0;
		public int            Deaths         { get; set; } = 0;
		public List<Teamkill> Teamkills      { get; set; } = new List<Teamkill>();
		public int            TimerCountdown { get; set; } = -1;
		public bool           Disconnected   { get; set; } = false;
		public bool           Banned         { get; set; } = false;

		public Teamkiller(int playerId, string name, string userId, string ipAddress)
		{
			this.PlayerId = playerId;
			this.Nickname = name;
			this.UserId = userId;
			this.IPAddress = ipAddress;
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
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Nickname);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IPAddress);
			return hashCode;
		}

		public override string ToString()
		{
			return PlayerId + " " + Nickname + " " + UserId + " " + IPAddress;
		}
	}
}
