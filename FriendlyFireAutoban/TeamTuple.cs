using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendlyFireAutoban
{
	public struct TeamTuple
	{
		public Team KillerTeam, VictimTeam;

		public TeamTuple(Team killerTeam, Team victimRole)
		{
			this.KillerTeam = killerTeam;
			this.VictimTeam = victimRole;
		}

		public override string ToString()
		{
			return KillerTeam + ":" + VictimTeam;
		}
	}
}
