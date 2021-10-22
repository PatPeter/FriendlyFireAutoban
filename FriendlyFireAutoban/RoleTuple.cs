using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FriendlyFireAutoban
{
	internal struct RoleTuple
	{
		public RoleType KillerRole { get; set; }
		public RoleType VictimRole { get; set; }

		public RoleTuple(RoleType killerRole, RoleType victimRole)
		{
			this.KillerRole = killerRole;
			this.VictimRole = victimRole;
		}

		public override string ToString()
		{
			return KillerRole + "," + VictimRole;
		}
	}
}
