using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using EXILED;
using EXILED.Extensions;
using MEC;
using static DamageTypes;
using static Inventory;

namespace FriendlyFireAutoban
{
	public class Plugin : EXILED.Plugin
	{
		private static Plugin instance = null;
		private Plugin plugin = null;

		public static Plugin GetInstance()
		{
			return instance;
		}

		internal bool DuringRound = false;
		internal bool ProcessingDisconnect = false;

		/*
		 * 
		 */
		public bool enable = true;
		public bool outall = false;
		public int system = 1;
		public List<TeamTuple> matrix = new List<TeamTuple>()
		{
			new TeamTuple(0, 0),
			new TeamTuple(1, 1),
			new TeamTuple(2, 2),
			new TeamTuple(3, 3),
			new TeamTuple(4, 4),
			new TeamTuple(1, 3),
			new TeamTuple(2, 4),
			new TeamTuple(3, 1),
			new TeamTuple(4, 2),
		};
		public int amount = 5;
		public int length = 1440;
		public int expire = 60;
		public Dictionary<int, int> scaled = new Dictionary<int, int>()
		{
			{ 4, 1440 },
			{ 5, 4320 },
			{ 6, 4320 },
			{ 7, 10080 },
			{ 8, 10080 },
			{ 9, 43800 },
			{ 10, 43800 },
			{ 11, 129600 },
			{ 12, 129600 },
			{ 13, 525600 }
		};
		public int noguns = 0;
		public int tospec = 0;
		public int kicker = 0;
		public HashSet<string> immune = new HashSet<string>()
		{
			"owner",
			"admin",
			"moderator"
		};
		public int bomber = 0;
		public bool disarm = false;
		public List<RoleTuple> rolewl = new List<RoleTuple>();
		public int invert = 0;
		public float mirror = 0;
		public int undead = 0;
		public int warntk = 0;
		public int votetk = 0;
		public int kdsafe = 0;

		internal Dictionary<string, Teamkiller> Teamkillers = new Dictionary<string, Teamkiller>();
		internal Dictionary<string, Timer> TeamkillTimers = new Dictionary<string, Timer>();
		internal Dictionary<string, Teamkill> TeamkillVictims = new Dictionary<string, Teamkill>();

		internal HashSet<string> banWhitelist = new HashSet<string>();

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

		//Instance variable for eventhandlers
		public EventHandlers EventHandlers;

		/*
		 * Ban Events
		 */
		
		public readonly string victimMessage = "<size=36>{0} <color=red>teamkilled</color> you at {1}. If this was an accidental teamkill, please press ~ and then type .forgive to prevent this user from being banned.</size>";
		
		public readonly string killerMessage = "You teamkilled {0} {1}.";
		
		public readonly string killerKDRMessage = "You teamkilled {0} {1}. Because your K/D ratio is {2}, you will not be punished. Please watch your fire.";
		
		public readonly string killerWarning = "If you teamkill {0} more times you will be banned.";
		
		public readonly string killerRequest = "Please do not teamkill.";
		
		public readonly string nogunsOutput = "Your guns have been removed for <color=red>teamkilling</color>. You will get them back when your teamkill expires.";
		
		public readonly string tospecOutput = "You have been moved to spectate for <color=red>teamkilling</color>.";
		
		public readonly string undeadKillerOutput = "{0} has been respawned because you are <color=red>teamkilling</color> too much. If you continue, you will be banned.";
		
		public readonly string undeadVictimOutput = "You have been respawned after being teamkilled by {0}.";
		
		public readonly string kickerOutput = "You will be kicked for <color=red>teamkilling</color>.";
		
		public readonly string bannedOutput = "Player {0} has been banned for <color=red>teamkilling</color> {1} players.";

		
		// OFFLINE BAN, DO NOT ADD BBCODE
		public readonly string offlineBan = "Banned {0} minutes for teamkilling {1} players";

		/*
		 * Teamkiller/Teamkill
		 */
		
		public readonly string roleDisarmed = "DISARMED ";
		
		public readonly string roleSeparator = "on";
		
		public readonly string roleDclass = "<color=orange>D-CLASS</color>";
		
		public readonly string roleScientist = "<color=yellow>SCIENTIST</color>";
		
		public readonly string roleGuard = "<color=silver>GUARD</color>";
		
		public readonly string roleCadet = "<color=cyan>CADET</color>";
		
		public readonly string roleLieutenant = "<color=aqua>LIEUTENANT</color>";
		
		public readonly string roleCommander = "<color=blue>COMMANDER</color>";
		
		public readonly string roleNTFScientist = "<color=aqua>NTF SCIENTIST</color>";
		
		public readonly string roleChaos = "<color=green>CHAOS</color>";
		
		public readonly string roleTutorial = "<color=lime>TUTORIAL</color>";

		/*
		 * Commands
		 */
		
		public readonly string toggleDescription = "Toggle Friendly Fire Autoban on and off.";
		
		public readonly string toggleDisable = "Friendly fire Autoban has been disabled.";
		
		public readonly string toggleEnable = "Friendly fire Autoban has been enabled.";

		
		public readonly string whitelistDescription = "Whitelist a user from being banned by FFA until the end of the round.";
		
		public readonly string whitelistError = "A single name or Steam ID must be provided.";
		
		public readonly string whitelistAdd = "Added player {0} ({1}) to ban whitelist.";
		
		public readonly string whitelistRemove = "Removed player {0} ({1}) from ban whitelist.";

		/*
		 * Client Commands
		 */
		
		public readonly string forgiveCommand = "forgive";
		
		public readonly string forgiveSuccess = "You have forgiven {0} {1}!";
		
		public readonly string forgiveDuplicate = "You already forgave {0} {1}.";
		
		public readonly string forgiveDisconnect = "The player has disconnected.";
		
		public readonly string forgiveInvalid = "You have not been teamkilled yet.";

		
		public readonly string tksCommand = "tks";
		
		public readonly string tksNoTeamkills = "No players by this name or Steam ID has any teamkills.";
		
		public readonly string tksTeamkillEntry = "({0}) {1} teamkilled {2} {3}.";
		
		public readonly string tksNotFound = "Player name not provided or not quoted.";

		
		public readonly string ffaDisabled = "Friendly Fire Autoban is currently disabled.";

		public string GetTranslation(string name)
		{
			Type t = this.GetType();
			PropertyInfo property = t.GetProperty(name);
			if (property != null)
			{
				return (string) property.GetValue(this);
			}
			else
			{
				return "INVALID TRANSLATION: " + name;
			}
		}

		public override string getName { get; } = "Friendly Fire Autoban";

		public override void OnEnable()
		{
			ReloadConfig();

			if (!enable)
				return;

			Plugin.instance = this;
			this.plugin = this;

			try
			{
				Log.Debug("Initializing event handlers..");
				//Set instance varible to a new instance, this should be nulled again in OnDisable
				EventHandlers = new EventHandlers(this);
				//Hook the events you will be using in the plugin. You should hook all events you will be using here, all events should be unhooked in OnDisabled 
				Events.RoundStartEvent += EventHandlers.OnRoundStart;
				Events.RoundEndEvent += EventHandlers.OnRoundEnd;

				Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
				Events.PlayerLeaveEvent += EventHandlers.OnPlayerLeave;

				Events.PlayerDeathEvent += EventHandlers.OnPlayerDeath;
				Events.PlayerHurtEvent += EventHandlers.OnPlayerHurt;

				Events.PlayerSpawnEvent += EventHandlers.OnPlayerSpawn;
				Events.SetClassEvent += EventHandlers.OnSetClass;
				Events.PickupItemEvent += EventHandlers.OnPickupItem;

				Events.RemoteAdminCommandEvent += EventHandlers.OnRACommand;
				Events.ConsoleCommandEvent += EventHandlers.OnConsoleCommand;

				Log.Info($"Friendly Fire Autoban has been loaded!");
			}
			catch (Exception e)
			{
				//This try catch is redundant, as EXILED will throw an error before this block can, but is here as an example of how to handle exceptions/errors
				Log.Error($"There was an error loading the plugin: {e}");
			}
		}

		public override void OnDisable()
		{
			Events.RoundStartEvent -= EventHandlers.OnRoundStart;
			Events.RoundEndEvent -= EventHandlers.OnRoundEnd;

			Events.PlayerJoinEvent -= EventHandlers.OnPlayerJoin;
			Events.PlayerLeaveEvent -= EventHandlers.OnPlayerLeave;

			Events.PlayerDeathEvent -= EventHandlers.OnPlayerDeath;
			Events.PlayerHurtEvent -= EventHandlers.OnPlayerHurt;

			Events.PlayerSpawnEvent -= EventHandlers.OnPlayerSpawn;
			Events.SetClassEvent -= EventHandlers.OnSetClass;
			Events.PickupItemEvent -= EventHandlers.OnPickupItem;

			Events.RemoteAdminCommandEvent -= EventHandlers.OnRACommand;
			Events.ConsoleCommandEvent -= EventHandlers.OnConsoleCommand;

			EventHandlers = null;
		}

		public override void OnReload()
		{
			//This is only fired when you use the EXILED reload command, the reload command will call OnDisable, OnReload, reload the plugin, then OnEnable in that order. There is no GAC bypass, so if you are updating a plugin, it must have a unique assembly name, and you need to remove the old version from the plugins folder
		}

		public void ReloadConfig()
		{
			this.enable = Config.GetBool("friendly_fire_autoban_enable", true);
			this.outall = Config.GetBool("friendly_fire_autoban_outall", false);
			this.system = Config.GetInt("friendly_fire_autoban_system", 1);

			this.matrix = new List<TeamTuple>();
			List<string> teamkillMatrix = Config.GetStringList("friendly_fire_autoban_matrix");
			foreach (string pair in teamkillMatrix)
			{
				string[] tuple = pair.Split(':');
				if (tuple.Length != 2)
				{
					if (this.outall)
					{
						Log.Info("Tuple " + pair + " does not have a single : in it.");
					}
					continue;
				}
				int tuple0 = -1, tuple1 = -1;
				if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
				{
					if (this.outall)
					{
						Log.Info("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
					}
					continue;
				}

				this.matrix.Add(new TeamTuple(tuple0, tuple1));
			}

			this.amount = Config.GetInt("friendly_fire_autoban_amount");
			this.length = Config.GetInt("friendly_fire_autoban_length");
			this.expire = Config.GetInt("friendly_fire_autoban_expire");

			this.scaled = new Dictionary<int, int>();
			List<string> teamkillScaled = Config.GetStringList("friendly_fire_autoban_scaled");
			foreach (string pair in teamkillScaled)
			{
				string[] tuple = pair.Split(':');
				if (tuple.Length != 2)
				{
					if (this.outall)
					{
						Log.Info("Tuple " + pair + " does not have a single : in it.");
					}
					continue;
				}
				int tuple0 = -1, tuple1 = -1;
				if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
				{
					if (this.outall)
					{
						Log.Info("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
					}
					continue;
				}

				if (!this.scaled.ContainsKey(tuple0))
				{
					this.scaled[tuple0] = tuple1;
				}
			}

			this.noguns = Config.GetInt("friendly_fire_autoban_noguns");
			this.tospec = Config.GetInt("friendly_fire_autoban_tospec");
			this.kicker = Config.GetInt("friendly_fire_autoban_kicker");

			this.immune = new HashSet<string>();
			foreach (string rank in Config.GetStringList("friendly_fire_autoban_immune"))
			{
				this.immune.Add(rank);
			}

			this.bomber = Config.GetInt("friendly_fire_autoban_bomber");
			this.disarm = Config.GetBool("friendly_fire_autoban_disarm");

			this.rolewl = new List<RoleTuple>();
			List<String> roleWhitelist = Config.GetStringList("friendly_fire_autoban_rolewl");
			foreach (string pair in roleWhitelist)
			{
				string[] tuple = pair.Split(':');
				if (tuple.Length != 2)
				{
					if (this.outall)
					{
						Log.Info("Tuple " + pair + " does not have a single : in it.");
					}
					continue;
				}
				int tuple0 = -1, tuple1 = -1;
				if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
				{
					if (this.outall)
					{
						Log.Info("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
					}
					continue;
				}

				this.rolewl.Add(new RoleTuple(tuple0, tuple1));
			}

			this.invert = Config.GetInt("friendly_fire_autoban_invert");
			this.mirror = Config.GetFloat("friendly_fire_autoban_mirror");
			this.undead = Config.GetInt("friendly_fire_autoban_undead");
			this.warntk = Config.GetInt("friendly_fire_autoban_warntk");
			this.votetk = Config.GetInt("friendly_fire_autoban_votetk");
			this.kdsafe = Config.GetInt("friendly_fire_autoban_kdsafe");

			this.banWhitelist = new HashSet<string>();

			// Add back if we want to keep track of which teamkills are removed
			//foreach (Timer timer in this.teamkillTimers.Values)
			//{
			//	timer.Enabled = true;
			//}

			if (this.outall)
			{
				this.PrintConfigs();
			}
		}

		public void PrintConfigs()
		{
			Log.Info("friendly_fire_autoban_enable default value: " + Config.GetBool("friendly_fire_autoban_enable"));
			Log.Info("friendly_fire_autoban_system default value: " + Config.GetInt("friendly_fire_autoban_system"));
			string matrix = "";
			foreach (string s in Config.GetStringList("friendly_fire_autoban_matrix"))
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
			Log.Info("friendly_fire_autoban_matrix default value: " + matrix);
			Log.Info("friendly_fire_autoban_amount default value: " + Config.GetInt("friendly_fire_autoban_amount"));
			Log.Info("friendly_fire_autoban_length default value: " + Config.GetInt("friendly_fire_autoban_length"));
			Log.Info("friendly_fire_autoban_expire default value: " + Config.GetInt("friendly_fire_autoban_expire"));
			string scaled = "";
			foreach (string s in Config.GetStringList("friendly_fire_autoban_scaled"))
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
			Log.Info("friendly_fire_autoban_scaled default value: " + scaled);
			Log.Info("friendly_fire_autoban_noguns default value: " + Config.GetInt("friendly_fire_autoban_noguns"));
			Log.Info("friendly_fire_autoban_tospec default value: " + Config.GetInt("friendly_fire_autoban_tospec"));
			Log.Info("friendly_fire_autoban_kicker default value: " + Config.GetInt("friendly_fire_autoban_kicker"));
			Log.Info("friendly_fire_autoban_bomber default value: " + Config.GetInt("friendly_fire_autoban_bomber"));
			Log.Info("friendly_fire_autoban_disarm default value: " + Config.GetBool("friendly_fire_autoban_disarm"));
			Log.Info("friendly_fire_autoban_rolewl default value: " + Config.GetStringList("friendly_fire_autoban_rolewl"));
			Log.Info("friendly_fire_autoban_invert default value: " + Config.GetFloat("friendly_fire_autoban_invert"));
			Log.Info("friendly_fire_autoban_mirror default value: " + Config.GetFloat("friendly_fire_autoban_mirror"));
			Log.Info("friendly_fire_autoban_undead default value: " + Config.GetFloat("friendly_fire_autoban_undead"));
			Log.Info("friendly_fire_autoban_warntk default value: " + Config.GetInt("friendly_fire_autoban_warntk"));
			Log.Info("friendly_fire_autoban_votetk default value: " + Config.GetInt("friendly_fire_autoban_votetk"));

			string immune = "";
			foreach (string s in Config.GetStringList("friendly_fire_autoban_immune"))
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
			Log.Info("friendly_fire_autoban_immune default value: " + immune);
		}

		public bool isImmune(ReferenceHub player)
		{
			if (this.plugin.immune.Contains(Player.GetGroupName(player)) || this.plugin.immune.Contains(Player.GetRank(player).BadgeText))
			{
				return true;
			}
			else
			{
				return false;
			}

			/*string[] immuneRanks = Config.GetStringList("friendly_fire_autoban_immune");
			foreach (string rank in immuneRanks)
			{
				if (this.outall)
				{
					Log.Info("Does immune rank " + rank + " equal " + player.GetUserGroup().Name + " or " + player.GetRankName() + "?");
				}
				if (String.Equals(rank, player.GetUserGroup().Name, StringComparison.CurrentCultureIgnoreCase) || String.Equals(rank, player.GetRankName(), StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;*/
		}

		public bool isTeamkill(ReferenceHub killer, ReferenceHub victim)
		{
			string killerUserId = Player.GetUserId(killer);
			int killerTeam = (int)Player.GetTeam(killer);
			int killerRole = (int)Player.GetRole(killer);

			string victimUserId = Player.GetUserId(victim);
			int victimTeam = (int)Player.GetTeam(victim);
			int victimRole = (int)Player.GetRole(victim);

			if (String.Equals(killerUserId, victimUserId))
			{
				Log.Info(killerUserId + " equals " + victimUserId + ", this is a suicide and not a teamkill.");
				return false;
			}

			if (this.disarm && Player.IsHandCuffed(victim))
			{
				victimTeam = this.InverseTeams[victimTeam];
				victimRole = this.InverseRoles[victimRole];
				Log.Info(victimUserId + " is handcuffed, team inverted to " + victimTeam + " and role " + victimRole);
			}

			foreach (RoleTuple roleTuple in this.rolewl)
			{
				if (killerRole == roleTuple.KillerRole && victimRole == roleTuple.VictimRole)
				{
					Log.Info("Killer role " + killerRole + " and victim role " + victimRole + " is whitelisted, not a teamkill.");
					return false;
				}
			}

			foreach (TeamTuple teamTuple in this.matrix)
			{
				if (killerTeam == teamTuple.KillerTeam && victimTeam == teamTuple.VictimTeam)
				{
					Log.Info("Team " + killerTeam + " killing " + victimTeam + " WAS detected as a teamkill.");
					return true;
				}
			}

			Log.Info("Team " + killerTeam + " killing " + victimTeam + " was not detected as a teamkill.");
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="steamId"></param>
		/// <returns>scaled ban amount in minutes</returns>
		public int GetScaledBanAmount(string steamId)
		{
			int banLength = 0;
			foreach (int banAmount in this.scaled.Keys.OrderBy(k => k))
			{
				if (this.outall) Log.Info("Ban length set to " + banLength + ". Checking ban amount for key " + banAmount);
				// If ban kills is less than player's kills, set the banLength
				// This will ensure that players who teamkill more than the maximum
				// will still serve the maximum ban length
				if (banAmount < this.Teamkillers[steamId].Teamkills.Count)
				{
					if (this.outall) Log.Info("Ban amount is less than player teamkills.");
					banLength = this.scaled[banAmount];
				}
				// Exact ban amount match is found, set
				else if (banAmount == this.Teamkillers[steamId].Teamkills.Count)
				{
					if (this.outall) Log.Info("Ban amount is equal to player teamkills.");
					banLength = this.scaled[banAmount];
					break;
				}
				// If the smallest ban amount is larger than the player's bans,
				// then the player will not be banned.
				// If banAmount has not been found, it will still be set to 0
				else if (banAmount > this.Teamkillers[steamId].Teamkills.Count)
				{
					if (this.outall) Log.Info("Ban amount is greater than player teamkills.");
					break;
				}
			}
			return banLength;
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnBan")]
		//[PipeMethod]
		public bool OnBan(ReferenceHub player, string playerName, int banLength, List<Teamkill> teamkills)
		{
			String playerUserId = Player.GetUserId(player);
			String playerIpAddress = Player.GetIpAddress(player);
			bool immune = isImmune(player);
			if (immune)
			{
				Log.Info("Admin/Moderator " + playerName + " has avoided a ban for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				return false;
			}
			else if (this.plugin.banWhitelist.Contains(playerUserId))
			{
				Log.Info("Player " + playerName + " " + playerUserId + " " + playerIpAddress + " not being punished by FFA because the player is whitelisted.");
				return false;
			}
			else
			{
				if (teamkills.Count > 3)
				{
					Player.BanPlayer(player, banLength, "Banned " + banLength + " minutes for teamkilling " + teamkills.Count + " players", "FriendlyFireAutoban");
				}
				else
				{
					Player.BanPlayer(player, banLength, "Banned " + banLength + " minutes for teamkilling player(s) " + string.Join(", ", teamkills.Select(teamkill => teamkill.VictimName).ToArray()), "FriendlyFireAutoban");
				}
				Log.Info("Player " + playerName + " has been banned for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				Map.Broadcast(string.Format(this.GetTranslation("banned_output"), playerName, teamkills.Count), 3, false);
				return true;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckRemoveGuns")]
		//[PipeMethod]
		public bool OnCheckRemoveGuns(ReferenceHub killer)
		{
			String killerUserId = Player.GetUserId(killer);
			String killerNickname = Player.GetNickname(killer);
			String killerIpAddress = Player.GetIpAddress(killer);
			if (this.noguns > 0 && this.Teamkillers.ContainsKey(killerUserId) && this.Teamkillers[killerUserId].Teamkills.Count >= this.noguns && !this.isImmune(killer))
			{
				Log.Info("Player " + killerNickname + " " + killerUserId + " " + killerIpAddress + " has had his/her guns removed for teamkilling.");
				Item[] inv = killer.inventory.availableItems;
				for (int i = 0; i < inv.Length; i++)
				{
					switch (inv[i].id)
					{
						case ItemType.GunCOM15:
						case ItemType.GunE11SR:
						case ItemType.GunLogicer:
						case ItemType.MicroHID:
						case ItemType.GunMP7:
						case ItemType.GunProject90:
						case ItemType.GrenadeFrag:
						case ItemType.GrenadeFlash:
							inv[i].id = ItemType.None;
							break;
					}
				}
				Player.Broadcast(killer, 2, this.GetTranslation("noguns_output"), false);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckToSpectator")]
		//[PipeMethod]
		public bool OnCheckToSpectator(ReferenceHub killer)
		{
			String killerUserId = Player.GetUserId(killer);
			String killerNickname = Player.GetNickname(killer);
			String killerIpAddress = Player.GetIpAddress(killer);
			if (this.tospec > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= this.tospec && !this.isImmune(killer))
			{
				Log.Info("Player " + killerNickname + " " + killerUserId + " " + killerIpAddress + " has been moved to spectator for teamkilling " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				Player.Broadcast(killer, 5, this.GetTranslation("tospec_output"), false);
				Player.SetRole(killer, RoleType.Spectator);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeMethod]
		public bool OnCheckUndead(ReferenceHub killer, ReferenceHub victim)
		{
			String killerUserId = Player.GetUserId(killer);
			String killerNickname = Player.GetNickname(killer);
			String killerIpAddress = Player.GetIpAddress(killer);
			String victimUserId = Player.GetUserId(victim);
			String victimNickname = Player.GetNickname(victim);
			String victimIpAddress = Player.GetIpAddress(victim);
			RoleType victimRole = Player.GetRole(victim);
			if (this.undead > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= this.undead && !this.isImmune(killer))
			{
				RoleType oldRole = victimRole;
				//Vector oldPosition = victim.GetPosition();
				Log.Info("Player " + victimNickname + " " + victimUserId + " " + victimIpAddress + " has been respawned as " + oldRole + " after " + killerNickname + " " + killerUserId + " " + killerIpAddress + " teamkilled " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				Player.Broadcast(killer, 5, string.Format(this.GetTranslation("undead_killer_output"), victimNickname), false);
				Player.Broadcast(victim, 5, string.Format(this.GetTranslation("undead_victim_output"), killerNickname), false);
				Timer t = new Timer
				{
					Interval = 3000,
					Enabled = true
				};
				t.Elapsed += delegate
				{
					Log.Info("Respawning victim " + victimNickname + " " + victimUserId + " " + victimIpAddress + "as " + victimRole + "...");
					Player.SetRole(victim, oldRole);
					//victim.Teleport(oldPosition);
					t.Dispose();
				};
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckKick")]
		//[PipeMethod]
		public bool OnCheckKick(ReferenceHub killer)
		{
			String killerUserId = Player.GetUserId(killer);
			String killerNickname = Player.GetNickname(killer);
			String killerIpAddress = Player.GetIpAddress(killer);
			if (this.kicker > 0 && this.Teamkillers[killerUserId].Teamkills.Count == this.kicker && !this.isImmune(killer))
			{
				Log.Info("Player " + killerNickname + " " + killerUserId + " " + killerIpAddress + " has been kicked for teamkilling " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				Player.Broadcast(killer, 1, this.GetTranslation("kicker_output"), false);
				Player.KickPlayer(killer, this.GetTranslation("kicker_output"));
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckVote")]
		//[PipeMethod]
		/*public bool OnVoteTeamkill(ReferenceHub killer)
		{
			if (this.outall)
			{
				Log.Info("votetk > 0: " + this.votetk);
				Log.Info("Teamkiller count is greater than votetk? " + this.Teamkillers[killerUserId].Teamkills.Count);
				Log.Info("Teamkiller is immune? " + this.isImmune(killer));
			}
			if (this.votetk > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= this.votetk && !this.isImmune(killer))
			{
				Log.Info("Player " + killerNickname + " " + killerUserId + " " + killerIpAddress + " is being voted on a ban for teamkilling " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				Dictionary<int, string> options = new Dictionary<int, string>();
				options[1] = "Yes";
				options[2] = "No";
				HashSet<string> votes = new HashSet<string>();
				Dictionary<int, int> counter = new Dictionary<int, int>();

				if (Voting != null && StartVote != null && !Voting.Invoke())
				{
					//this.plugin.InvokeEvent("OnStartVote", "Ban " + killerNickname + "?", options, votes, counter);
					Log.Info("Running vote: " + "Ban " + killerNickname + "?");
					this.StartVote.Invoke("Ban " + killerNickname + "?", options, votes, counter);
					return true;
				}
				else
				{
					this.Warn("patpeter.callvote Voting PipeLink is broken. Cannot start vote.");
					return false;
				}
			}
			else
			{
				return false;
			}
		}*/
	}

	public struct TeamTuple
	{
		public int KillerTeam, VictimTeam;

		public TeamTuple(int killerTeam, int victimRole)
		{
			this.KillerTeam = killerTeam;
			this.VictimTeam = victimRole;
		}
	}

	public struct RoleTuple
	{
		public int KillerRole, VictimRole;

		public RoleTuple(int killerRole, int victimRole)
		{
			this.KillerRole = killerRole;
			this.VictimRole = victimRole;
		}
	}

	public class Teamkiller
	{
		public int PlayerId;
		public string Name;
		public string UserId;
		public string IpAddress;
		public int Kills;
		public int Deaths;
		public List<Teamkill> Teamkills = new List<Teamkill>();
		//public Timer Timer;

		public Teamkiller(int playerId, string name, string userId, string ipAddress)
		{
			this.PlayerId = playerId;
			this.Name = name;
			this.UserId = userId;
			this.IpAddress = ipAddress;
		}

		public float GetKDR()
		{
			return Deaths == 0 ? 0 : (float)Kills / Deaths;
		}

		public override bool Equals(object obj)
		{
			var teamkiller = obj as Teamkiller;
			return teamkiller != null &&
				   PlayerId == teamkiller.PlayerId;
		}

		public override int GetHashCode()
		{
			var hashCode = -1156428363;
			hashCode = hashCode * -1521134295 + PlayerId.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IpAddress);
			return hashCode;
		}

		public override string ToString()
		{
			return PlayerId + " " + Name + " " + UserId + " " + IpAddress;
		}
	}

	public class Teamkill
	{
		public string KillerName;
		public string KillerUserId;
		public RoleType KillerTeamRole;
		public string VictimName;
		public string VictimUserId;
		public RoleType VictimTeamRole;
		public bool VictimDisarmed;
		public DamageType DamageType;
		public int Duration;

		public Teamkill(string killerName, string killerSteamId, RoleType killerTeamRole, string victimName, string victimSteamId, RoleType victimTeamRole, bool victimDisarmed, DamageType damageType, int duration)
		{
			this.KillerName = killerName;
			this.KillerUserId = killerSteamId;
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
			switch (KillerTeamRole)
			{
				case RoleType.ClassD:
					retval += Plugin.GetInstance().GetTranslation("role_dclass");
					break;

				case RoleType.Scientist:
					retval += Plugin.GetInstance().GetTranslation("role_scientist");
					break;

				case RoleType.FacilityGuard:
					retval += Plugin.GetInstance().GetTranslation("role_guard");
					break;

				case RoleType.NtfCadet:
					retval += Plugin.GetInstance().GetTranslation("role_cadet");
					break;

				case RoleType.NtfLieutenant:
					retval += Plugin.GetInstance().GetTranslation("role_lieutenant");
					break;

				case RoleType.NtfCommander:
					retval += Plugin.GetInstance().GetTranslation("role_commander");
					break;

				case RoleType.NtfScientist:
					retval += Plugin.GetInstance().GetTranslation("role_ntf_scientist");
					break;

				case RoleType.ChaosInsurgency:
					retval += Plugin.GetInstance().GetTranslation("role_chaos");
					break;

				case RoleType.Tutorial:
					retval += Plugin.GetInstance().GetTranslation("role_tutorial");
					break;
			}
			retval += " " + Plugin.GetInstance().GetTranslation("role_separator") + " ";
			if (VictimDisarmed)
			{
				retval += Plugin.GetInstance().GetTranslation("role_disarmed");
			}
			switch (VictimTeamRole)
			{
				case RoleType.ClassD:
					retval += Plugin.GetInstance().GetTranslation("role_dclass");
					break;

				case RoleType.Scientist:
					retval += Plugin.GetInstance().GetTranslation("role_scientist");
					break;

				case RoleType.FacilityGuard:
					retval += Plugin.GetInstance().GetTranslation("role_guard");
					break;

				case RoleType.NtfCadet:
					retval += Plugin.GetInstance().GetTranslation("role_cadet");
					break;

				case RoleType.NtfLieutenant:
					retval += Plugin.GetInstance().GetTranslation("role_lieutenant");
					break;

				case RoleType.NtfCommander:
					retval += Plugin.GetInstance().GetTranslation("role_commander");
					break;

				case RoleType.NtfScientist:
					retval += Plugin.GetInstance().GetTranslation("role_ntf_scientist");
					break;

				case RoleType.ChaosInsurgency:
					retval += Plugin.GetInstance().GetTranslation("role_chaos");
					break;

				case RoleType.Tutorial:
					retval += Plugin.GetInstance().GetTranslation("role_tutorial");
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
				   EqualityComparer<RoleType>.Default.Equals(KillerTeamRole, teamkill.KillerTeamRole) &&
				   VictimName == teamkill.VictimName &&
				   VictimUserId == teamkill.VictimUserId &&
				   EqualityComparer<RoleType>.Default.Equals(VictimTeamRole, teamkill.VictimTeamRole) &&
				   VictimDisarmed == teamkill.VictimDisarmed &&
				   DamageType == teamkill.DamageType &&
				   Duration == teamkill.Duration;
		}

		public override int GetHashCode()
		{
			var hashCode = -153347006;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KillerName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(KillerUserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<RoleType>.Default.GetHashCode(KillerTeamRole);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VictimName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VictimUserId);
			hashCode = hashCode * -1521134295 + EqualityComparer<RoleType>.Default.GetHashCode(VictimTeamRole);
			hashCode = hashCode * -1521134295 + VictimDisarmed.GetHashCode();
			hashCode = hashCode * -1521134295 + DamageType.GetHashCode();
			hashCode = hashCode * -1521134295 + Duration.GetHashCode();
			return hashCode;
		}
	}
}
