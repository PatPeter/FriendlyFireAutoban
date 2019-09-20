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
using Smod2.Lang;
using Smod2.Piping;

namespace FriendlyFireAutoban
{
	[PluginDetails(
		author = AssemblyInfo.Author,
		name = AssemblyInfo.Name,
		description = AssemblyInfo.Description,
		id = AssemblyInfo.Id,
		configPrefix = AssemblyInfo.ConfigPrefix,
		langFile = AssemblyInfo.LangFile,
		version = AssemblyInfo.Version,
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 0
		)]
	class FriendlyFireAutobanPlugin : Plugin
	{
		private static FriendlyFireAutobanPlugin instance = null;

		public static FriendlyFireAutobanPlugin GetInstance()
		{
			return instance;
		}

		internal bool DuringRound = false;
		internal bool ProcessingDisconnect = false;

		/*
		 * Config variables
		 * 
		 * Keep lowercase six-character naming scheme from config
		 */
		internal bool enable = true;
		internal bool outall = false;
		internal int system = 1;
		internal List<TeamTuple> matrix = new List<TeamTuple>();
		internal int amount = 5;
		internal int length = 1440;
		internal int expire = 60;
		internal Dictionary<int, int> scaled = new Dictionary<int, int>();
		internal int noguns = 0;
		internal int tospec = 0;
		internal int kicker = 0;
		internal int bomber = 0;
		internal bool disarm = false;
		internal List<RoleTuple> rolewl = new List<RoleTuple>();
		internal float mirror = 0;
		internal int warntk = 0;
		internal int votetk = 0;

		internal Dictionary<string, Teamkiller> Teamkillers = new Dictionary<string, Teamkiller>();
		internal Dictionary<string, Timer> TeamkillTimers = new Dictionary<string, Timer>();
		internal Dictionary<string, Teamkill> TeamkillVictims = new Dictionary<string, Teamkill>();

		internal Dictionary<int, int> InverseTeams = new Dictionary<int, int>()
		{
			{ -1, -1 },
			{ 0, 0 },
			{ 1, 2 },
			{ 2, 1 },
			{ 3, 4 },
			{ 4, 3 },
			{ 5, 5 },
			{ 6, 6 }
		};
		internal Dictionary<int, int> InverseRoles = new Dictionary<int, int>()
		{
			{ -1, -1 },
			{ 0, 0 },
			{ 1, 6 },
			{ 2, 2 },
			{ 3, 3 },
			{ 4, 8 },
			{ 5, 5 },
			{ 6, 1 },
			{ 7, 7 },
			{ 8, 12 },
			{ 9, 9 },
			{ 10, 10 },
			{ 11, 8 },
			{ 12, 8 },
			{ 13, 8 },
			{ 14, 14 },
			{ 15, 8 },
			{ 16, 17 },
			{ 17, 16 }
		};

		[PipeLink("patpeter.callvote", "Voting")]
		private readonly MethodPipe<bool> Voting;

		[PipeLink("patpeter.callvote", "StartVote")]
		private readonly MethodPipe<bool> StartVote;

		/*
		 * Ban Events
		 */
		[LangOption]
		public readonly string victimMessage = "{0} teamkilled you. If this was an accidental teamkill, please press ~ and then type .forgive to prevent this user from being banned.";
		[LangOption]
		public readonly string killerMessage = "You teamkilled {0} {1}.";
		[LangOption]
		public readonly string killerWarning = "If you teamkill {0} more times you will be banned.";
		[LangOption]
		public readonly string killerRequest = "Please do not teamkill.";
		[LangOption]
		public readonly string nogunsOutput = "Your guns have been removed for teamkilling. You will get them back when your teamkill expires.";
		[LangOption]
		public readonly string tospecOutput = "You have been moved to spectate for teamkilling.";
		[LangOption]
		public readonly string kickerOutput = "You will be kicked for teamkilling.";
		[LangOption]
		public readonly string bannedOutput = "Player {0} has been banned for teamkilling {1} players.";
		[LangOption]
		public readonly string offlineBan = "Banned {0} minutes for teamkilling {1} players";

		/*
		 * Teamkiller/Teamkill
		 */
		[LangOption]
		public readonly string roleDisarmed = "DISARMED ";
		[LangOption]
		public readonly string roleSeparator = "on";
		[LangOption]
		public readonly string roleDclass = "D-CLASS";
		[LangOption]
		public readonly string roleScientist = "SCIENTIST";
		[LangOption]
		public readonly string roleGuard = "GUARD";
		[LangOption]
		public readonly string roleCadet = "CADET";
		[LangOption]
		public readonly string roleLieutenant = "LIEUTENANT";
		[LangOption]
		public readonly string roleCommander = "COMMANDER";
		[LangOption("role_ntf_scientist")]
		public readonly string roleNTFScientist = "NTF SCIENTIST";
		[LangOption]
		public readonly string roleChaos = "CHAOS";
		[LangOption]
		public readonly string roleTutorial = "TUTORIAL";

		/*
		 * Commands
		 */
		[LangOption]
		public readonly string toggleDescription = "Toggle Friendly Fire Autoban on and off.";
		[LangOption]
		public readonly string toggleDisable = "Friendly fire Autoban has been disabled.";
		[LangOption]
		public readonly string toggleEnable = "Friendly fire Autoban has been enabled.";

		/*
		 * Client Commands
		 */
		[LangOption]
		public readonly string forgiveCommand = "forgive";
		[LangOption]
		public readonly string forgiveSuccess = "You have forgiven {0} {1}!";
		[LangOption]
		public readonly string forgiveDuplicate = "You already forgave {0} {1}.";
		[LangOption]
		public readonly string forgiveDisconnect = "The player has disconnected.";
		[LangOption]
		public readonly string forgiveInvalid = "You have not been teamkilled yet.";

		[LangOption]
		public readonly string tksCommand = "tks";
		[LangOption]
		public readonly string tksNoTeamkills = "No players by this name has any teamkills.";
		[LangOption]
		public readonly string tksTeamkillEntry = "({0}) {1} teamkilled {2} {3}.";
		[LangOption]
		public readonly string tksNotFound = "Player name not provided or not quoted.";

		[LangOption]
		public readonly string ffaDisabled = "Friendly Fire Autoban is currently disabled.";

		/*
		[LangOption]
		public readonly string KEY = VALUE;
		*/

		public override void OnEnable()
		{
			FriendlyFireAutobanPlugin.instance = this;
			if (this.outall)
			{
				this.PrintConfigs();
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
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_enable", true, true, "Enable Friendly Fire Autoban."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_outall", false, true, "Alterantive to sm_debug, which is just all config setting spam."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_system", 1, true, "Change system for processing teamkills: basic counter (1), timer-based counter (2), or end-of-round counter (3)."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_matrix", new string[] { "1:1", "2:2", "3:3", "4:4", "1:3", "2:4", "3:1", "4:2" }, true, "Matrix of killer:victim tuples that are considered teamkills."));
			// 1
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_amount", 5, true, "Amount of teamkills before a ban will be issued."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_length", 1440, true, "Length of ban in minutes."));
			// 2
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_expire", 60, true, "Time it takes in seconds for teamkill to degrade and not count towards ban."));
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
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_scaled", new string[] { "4:1440", "5:4320", "6:4320", "7:10080", "8:10080", "9:43800", "10:43800", "11:129600", "12:129600", "13:525600" }, true, "For ban system #3, dictionary of amount of teamkills:length of ban that will be processed at the end of the round."));

			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_noguns", 0, true, "Number of kills to remove the player's guns as a warning for teamkilling."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_tospec", 0, true, "Number of kills at which to put a player into spectator as a warning for teamkilling."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_kicker", 0, true, "Number of kills at which to kick as a warning for teamkilling."));

			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_bomber", 0, true, "Whether to delay grenade damage of thrower [experimental] (2), make player immune to grenade damage (1), or keep disabled (0)."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_disarm", false, true, "If teamkilling disarmed players should count as a teamkill set to true."));
			// NTF roleplay settings:
			// 12:11,12:4,12:13,12:15,4:11,4:13,4:15,11:13,11:15,13:15
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_rolewl", new string[] {  }, true, "Whitelist of roles that are allowed to kill each other."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_mirror", 0, true, "Mirror friendly fire damage to the person causing the damage. Increasing past 1 will increase the multiplayer."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_warntk", 0, true, "The number of TKs to warn for before banning, (0) for a generic warning after every TK, and (-1) for no warning."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_votetk", 0, true, "Number of TKs at which to trigger a vote via the callvote plugin."));

			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_immune", new string[] { "owner", "admin", "moderator" }, true, "Ranks that are immune to being autobanned."));

			this.AddCommand("friendly_fire_autoban_toggle", new ToggleCommand(this));
			this.AddCommand("ffa_toggle", new ToggleCommand(this));
		}

		public void PrintConfigs()
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
			this.Info("friendly_fire_autoban_bomber default value: " + this.GetConfigInt("friendly_fire_autoban_bomber"));
			this.Info("friendly_fire_autoban_disarm default value: " + this.GetConfigBool("friendly_fire_autoban_disarm"));
			this.Info("friendly_fire_autoban_rolewl default value: " + this.GetConfigList("friendly_fire_autoban_rolewl"));
			this.Info("friendly_fire_autoban_mirror default value: " + this.GetConfigInt("friendly_fire_autoban_mirror"));
			this.Info("friendly_fire_autoban_warntk default value: " + this.GetConfigInt("friendly_fire_autoban_warntk"));
			this.Info("friendly_fire_autoban_votetk default value: " + this.GetConfigInt("friendly_fire_autoban_votetk"));

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

		public bool isTeamkill(Player killer, Player victim)
		{
			int killerTeam = (int)killer.TeamRole.Team;
			int victimTeam = (int)victim.TeamRole.Team;
			int killerRole = (int)killer.TeamRole.Role;
			int victimRole = (int)victim.TeamRole.Role;

			if (String.Equals(killer.SteamId, victim.SteamId))
			{
				return false;
			}

			if (this.disarm && victim.IsHandcuffed())
			{
				victimTeam = this.InverseTeams[victimTeam];
				victimRole = this.InverseRoles[victimRole];
			}

			foreach (RoleTuple roleTuple in this.rolewl)
			{
				if (killerRole == roleTuple.KillerRole && victimRole == roleTuple.VictimRole)
				{
					return false;
				}
			}

			foreach (TeamTuple teamTuple in this.matrix)
			{
				if (killerTeam == teamTuple.KillerTeam && victimTeam == teamTuple.VictimTeam)
				{
					return true;
				}
			}

			return false;
		}

		public int GetScaledBanAmount(string steamId)
		{
			int banLength = 0;
			foreach (int banAmount in this.scaled.Keys.OrderBy(k => k))
			{
				if (this.outall) this.Info("Ban length set to " + banLength + ". Checking ban amount for key " + banAmount);
				// If ban kills is less than player's kills, set the banLength
				// This will ensure that players who teamkill more than the maximum
				// will still serve the maximum ban length
				if (banAmount < this.Teamkillers[steamId].Teamkills.Count)
				{
					if (this.outall) this.Info("Ban amount is less than player teamkills.");
					banLength = this.scaled[banAmount];
				}
				// Exact ban amount match is found, set
				else if (banAmount == this.Teamkillers[steamId].Teamkills.Count)
				{
					if (this.outall) this.Info("Ban amount is equal to player teamkills.");
					banLength = this.scaled[banAmount];
					break;
				}
				// If the smallest ban amount is larger than the player's bans,
				// then the player will not be banned.
				// If banAmount has not been found, it will still be set to 0
				else if (banAmount > this.Teamkillers[steamId].Teamkills.Count)
				{
					if (this.outall) this.Info("Ban amount is greater than player teamkills.");
					break;
				}
			}
			return banLength;
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnBan")]
		[PipeMethod]
		public bool OnBan(Player player, string playerName, int banLength, List<Teamkill> teamkills)
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
					player.Ban(banLength, "Banned " + banLength + " minutes for teamkilling player(s) " + string.Join(", ", teamkills.Select(teamkill => teamkill.VictimName).ToArray()));
				}
				this.Info("Player " + playerName + " has been banned for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				this.Server.Map.Broadcast(3, string.Format(this.GetTranslation("banned_output"), playerName, teamkills.Count), false);
				return true;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckRemoveGuns")]
		[PipeMethod]
		public bool OnCheckRemoveGuns(Player killer)
		{
			if (this.noguns > 0 && this.Teamkillers.ContainsKey(killer.SteamId) && this.Teamkillers[killer.SteamId].Teamkills.Count >= this.noguns && !this.isImmune(killer))
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
				killer.PersonalBroadcast(2, this.GetTranslation("noguns_output"), false);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckToSpectator")]
		[PipeMethod]
		public bool OnCheckToSpectator(Player killer)
		{
			if (this.tospec > 0 && this.Teamkillers[killer.SteamId].Teamkills.Count >= this.tospec && !this.isImmune(killer))
			{
				this.Info("Player " + killer.Name + " " + killer.SteamId + " " + killer.IpAddress + " has been moved to spectator for teamkilling " + this.Teamkillers[killer.SteamId].Teamkills.Count + " times.");
				killer.PersonalBroadcast(5, this.GetTranslation("tospec_output"), false);
				killer.ChangeRole(Role.SPECTATOR);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckKick")]
		[PipeMethod]
		public bool OnCheckKick(Player killer)
		{
			if (this.kicker > 0 && this.Teamkillers[killer.SteamId].Teamkills.Count == this.kicker && !this.isImmune(killer))
			{
				this.Info("Player " + killer.Name + " " + killer.SteamId + " " + killer.IpAddress + " has been kicked for teamkilling " + this.Teamkillers[killer.SteamId].Teamkills.Count + " times.");
				killer.PersonalBroadcast(1, this.GetTranslation("kicker_output"), false);
				killer.Ban(0);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckVote")]
		[PipeMethod]
		public bool OnVoteTeamkill(Player killer)
		{
			if (this.votetk > 0 && this.Teamkillers[killer.SteamId].Teamkills.Count >= this.votetk && !this.isImmune(killer))
			{
				this.Info("Player " + killer.Name + " " + killer.SteamId + " " + killer.IpAddress + " is being voted on a ban for teamkilling " + this.Teamkillers[killer.SteamId].Teamkills.Count + " times.");
				Dictionary<int, string> options = new Dictionary<int, string>();
				options[1] = "Yes";
				options[2] = "No";
				HashSet<string> votes = new HashSet<string>();
				Dictionary<int, int> counter = new Dictionary<int, int>();

				if (Voting.Invoke())
				{
					return StartVote.Invoke("Ban " + killer.Name + "?", options, votes, counter);
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}

	struct TeamTuple
	{
		public int KillerTeam, VictimTeam;

		public TeamTuple(int killerTeam, int victimRole)
		{
			this.KillerTeam = killerTeam;
			this.VictimTeam = victimRole;
		}
	}

	struct RoleTuple
	{
		public int KillerRole, VictimRole;

		public RoleTuple(int killerRole, int victimRole)
		{
			this.KillerRole = killerRole;
			this.VictimRole = victimRole;
		}
	}
}
