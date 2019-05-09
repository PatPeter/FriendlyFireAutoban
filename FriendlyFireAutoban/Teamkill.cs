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
		public string KillerSteamId;
		public TeamRole KillerTeamRole;
		public string VictimName;
		public string VictimSteamId;
		public TeamRole VictimTeamRole;
		public bool VictimDisarmed;
		public DamageType DamageType;
		public int Duration;

		public Teamkill(string killerName, string killerSteamId, TeamRole killerTeamRole, string victimName, string victimSteamId, TeamRole victimTeamRole, bool victimDisarmed, DamageType damageType, int duration)
		{
			this.KillerName = killerName;
			this.KillerSteamId = killerSteamId;
			this.KillerTeamRole = killerTeamRole;
			this.VictimName = victimName;
			this.VictimSteamId = victimSteamId;
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
				case Role.CLASSD:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleDclass");
					break;

				case Role.SCIENTIST:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleScientist");
					break;

				case Role.FACILITY_GUARD:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleGuard");
					break;

				case Role.NTF_CADET:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleCadet");
					break;

				case Role.NTF_LIEUTENANT:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleLieutenant");
					break;

				case Role.NTF_COMMANDER:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleCommander");
					break;

				case Role.NTF_SCIENTIST:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleNTFScientist");
					break;

				case Role.CHAOS_INSURGENCY:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleChaos");
					break;

				case Role.TUTORIAL:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleTutorial");
					break;
			}
			retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleSeparator");
			if (VictimDisarmed)
			{
				retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleDisarmed");
			}
			switch (VictimTeamRole.Role)
			{
				case Role.CLASSD:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleDclass");
					break;

				case Role.SCIENTIST:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleScientist");
					break;

				case Role.FACILITY_GUARD:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleGuard");
					break;

				case Role.NTF_CADET:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleCadet");
					break;

				case Role.NTF_LIEUTENANT:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleLieutenant");
					break;

				case Role.NTF_COMMANDER:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleCommander");
					break;

				case Role.NTF_SCIENTIST:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleNTFScientist");
					break;

				case Role.CHAOS_INSURGENCY:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleChaos");
					break;

				case Role.TUTORIAL:
					retval += FriendlyFireAutobanPlugin.GetInstance().GetTranslation("roleTutorial");
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
				   KillerSteamId == teamkill.KillerSteamId &&
				   EqualityComparer<TeamRole>.Default.Equals(KillerTeamRole, teamkill.KillerTeamRole) &&
				   VictimName == teamkill.VictimName &&
				   VictimSteamId == teamkill.VictimSteamId &&
				   EqualityComparer<TeamRole>.Default.Equals(VictimTeamRole, teamkill.VictimTeamRole) &&
				   VictimDisarmed == teamkill.VictimDisarmed &&
				   DamageType == teamkill.DamageType &&
				   Duration == teamkill.Duration;
		}

		public override int GetHashCode()
		{
			var hashCode = -153347006;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KillerName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KillerSteamId);
			hashCode = hashCode * -1521134295 + EqualityComparer<TeamRole>.Default.GetHashCode(KillerTeamRole);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VictimName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VictimSteamId);
			hashCode = hashCode * -1521134295 + EqualityComparer<TeamRole>.Default.GetHashCode(VictimTeamRole);
			hashCode = hashCode * -1521134295 + VictimDisarmed.GetHashCode();
			hashCode = hashCode * -1521134295 + DamageType.GetHashCode();
			hashCode = hashCode * -1521134295 + Duration.GetHashCode();
			return hashCode;
		}
	}
}
