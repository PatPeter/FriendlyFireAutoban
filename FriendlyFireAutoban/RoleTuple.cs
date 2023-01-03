using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerRoles;

namespace FriendlyFireAutoban
{
	internal struct RoleTuple
	{
		public RoleTypeId KillerRole { get; set; }
		public RoleTypeId VictimRole { get; set; }

		public RoleTuple(RoleTypeId killerRole, RoleTypeId victimRole)
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
