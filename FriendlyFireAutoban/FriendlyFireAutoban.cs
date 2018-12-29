using FriendlyFireAutoban.EventHandlers;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;
using System.Timers;
using System;

namespace FriendlyFireAutoban
{
	[PluginDetails(
		author = "PatPeter",
		name = "Friendly Fire Autoban",
		description = "Plugin that autobans players for friendly firing.",
		id = "patpeter.friendly.fire.autoban",
		version = "2.2.1.34",
		SmodMajor = 3,
		SmodMinor = 1,
		SmodRevision = 20
		)]
	class FriendlyFireAutobanPlugin : Plugin
	{
		internal bool duringRound = false;
		internal bool enabled = true;
		internal bool debug = false;
		internal Dictionary<string, int> teamkillCounter = new Dictionary<string, int>();
		internal List<TeamkillTuple> teamkillMatrix = new List<TeamkillTuple>();
		internal Dictionary<string, Timer> teamkillTimers = new Dictionary<string, Timer>();
		internal Dictionary<int, int> teamkillScaled = new Dictionary<int, int>();

		public override void OnEnable()
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

		public override void OnDisable()
		{

		}

		public override void Register()
		{
			// Register Events
			this.AddEventHandler(typeof(IEventHandlerRoundStart), new RoundStartHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventHandlerRoundEnd), new RoundEndHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventHandlerPlayerDie), new PlayerDieHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventHandlerPlayerHurt), new PlayerHurtHandler(this), Priority.Highest);

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
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_scaled", new string[] { "1:0", "2:1", "3:5", "4:15", "5:30", "6:60", "7:180", "8:300", "9:480", "10:720", "11:1440", "12:4320", "13:10080", "14:20160", "15:43200", "16:43200", "17:14400", "18:525600", "19:2628000", "20:26280000" }, Smod2.Config.SettingType.LIST, true, "For ban system #3, dictionary of amount of teamkills:length of ban that will be processed at the end of the round."));

			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_noguns", 0, Smod2.Config.SettingType.NUMERIC, true, "Number of kills to remove the player's guns as a warning for teamkilling."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_tospec", 0, Smod2.Config.SettingType.NUMERIC, true, "Number of kills at which to put a player into spectator as a warning for teamkilling."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_kicker", 0, Smod2.Config.SettingType.NUMERIC, true, "Number of kills at which to kick as a warning for teamkilling."));

			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_immune", new string[] { "owner", "admin", "moderator" }, Smod2.Config.SettingType.LIST, true, "Ranks that are immune to being autobanned."));
		}

		public bool isImmune(Player player)
		{
			string[] immuneRanks = this.GetConfigList("friendly_fire_autoban_immune");
			foreach (string rank in immuneRanks)
			{
				if (this.debug)
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

		public bool Ban(Player player, string playerName, int banLength, int teamkills)
		{
			bool immune = isImmune(player);
			if (immune)
			{
				this.Info("Admin/Moderator " + playerName + " has avoided a ban for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				return false;
			}
			else
			{
				player.Ban(banLength);
				this.Info("Player " + playerName + " has been banned for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				return true;
			}
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
