using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FriendlyFireAutoban
{
	public class TeamTuple
	{
		public Team KillerTeam { get; set; }
		public Team VictimTeam { get; set; }

		public TeamTuple(Team killerTeam, Team victimRole)
		{
			this.KillerTeam = killerTeam;
			this.VictimTeam = victimRole;
		}

		public override string ToString()
		{
			return KillerTeam + "," + VictimTeam;
		}
	}
}
