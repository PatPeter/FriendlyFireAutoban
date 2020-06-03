using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FriendlyFireAutoban
{

	class Teamkill
	{
		public string KillerName;
		public string KillerUserId;
		public TeamRole KillerTeamRole;
		public string VictimName;
		public string VictimUserId;
		public TeamRole VictimTeamRole;
		public bool VictimDisarmed;
		public DamageType DamageType;
		public int Duration;

		public Teamkill(string killerName, string killerUserId, TeamRole killerTeamRole, string victimName, string victimSteamId, TeamRole victimTeamRole, bool victimDisarmed, DamageType damageType, int duration)
		{
			this.KillerName = killerName;
			this.KillerUserId = killerUserId;
			this.KillerTeamRole = killerTeamRole;
			this.VictimName = victimName;
			this.VictimUserId = victimSteamId;
			this.VictimTeamRole = victimTeamRole;
			this.VictimDisarmed = victimDisarmed;
			this.DamageType = damageType;
			this.Duration = duration;
		}

		public string GetRoleDisplay()
		{
			string retval = "(";
			switch (KillerTeamRole.Role)
			{
				case Smod2.API.RoleType.CLASSD:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_dclass");
					break;

				case Smod2.API.RoleType.SCIENTIST:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_scientist");
					break;

				case Smod2.API.RoleType.FACILITY_GUARD:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_guard");
					break;

				case Smod2.API.RoleType.NTF_CADET:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_cadet");
					break;

				case Smod2.API.RoleType.NTF_LIEUTENANT:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_lieutenant");
					break;

				case Smod2.API.RoleType.NTF_COMMANDER:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_commander");
					break;

				case Smod2.API.RoleType.NTF_SCIENTIST:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_ntf_scientist");
					break;

				case Smod2.API.RoleType.CHAOS_INSURGENCY:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_chaos");
					break;

				case Smod2.API.RoleType.TUTORIAL:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_tutorial");
					break;
			}
			retval += " " + FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_separator") + " ";
			if (VictimDisarmed)
			{
				retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_disarmed");
			}
			switch (VictimTeamRole.Role)
			{
				case Smod2.API.RoleType.CLASSD:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_dclass");
					break;

				case Smod2.API.RoleType.SCIENTIST:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_scientist");
					break;

				case Smod2.API.RoleType.FACILITY_GUARD:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_guard");
					break;

				case Smod2.API.RoleType.NTF_CADET:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_cadet");
					break;

				case Smod2.API.RoleType.NTF_LIEUTENANT:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_lieutenant");
					break;

				case Smod2.API.RoleType.NTF_COMMANDER:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_commander");
					break;

				case Smod2.API.RoleType.NTF_SCIENTIST:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_ntf_scientist");
					break;

				case Smod2.API.RoleType.CHAOS_INSURGENCY:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_chaos");
					break;

				case Smod2.API.RoleType.TUTORIAL:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("role_tutorial");
					break;
			}
			retval += ")";
			return retval;
		}

		public override bool Equals(object obj)
		{
			var teamkill = obj as Teamkill;
			return teamkill != null &&
				   KillerName == teamkill.KillerName &&
				   KillerUserId == teamkill.KillerUserId &&
				   EqualityComparer<TeamRole>.Default.Equals(KillerTeamRole, teamkill.KillerTeamRole) &&
				   VictimName == teamkill.VictimName &&
				   VictimUserId == teamkill.VictimUserId &&
				   EqualityComparer<TeamRole>.Default.Equals(VictimTeamRole, teamkill.VictimTeamRole) &&
				   VictimDisarmed == teamkill.VictimDisarmed &&
				   DamageType == teamkill.DamageType &&
				   Duration == teamkill.Duration;
		}

		public override int GetHashCode()
		{
			var hashCode = -153347006;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KillerName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KillerUserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<TeamRole>.Default.GetHashCode(KillerTeamRole);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VictimName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VictimUserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<TeamRole>.Default.GetHashCode(VictimTeamRole);
			hashCode = hashCode * -1521134295 + VictimDisarmed.GetHashCode();
			hashCode = hashCode * -1521134295 + DamageType.GetHashCode();
			hashCode = hashCode * -1521134295 + Duration.GetHashCode();
			return hashCode;
		}
	}
}
