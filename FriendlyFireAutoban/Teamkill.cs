using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FriendlyFireAutoban
{
	internal class Teamkill
	{
		public string KillerName     { get; set; }
		public string KillerUserId   { get; set; }
		public short  KillerTeamRole { get; set; }
		public string VictimName     { get; set; }
		public string VictimUserId   { get; set; }
		public short  VictimTeamRole { get; set; }
		public bool   VictimDisarmed { get; set; }
		public short  DamageType     { get; set; }
		public int    Duration       { get; set; }

		public Teamkill(string killerName, string killerSteamId, short killerTeamRole, string victimName, string victimSteamId, short victimTeamRole, bool victimDisarmed, short damageType, int duration)
		{
			this.KillerName     = killerName;
			this.KillerUserId   = killerSteamId;
			this.KillerTeamRole = killerTeamRole;
			this.VictimName     = victimName;
			this.VictimUserId   = victimSteamId;
			this.VictimTeamRole = victimTeamRole;
			this.VictimDisarmed = victimDisarmed;
			this.DamageType     = damageType;
			this.Duration       = duration;
		}

		public override string ToString()
		{
			return "(" + KillerName + " killed " + VictimName + ")";
		}

		public string GetRoleDisplay()
		{
			string retval = "(";
			switch (KillerTeamRole)
			{
				case (short)RoleType.ClassD:
					retval += Plugin.Instance.GetTranslation("role_dclass");
					break;

				case (short)RoleType.Scientist:
					retval += Plugin.Instance.GetTranslation("role_scientist");
					break;

				case (short)RoleType.FacilityGuard:
					retval += Plugin.Instance.GetTranslation("role_guard");
					break;

				case (short)RoleType.NtfPrivate:
					retval += Plugin.Instance.GetTranslation("role_cadet");
					break;

				case (short)RoleType.NtfSergeant:
					retval += Plugin.Instance.GetTranslation("role_lieutenant");
					break;

				case (short)RoleType.NtfCaptain:
					retval += Plugin.Instance.GetTranslation("role_commander");
					break;

				case (short)RoleType.NtfSpecialist:
					retval += Plugin.Instance.GetTranslation("role_ntf_scientist");
					break;

				case (short)RoleType.ChaosConscript:
					retval += Plugin.Instance.GetTranslation("role_chaos");
					break;

				case (short)RoleType.ChaosRifleman:
					retval += Plugin.Instance.GetTranslation("role_chaos");
					break;

				case (short)RoleType.ChaosRepressor:
					retval += Plugin.Instance.GetTranslation("role_chaos");
					break;

				case (short)RoleType.ChaosMarauder:
					retval += Plugin.Instance.GetTranslation("role_chaos");
					break;

				case (short)RoleType.Tutorial:
					retval += Plugin.Instance.GetTranslation("role_tutorial");
					break;
			}
			retval += " " + Plugin.Instance.GetTranslation("role_separator") + " ";
			if (VictimDisarmed)
			{
				retval += Plugin.Instance.GetTranslation("role_disarmed");
			}
			switch (VictimTeamRole)
			{
				case (short)RoleType.ClassD:
					retval += Plugin.Instance.GetTranslation("role_dclass");
					break;

				case (short)RoleType.Scientist:
					retval += Plugin.Instance.GetTranslation("role_scientist");
					break;

				case (short)RoleType.FacilityGuard:
					retval += Plugin.Instance.GetTranslation("role_guard");
					break;

				case (short)RoleType.NtfPrivate:
					retval += Plugin.Instance.GetTranslation("role_cadet");
					break;

				case (short)RoleType.NtfSergeant:
					retval += Plugin.Instance.GetTranslation("role_lieutenant");
					break;

				case (short)RoleType.NtfCaptain:
					retval += Plugin.Instance.GetTranslation("role_commander");
					break;

				case (short)RoleType.NtfSpecialist:
					retval += Plugin.Instance.GetTranslation("role_ntf_scientist");
					break;

				case (short)RoleType.ChaosConscript:
					retval += Plugin.Instance.GetTranslation("role_chaos");
					break;

				case (short)RoleType.ChaosRifleman:
					retval += Plugin.Instance.GetTranslation("role_chaos");
					break;

				case (short)RoleType.ChaosRepressor:
					retval += Plugin.Instance.GetTranslation("role_chaos");
					break;

				case (short)RoleType.ChaosMarauder:
					retval += Plugin.Instance.GetTranslation("role_chaos");
					break;

				case (short)RoleType.Tutorial:
					retval += Plugin.Instance.GetTranslation("role_tutorial");
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
				   EqualityComparer<short>.Default.Equals(KillerTeamRole, teamkill.KillerTeamRole) &&
				   VictimName == teamkill.VictimName &&
				   VictimUserId == teamkill.VictimUserId &&
				   EqualityComparer<short>.Default.Equals(VictimTeamRole, teamkill.VictimTeamRole) &&
				   VictimDisarmed == teamkill.VictimDisarmed &&
				   DamageType == teamkill.DamageType &&
				   Duration == teamkill.Duration;
		}

		public override int GetHashCode()
		{
			var hashCode = -153347006;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KillerName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KillerUserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<short>.Default.GetHashCode(KillerTeamRole);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VictimName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VictimUserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<short>.Default.GetHashCode(VictimTeamRole);
			hashCode = hashCode * -1521134295 + VictimDisarmed.GetHashCode();
			hashCode = hashCode * -1521134295 + DamageType.GetHashCode();
			hashCode = hashCode * -1521134295 + Duration.GetHashCode();
			return hashCode;
		}
	}
}
