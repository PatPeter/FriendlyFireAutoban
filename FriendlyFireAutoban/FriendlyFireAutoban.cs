using FriendlyFireAutoban.EventHandlers;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.EventHandlers;
using Smod2.Commands;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System;

namespace FriendlyFireAutoban
{
	[PluginDetails(
		author = "PatPeter",
		name = "Friendly Fire Autoban",
		description = "Plugin that autobans players for friendly firing.",
		id = "patpeter.friendly.fire.autoban",
		version = "3.0.2.40",
		SmodMajor = 3,
		SmodMinor = 2,
		SmodRevision = 2
		)]
	class FriendlyFireAutobanPlugin : Plugin
	{
		internal bool duringRound = false;
		internal bool enable = true;
		internal bool outall = false;
		internal int system = 1;
		internal int amount = 5;
		internal int length = 1440;
		internal int expire = 60;
		internal int noguns = 0;
		internal int tospec = 0;
		internal int kicker = 0;
		internal int bomber = 0;
		internal Dictionary<string, List<string>> ipAddressToSteamList = new Dictionary<string, List<string>>();
		internal List<TeamkillTuple> teamkillMatrix = new List<TeamkillTuple>();
		internal Dictionary<int, int> teamkillScaled = new Dictionary<int, int>();

		internal Dictionary<string, List<Teamkill>> teamkillCounter = new Dictionary<string, List<Teamkill>>();
		internal Dictionary<string, Timer> teamkillTimers = new Dictionary<string, Timer>();
		internal Dictionary<string, Teamkill> teamkillVictims = new Dictionary<string, Teamkill>();

		public override void OnEnable()
		{
			if (this.outall)
			{
				this.Info("friendly_fire_autoban_enable default value: " + this.GetConfigBool("friendly_fire_autoban_enable"));
				this.Info("friendly_fire_autoban_system default value: " + this.GetConfigInt("friendly_fire_autoban_system"));
				string matrix = "";
				foreach (string s in this.GetConfigList("friendly_fire_autoban_matrix"))
				{
					if (matrix.Length == 0)
					{
						matrix += s;
					}
					else
					{
						matrix += ',' + s;
					}
				}
				this.Info("friendly_fire_autoban_matrix default value: " + matrix);
				this.Info("friendly_fire_autoban_amount default value: " + this.GetConfigInt("friendly_fire_autoban_amount"));
				this.Info("friendly_fire_autoban_length default value: " + this.GetConfigInt("friendly_fire_autoban_length"));
				this.Info("friendly_fire_autoban_expire default value: " + this.GetConfigInt("friendly_fire_autoban_expire"));
				string scaled = "";
				foreach (string s in this.GetConfigList("friendly_fire_autoban_scaled"))
				{
					if (scaled.Length == 0)
					{
						scaled += s;
					}
					else
					{
						scaled += ',' + s;
					}
				}
				this.Info("friendly_fire_autoban_scaled default value: " + scaled);
				this.Info("friendly_fire_autoban_noguns default value: " + this.GetConfigInt("friendly_fire_autoban_noguns"));
				this.Info("friendly_fire_autoban_tospec default value: " + this.GetConfigInt("friendly_fire_autoban_tospec"));
				this.Info("friendly_fire_autoban_kicker default value: " + this.GetConfigInt("friendly_fire_autoban_kicker"));
				string immune = "";
				foreach (string s in this.GetConfigList("friendly_fire_autoban_immune"))
				{
					if (immune.Length == 0)
					{
						immune += s;
					}
					else
					{
						immune += ',' + s;
					}
				}
				this.Info("friendly_fire_autoban_immune default value: " + immune);
			}
		}

		public override void OnDisable()
		{
			/*foreach (Timer t in teamkillTimers.Values)
			{
				t.Dispose();
			}
			duringRound = false;
			enable = false;
			outall = false;
			system = 1;
			amount = 5;
			length = 1440;
			expire = 60;
			noguns = 0;
			tospec = 0;
			kicker = 0;
			teamkillCounter = new Dictionary<string, int>();
			teamkillMatrix = new List<TeamkillTuple>();
			teamkillTimers = new Dictionary<string, Timer>();
			teamkillScaled = new Dictionary<int, int>();*/
		}

		public override void Register()
		{
			// Register Events
			this.AddEventHandler(typeof(IEventHandlerRoundStart), new RoundStartHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerRoundEnd), new RoundEndHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerPlayerDie), new PlayerDieHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerPlayerHurt), new PlayerHurtHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerPlayerJoin), new PlayerJoinHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerDisconnect), new PlayerDisconnectHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerSpawn), new SpawnHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerSetRole), new SetRoleHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerPlayerPickupItemLate), new PlayerPickupItemLateHandler(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerCallCommand), new CallCommandHandler(this), Priority.Normal);

			// Register config settings
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_enable", true, Smod2.Config.SettingType.BOOL, true, "Enable Friendly Fire Autoban."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_outall", false, Smod2.Config.SettingType.BOOL, true, "Alterantive to sm_debug, which is just all config setting spam."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_system", 1, Smod2.Config.SettingType.NUMERIC, true, "Change system for processing teamkills: basic counter (1), timer-based counter (2), or end-of-round counter (3)."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_matrix", new string[] { "1:1", "2:2", "3:3", "4:4", "1:3", "2:4", "3:1", "4:2" }, Smod2.Config.SettingType.LIST, true, "Matrix of killer:victim tuples that are considered teamkills."));
			// 1
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_amount", 5, Smod2.Config.SettingType.NUMERIC, true, "Amount of teamkills before a ban will be issued."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_length", 1440, Smod2.Config.SettingType.NUMERIC, true, "Length of ban in minutes."));
			// 2
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_expire", 60, Smod2.Config.SettingType.NUMERIC, true, "Time it takes in seconds for teamkill to degrade and not count towards ban."));
			// 3
			/*
			 * 0 - kick
			 * 1 - 1 minute
			 * 5 - 5 minutes
			 * 15 - 15 minutes
			 * 30 - 30 minutes
			 * 60 - 1 hour
			 * 180 - 3 hours
			 * 300 - 5 hours
			 * 480 - 8 hours
			 * 720 - 12 hours
			 * 1440 - 1 day
			 * 4320 - 3 days
			 * 10080 - 1 week
			 * 20160 - 2 weeks
			 * 43200 - 1 month
			 * 129600 - 3 months
			 * 525600 - 1 year
			 * 26280000 - 50 years
			 */
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_scaled", new string[] { "4:1440", "5:4320", "6:4320", "7:10080", "8:10080", "9:43800", "10:43800", "11:129600", "12:129600", "13:525600" }, Smod2.Config.SettingType.LIST, true, "For ban system #3, dictionary of amount of teamkills:length of ban that will be processed at the end of the round."));

			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_noguns", 0, Smod2.Config.SettingType.NUMERIC, true, "Number of kills to remove the player's guns as a warning for teamkilling."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_tospec", 0, Smod2.Config.SettingType.NUMERIC, true, "Number of kills at which to put a player into spectator as a warning for teamkilling."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_kicker", 0, Smod2.Config.SettingType.NUMERIC, true, "Number of kills at which to kick as a warning for teamkilling."));

			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_bomber", 0, Smod2.Config.SettingType.NUMERIC, true, "Whether to delay grenade damage of thrower [experimental] (2), make player immune to grenade damage (1), or keep disabled (0)."));

			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_immune", new string[] { "owner", "admin", "moderator" }, Smod2.Config.SettingType.LIST, true, "Ranks that are immune to being autobanned."));

			this.AddCommand("friendly_fire_autoban_toggle", new ToggleCommand(this));
		}

		public bool isImmune(Player player)
		{
			string[] immuneRanks = this.GetConfigList("friendly_fire_autoban_immune");
			foreach (string rank in immuneRanks)
			{
				if (this.outall)
				{
					this.Info("Does immune rank " + rank + " equal " + player.GetUserGroup().Name + " or " + player.GetRankName() + "?");
				}
				if (String.Equals(rank, player.GetUserGroup().Name, StringComparison.CurrentCultureIgnoreCase) || String.Equals(rank, player.GetRankName(), StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public bool Ban(Player player, string playerName, int banLength, List<Teamkill> teamkills)
		{
			bool immune = isImmune(player);
			if (immune)
			{
				this.Info("Admin/Moderator " + playerName + " has avoided a ban for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				return false;
			}
			else
			{
				if (teamkills.Count > 3)
				{
					player.Ban(banLength, "Banned " + banLength + " minutes for teamkilling " + teamkills.Count + " players");
				}
				else
				{
					player.Ban(banLength, "Banned " + banLength + " minutes for teamkilling player(s) " + string.Join(", ", teamkills.Select(teamkill => teamkill.victimName).ToArray()));
				}
				this.Info("Player " + playerName + " has been banned for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				this.Server.Map.Broadcast(3, "Player " + playerName + " has been banned for teamkilling " + teamkills.Count + " players.", false);
				return true;
			}
		}

		public int GetScaledBanAmount(string steamId)
		{
			int banLength = 0;
			foreach (int banAmount in this.teamkillScaled.Keys.OrderBy(k => k))
			{
				if (this.outall) this.Info("Ban length set to " + banLength + ". Checking ban amount for key " + banAmount);
				// If ban kills is less than player's kills, set the banLength
				// This will ensure that players who teamkill more than the maximum
				// will still serve the maximum ban length
				if (banAmount < this.teamkillCounter[steamId].Count)
				{
					if (this.outall) this.Info("Ban amount is less than player teamkills.");
					banLength = this.teamkillScaled[banAmount];
				}
				// Exact ban amount match is found, set
				else if (banAmount == this.teamkillCounter[steamId].Count)
				{
					if (this.outall) this.Info("Ban amount is equal to player teamkills.");
					banLength = this.teamkillScaled[banAmount];
					break;
				}
				// If the smallest ban amount is larger than the player's bans,
				// then the player will not be banned.
				// If banAmount has not been found, it will still be set to 0
				else if (banAmount > this.teamkillCounter[steamId].Count)
				{
					if (this.outall) this.Info("Ban amount is greater than player teamkills.");
					break;
				}
			}
			return banLength;
		}

		public bool CheckRemoveGuns(Player killer)
		{
			if (this.noguns > 0 && this.teamkillCounter.ContainsKey(killer.SteamId) && this.teamkillCounter[killer.SteamId].Count >= this.noguns && !this.isImmune(killer))
			{
				this.Info("Player " + killer.Name + " " + killer.SteamId + " " + killer.IpAddress + " has had his/her guns removed for teamkilling.");
				List<Item> inv = killer.GetInventory();
				for (int i = 0; i < inv.Count; i++)
				{
					switch (inv[i].ItemType)
					{
						case ItemType.COM15:
						case ItemType.E11_STANDARD_RIFLE:
						case ItemType.LOGICER:
						case ItemType.MICROHID:
						case ItemType.MP4:
						case ItemType.P90:
						case ItemType.FRAG_GRENADE:
						case ItemType.FLASHBANG:
							inv[i].Remove();
							break;
					}
				}
				killer.PersonalBroadcast(5, "Your guns have been removed for teamkilling. You will get them back when your teamkill expires.", false);
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	class ToggleCommand : ICommandHandler
	{
		private readonly FriendlyFireAutobanPlugin plugin;

		public ToggleCommand(FriendlyFireAutobanPlugin plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Toggle Friendly Fire Autoban on and off.";
		}

		public string GetUsage()
		{
			return "FRIENDLY_FIRE_AUTOBAN_TOGGLE";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			Player caller = sender as Player;

			if (this.plugin.enable)
			{
				this.plugin.enable = false;
				return new string[] { "Friendly fire autoban has been disabled." };
			}
			else
			{
				this.plugin.enable = true;
				return new string[] { "Friendly fire autoban has been enabled." };
			}

		}
	}

	class Teamkill
	{
		public string killerName;
		public string killerSteamId;
		public TeamRole killerTeamRole;
		public string victimName;
		public string victimSteamId;
		public TeamRole victimTeamRole;
		public DamageType damageType;

		public Teamkill(string killerName, string killerSteamId, TeamRole killerTeamRole, string victimName, string victimSteamId, TeamRole victimTeamRole, DamageType damageType)
		{
			this.killerName = killerName;
			this.killerSteamId = killerSteamId;
			this.killerTeamRole = killerTeamRole;
			this.victimName = victimName;
			this.victimSteamId = victimSteamId;
			this.victimTeamRole = victimTeamRole;
			this.damageType = damageType;
		}

		public string getRoleDisplay()
		{
			string retval = "(";
			switch (killerTeamRole.Role)
			{
				case Role.CLASSD:
					retval += "DCLASS";
					break;

				case Role.SCIENTIST:
					retval += "SCIENTIST";
					break;

				case Role.FACILITY_GUARD:
					retval += "GUARD";
					break;

				case Role.NTF_CADET:
					retval += "CADET";
					break;

				case Role.NTF_LIEUTENANT:
					retval += "NTF LT";
					break;

				case Role.NTF_COMMANDER:
					retval += "NTF CDR";
					break;

				case Role.NTF_SCIENTIST:
					retval += "NTF SCT";
					break;

				case Role.CHAOS_INSURGENCY:
					retval += "CI";
					break;

				case Role.TUTORIAL:
					retval += "TUT";
					break;
			}
			retval += "x";
			switch (victimTeamRole.Role)
			{
				case Role.CLASSD:
					retval += "DCLASS";
					break;

				case Role.SCIENTIST:
					retval += "SCIENTIST";
					break;

				case Role.FACILITY_GUARD:
					retval += "GUARD";
					break;

				case Role.NTF_CADET:
					retval += "CADET";
					break;

				case Role.NTF_LIEUTENANT:
					retval += "NTF LT";
					break;

				case Role.NTF_COMMANDER:
					retval += "NTF CDR";
					break;

				case Role.NTF_SCIENTIST:
					retval += "NTF SCT";
					break;

				case Role.CHAOS_INSURGENCY:
					retval += "CI";
					break;

				case Role.TUTORIAL:
					retval += "TUT";
					break;
			}
			retval += ")";
			return retval;
		}

		public override bool Equals(object obj)
		{
			var teamkill = obj as Teamkill;
			return teamkill != null &&
				killerName == teamkill.killerName &&
				killerSteamId == teamkill.killerSteamId &&
				EqualityComparer<TeamRole>.Default.Equals(killerTeamRole, teamkill.killerTeamRole) &&
				victimName == teamkill.victimName &&
				victimSteamId == teamkill.victimSteamId &&
				EqualityComparer<TeamRole>.Default.Equals(victimTeamRole, teamkill.victimTeamRole) &&
				damageType == teamkill.damageType;
		}

		public override int GetHashCode()
		{
			var hashCode = 1729039167;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(killerName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(killerSteamId);
			hashCode = hashCode * -1521134295 + EqualityComparer<TeamRole>.Default.GetHashCode(killerTeamRole);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(victimName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(victimSteamId);
			hashCode = hashCode * -1521134295 + EqualityComparer<TeamRole>.Default.GetHashCode(victimTeamRole);
			hashCode = hashCode * -1521134295 + damageType.GetHashCode();
			return hashCode;
		}
	}

	struct TeamkillTuple
	{
		public int killerRole, victimRole;

		public TeamkillTuple(int killerRole, int victimRole)
		{
			this.killerRole = killerRole;
			this.victimRole = victimRole;
		}
	}

	/*struct ScaledTuple
	{
		public int teamkillAmount, banLength;

		public ScaledTuple(int teamkillAmount, int banLength)
		{
			this.teamkillAmount = teamkillAmount;
			this.banLength = banLength;
		}
	}*/
}
