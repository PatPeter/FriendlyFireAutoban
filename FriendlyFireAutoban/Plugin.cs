using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;

namespace FriendlyFireAutoban
{
	public class Plugin : Plugin<Config>
	{
		private static readonly Lazy<Plugin> LazyInstance = new Lazy<Plugin>(() => new Plugin());

		private Plugin()
		{
		}

		/// <summary>
		/// Gets the lazy instance.
		/// </summary>
		public static Plugin Instance => LazyInstance.Value;
		public EventHandlers EventHandlers;

		/*
		 * FFA internal values
		 */
		internal bool DuringRound = false;
		internal bool ProcessingDisconnect = false;
		internal CoroutineHandle FFAHandle = new CoroutineHandle();

		internal Dictionary<string, Teamkiller> Teamkillers = new Dictionary<string, Teamkiller>();
		internal Dictionary<string, Teamkill> TeamkillVictims = new Dictionary<string, Teamkill>();

		internal HashSet<string> BanWhitelist = new HashSet<string>();

		readonly internal Dictionary<Team, Team> InverseTeams = new Dictionary<Team, Team>()
		{
			{ Team.SCP, Team.SCP },
			{ Team.MTF, Team.CHI },
			{ Team.CHI, Team.MTF },
			{ Team.RSC, Team.CDP },
			{ Team.CDP, Team.RSC },
			{ Team.RIP, Team.RIP },
			{ Team.TUT, Team.TUT },
		};
		readonly internal Dictionary<RoleType, RoleType> InverseRoles = new Dictionary<RoleType, RoleType>()
		{
			{ RoleType.None, RoleType.None },
			{ RoleType.Spectator, RoleType.Spectator },
			{ RoleType.Tutorial, RoleType.Tutorial },
			// ClassD/Scientist
			{ RoleType.ClassD, RoleType.Scientist },
			{ RoleType.Scientist, RoleType.ClassD },
			// NTF to Chaos
			{ RoleType.FacilityGuard, RoleType.ChaosConscript },
			{ RoleType.NtfPrivate, RoleType.ChaosConscript },
			{ RoleType.NtfSpecialist, RoleType.ChaosRifleman },
			{ RoleType.NtfSergeant, RoleType.ChaosRepressor },
			{ RoleType.NtfCaptain, RoleType.ChaosMarauder },
			// Chaos to NTF
			{ RoleType.ChaosConscript, RoleType.NtfPrivate },
			{ RoleType.ChaosRifleman, RoleType.NtfSpecialist },
			{ RoleType.ChaosRepressor, RoleType.NtfSergeant },
			{ RoleType.ChaosMarauder, RoleType.NtfCaptain },
			// SCPs
			{ RoleType.Scp049, RoleType.Scp049 },
			{ RoleType.Scp0492, RoleType.Scp0492 },
			{ RoleType.Scp079, RoleType.Scp079 },
			{ RoleType.Scp096, RoleType.Scp096 },
			{ RoleType.Scp106, RoleType.Scp106 },
			{ RoleType.Scp173, RoleType.Scp173 },
			{ RoleType.Scp93953, RoleType.Scp93989 },
			{ RoleType.Scp93989, RoleType.Scp93953 },
		};

		/*
		 * Ban Events
		 */
		
		public readonly string victim_message = "<size=36>{0} <color=red>teamkilled</color> you at {1}. If this was an accidental teamkill, please press ~ and then type .forgive to prevent this user from being banned.</size>";
		
		public readonly string killer_message = "You teamkilled {0} {1}.";
		
		public readonly string killer_kdr_message = "You teamkilled {0} {1}. Because your K/D ratio is {2}, you will not be punished. Please watch your fire.";
		
		public readonly string killer_warning = "If you teamkill {0} more times you will be banned.";
		
		public readonly string killer_request = "Please do not teamkill.";
		
		public readonly string noguns_output = "Your guns have been removed for <color=red>teamkilling</color>. You will get them back when your teamkill expires.";
		
		public readonly string tospec_output = "You have been moved to spectate for <color=red>teamkilling</color>.";
		
		public readonly string undead_killer_output = "{0} has been respawned because you are <color=red>teamkilling</color> too much. If you continue, you will be banned.";
		
		public readonly string undead_victim_output = "You have been respawned after being teamkilled by {0}.";
		
		public readonly string kicker_output = "You will be kicked for <color=red>teamkilling</color>.";
		
		public readonly string banned_output = "Player {0} has been banned for <color=red>teamkilling</color> {1} players.";

		
		// OFFLINE BAN, DO NOT ADD BBCODE
		public readonly string offline_ban = "Banned {0} minutes for teamkilling {1} players";

		/*
		 * Teamkiller/Teamkill
		 */
		
		public readonly string role_disarmed = "DISARMED ";
		
		public readonly string role_separator = "on";
		
		public readonly string role_dclass = "<color=orange>D-CLASS</color>";
		
		public readonly string role_scientist = "<color=yellow>SCIENTIST</color>";
		
		public readonly string role_guard = "<color=silver>GUARD</color>";
		
		public readonly string role_cadet = "<color=cyan>CADET</color>";
		
		public readonly string role_lieutenant = "<color=aqua>LIEUTENANT</color>";
		
		public readonly string role_commander = "<color=blue>COMMANDER</color>";
		
		public readonly string role_ntf_scientist = "<color=aqua>NTF SCIENTIST</color>";
		
		public readonly string role_chaos = "<color=green>CHAOS</color>";
		
		public readonly string role_tutorial = "<color=lime>TUTORIAL</color>";

		/*
		 * Commands
		 */
		
		public readonly string toggle_description = "Toggle Friendly Fire Autoban on and off.";
		
		public readonly string toggle_disable = "Friendly fire Autoban has been disabled.";
		
		public readonly string toggle_enable = "Friendly fire Autoban has been enabled.";

		
		public readonly string whitelist_description = "Whitelist a user from being banned by FFA until the end of the round.";
		
		public readonly string whitelist_error = "A single name or Steam ID must be provided.";
		
		public readonly string whitelist_add = "Added player {0} ({1}) to ban whitelist.";
		
		public readonly string whitelist_remove = "Removed player {0} ({1}) from ban whitelist.";

		/*
		 * Client Commands
		 */
		
		public readonly string forgive_command = "forgive";
		
		public readonly string forgive_success = "You have forgiven {0} {1}!";
		
		public readonly string forgive_duplicate = "You already forgave {0} {1}.";
		
		public readonly string forgive_disconnect = "The player has disconnected.";
		
		public readonly string forgive_invalid = "You have not been teamkilled yet.";

		
		public readonly string tks_command = "tks";
		
		public readonly string tks_no_teamkills = "No players by this name or Steam ID has any teamkills.";
		
		public readonly string tks_teamkill_entry = "({0}) {1} teamkilled {2} {3}.";
		
		public readonly string tks_not_found = "Player name not provided or not quoted.";

		
		public readonly string ffa_disabled = "Friendly Fire Autoban is currently disabled.";

		public string GetTranslation(string name)
		{
			Type t = typeof(FriendlyFireAutoban.Plugin);
			FieldInfo p = t.GetField(name);
			if (p != null)
			{
				return (string) p.GetValue(this);
			}
			else
			{
				return "INVALID TRANSLATION: " + name;
			}
		}

		public override string Name { get; } = "Friendly Fire Autoban";

		public override void OnEnabled()
		{
			try
			{
				Log.Debug("Initializing event handlers..");
				//Set instance varible to a new instance, this should be nulled again in OnDisable
				EventHandlers = new EventHandlers(this);
				//Hook the events you will be using in the plugin. You should hook all events you will be using here, all events should be unhooked in OnDisabled
				Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
				Exiled.Events.Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;

				Exiled.Events.Handlers.Player.Verified += EventHandlers.OnPlayerVerified;
				Exiled.Events.Handlers.Player.Destroying += EventHandlers.OnPlayerDestroying;

				Exiled.Events.Handlers.Player.Died += EventHandlers.OnPlayerDeath;
				Exiled.Events.Handlers.Player.Hurting += EventHandlers.OnPlayerHurt;

				Exiled.Events.Handlers.Player.Spawning += EventHandlers.OnPlayerSpawn;
				Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnSetClass;
				Exiled.Events.Handlers.Player.PickingUpItem += EventHandlers.OnPickupItem;

				//Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += EventHandlers.OnRACommand;
				//Exiled.Events.Handlers.Server.SendingConsoleCommand += EventHandlers.OnConsoleCommand;

				Log.Info($"Friendly Fire Autoban has been loaded!");
			}
			catch (Exception e)
			{
				//This try catch is redundant, as EXILED will throw an error before this block can, but is here as an example of how to handle exceptions/errors
				Log.Error($"There was an error loading the plugin: {e}");
			}
		}

		public override void OnDisabled()
		{
			Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;

			Exiled.Events.Handlers.Player.Verified -= EventHandlers.OnPlayerVerified;
			Exiled.Events.Handlers.Player.Destroying -= EventHandlers.OnPlayerDestroying;

			Exiled.Events.Handlers.Player.Died -= EventHandlers.OnPlayerDeath;
			Exiled.Events.Handlers.Player.Hurting -= EventHandlers.OnPlayerHurt;

			Exiled.Events.Handlers.Player.Spawning -= EventHandlers.OnPlayerSpawn;
			Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnSetClass;
			Exiled.Events.Handlers.Player.PickingUpItem -= EventHandlers.OnPickupItem;

			//Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= EventHandlers.OnRACommand;
			//Exiled.Events.Handlers.Server.SendingConsoleCommand -= EventHandlers.OnConsoleCommand;

			EventHandlers = null;
		}

		public IEnumerator<float> FFACoRoutine()
		{
			for (; ; )
			{
				List<Player> players = Player.List.Where(p => Plugin.Instance.Teamkillers.ContainsKey(p.UserId) && Plugin.Instance.Teamkillers[p.UserId].Teamkills.Count > 0).ToList();

				foreach (Player killer in players)
				{
					string killerUserId = killer.UserId;
					string killerIpAddress = killer.IPAddress;
					string killerNickname = killer.Nickname;
					Team killerTeam = killer.Team;
					string killerOutput = killerNickname + " " + killerUserId + " " + killerIpAddress;
					Teamkiller killerTeamkiller = Plugin.Instance.Teamkillers[killer.UserId];
					
					if (killerTeamkiller.TimerCountdown > 0)
					{
						// Decrease teamkiller timer by 1 second
						killerTeamkiller.TimerCountdown--;
					}
					else
					{
						/*
						 * If ban system is #3, every player teamkill cancels and restarts the timer
						 * Wait until the timer expires after the teamkilling has ended to find out 
						 * how much teamkilling the player has done.
						 */
						if (Plugin.Instance.Config.System == 3)
						{
							int banLength = Plugin.Instance.GetScaledBanAmount(killerUserId);
							if (banLength > 0)
							{
								Plugin.Instance.OnBan(killer, killerNickname, banLength, killerTeamkiller.Teamkills);
							}
							else
							{
								if (Plugin.Instance.Config.OutAll)
								{
									Log.Info("Player " + killerUserId + " " + killerTeamkiller.Teamkills.Count + " teamkills is not bannable.");
								}
							}
						}
						
						// Forgive teamkills in ban system #2
						if (Plugin.Instance.Config.System == 2)
						{
							Teamkill firstTeamkill = killerTeamkiller.Teamkills[0];
							killerTeamkiller.Teamkills.RemoveAt(0);
							Log.Info("Player " + killerOutput + " " + killerTeam.ToString() + " teamkill " + firstTeamkill + " expired, counter now at " + killerTeamkiller.Teamkills.Count + ".");
						}
					}
				}
				
				yield return Timing.WaitForSeconds(1f);
			}
		}

		public bool isImmune(Player player)
		{
			if (Plugin.Instance.Config.Immune.Contains(player.GroupName) || (player.GlobalBadge.HasValue ? Plugin.Instance.Config.Immune.Contains(player.GlobalBadge.Value.Text) : false))
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
				if (Plugin.Instance.Config.OutAll)
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

		public bool isTeamkill(Player killer, Player victim)
		{
			Teamkiller teamkiller = Plugin.Instance.AddAndGetTeamkiller(killer);

			string killerUserId = killer.UserId;
			Team killerTeam = killer.Team;
			RoleType killerRole = killer.Role;

			string victimUserId = victim.UserId;
			Team victimTeam = victim.Team;
			RoleType victimRole = victim.Role;

			if (string.Equals(killerUserId, victimUserId))
			{
				Log.Info(killerUserId + " equals " + victimUserId + ", this is a suicide and not a teamkill.");
				return false;
			}

			if (Plugin.Instance.Config.Disarm && victim.IsCuffed)
			{
				victimTeam = this.InverseTeams[victimTeam];
				victimRole = this.InverseRoles[victimRole];
				Log.Info(victimUserId + " is handcuffed, team inverted to " + victimTeam + " and role " + victimRole);
			}

			foreach (RoleTuple roleTuple in Plugin.Instance.Config.RoleWL)
			{
				if (killerRole == roleTuple.KillerRole && victimRole == roleTuple.VictimRole)
				{
					Log.Info("Killer role " + killerRole + " and victim role " + victimRole + " is whitelisted, not a teamkill.");
					return false;
				}
			}

			foreach (TeamTuple teamTuple in Plugin.Instance.Config.Matrix)
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
		/// <param name="userId"></param>
		/// <returns>scaled ban amount in minutes</returns>
		public int GetScaledBanAmount(string userId)
		{
			int banLength = 0;
			foreach (int banAmount in Plugin.Instance.Config.Scaled.Keys.OrderBy(k => k))
			{
				if (Plugin.Instance.Config.OutAll) Log.Info("Ban length set to " + banLength + ". Checking ban amount for key " + banAmount);
				// If ban kills is less than player's kills, set the banLength
				// This will ensure that players who teamkill more than the maximum
				// will still serve the maximum ban length
				if (banAmount < this.Teamkillers[userId].Teamkills.Count)
				{
					if (Plugin.Instance.Config.OutAll) Log.Info("Ban amount is less than player teamkills.");
					banLength = Plugin.Instance.Config.Scaled[banAmount];
				}
				// Exact ban amount match is found, set
				else if (banAmount == this.Teamkillers[userId].Teamkills.Count)
				{
					if (Plugin.Instance.Config.OutAll) Log.Info("Ban amount is equal to player teamkills.");
					banLength = Plugin.Instance.Config.Scaled[banAmount];
					break;
				}
				// If the smallest ban amount is larger than the player's bans,
				// then the player will not be banned.
				// If banAmount has not been found, it will still be set to 0
				else if (banAmount > this.Teamkillers[userId].Teamkills.Count)
				{
					if (Plugin.Instance.Config.OutAll) Log.Info("Ban amount is greater than player teamkills.");
					break;
				}
			}
			return banLength;
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnBan")]
		//[PipeMethod]
		public bool OnBan(Player player, string playerName, int banLength, List<Teamkill> teamkills)
		{
			string playerUserId = player.UserId;
			string playerIpAddress = player.IPAddress;
			bool immune = isImmune(player);
			if (immune)
			{
				Log.Info("Admin/Moderator " + playerName + " has avoided a ban for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				return false;
			}
			else if (Plugin.Instance.BanWhitelist.Contains(playerUserId))
			{
				Log.Info("Player " + playerName + " " + playerUserId + " " + playerIpAddress + " not being punished by FFA because the player is whitelisted.");
				return false;
			}
			else
			{
				if (teamkills.Count > 3)
				{
					player.Ban(banLength, "Banned " + banLength + " minutes for teamkilling " + teamkills.Count + " players", "FriendlyFireAutoban");
				}
				else
				{
					player.Ban(banLength, "Banned " + banLength + " minutes for teamkilling player(s) " + string.Join(", ", teamkills.Select(teamkill => teamkill.VictimName).ToArray()), "FriendlyFireAutoban");
				}
				Log.Info("Player " + playerName + " has been banned for " + banLength + " minutes after teamkilling " + teamkills + " players during the round.");
				Map.Broadcast(new Exiled.API.Features.Broadcast(string.Format(this.GetTranslation("banned_output"), playerName, teamkills.Count), 3), false);
				return true;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckRemoveGuns")]
		//[PipeMethod]
		public bool OnCheckRemoveGuns(Player killer)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IPAddress;
			if (Plugin.Instance.Config.NoGuns > 0 && this.Teamkillers.ContainsKey(killerUserId) && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.NoGuns && !this.isImmune(killer))
			{
				Log.Info("Player " + killerNickname + " " + killerUserId + " " + killerIpAddress + " has had his/her guns removed for teamkilling.");


				List<Item> itemsToRemove = new List<Item>();
				foreach (Item i in killer.Items)
				{
					switch (i.Type)
					{
						case ItemType.GunAK:
						case ItemType.GunCOM15:
						case ItemType.GunCOM18:
						case ItemType.GunE11SR:
						case ItemType.GunLogicer:
						case ItemType.MicroHID:
						case ItemType.GunCrossvec:
						case ItemType.GunFSP9:
						case ItemType.GunRevolver:
						case ItemType.GunShotgun:
						case ItemType.GrenadeHE:
						case ItemType.GrenadeFlash:
							itemsToRemove.Add(i);
							break;
					}
				}
				foreach (Item i in itemsToRemove)
				{
					killer.RemoveItem(i);
				}

				killer.Broadcast(new Exiled.API.Features.Broadcast(this.GetTranslation("noguns_output"), 2), false);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckToSpectator")]
		//[PipeMethod]
		public bool OnCheckToSpectator(Player killer)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IPAddress;
			if (Plugin.Instance.Config.ToSpec > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.ToSpec && !this.isImmune(killer))
			{
				Log.Info("Player " + killerNickname + " " + killerUserId + " " + killerIpAddress + " has been moved to spectator for teamkilling " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				killer.Broadcast(new Exiled.API.Features.Broadcast(this.GetTranslation("tospec_output"), 5), false);
				killer.SetRole(RoleType.Spectator);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeMethod]
		public bool OnCheckUndead(Player killer, Player victim)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IPAddress;
			string victimUserId = victim.UserId;
			string victimNickname = victim.Nickname;
			string victimIpAddress = victim.IPAddress;
			RoleType victimRole = victim.Role;
			if (Plugin.Instance.Config.Undead > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.Undead && !this.isImmune(killer))
			{
				RoleType oldRole = victimRole;
				//Vector oldPosition = victim.GetPosition();
				Log.Info("Player " + victimNickname + " " + victimUserId + " " + victimIpAddress + " has been respawned as " + oldRole + " after " + killerNickname + " " + killerUserId + " " + killerIpAddress + " teamkilled " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				killer.Broadcast(new Exiled.API.Features.Broadcast(string.Format(this.GetTranslation("undead_killer_output"), victimNickname), 5), false);
				victim.Broadcast(new Exiled.API.Features.Broadcast(string.Format(this.GetTranslation("undead_victim_output"), killerNickname), 5), false);
				Timer t = new Timer
				{
					Interval = 3000,
					Enabled = true
				};
				t.Elapsed += delegate
				{
					Log.Info("Respawning victim " + victimNickname + " " + victimUserId + " " + victimIpAddress + "as " + victimRole + "...");
					victim.SetRole(oldRole);
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
		public bool OnCheckKick(Player killer)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IPAddress;
			if (Plugin.Instance.Config.Kicker > 0 && this.Teamkillers[killerUserId].Teamkills.Count == Plugin.Instance.Config.Kicker && !this.isImmune(killer))
			{
				Log.Info("Player " + killerNickname + " " + killerUserId + " " + killerIpAddress + " has been kicked for teamkilling " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				killer.Broadcast(new Exiled.API.Features.Broadcast(this.GetTranslation("kicker_output"), 5), true);
				killer.Kick(this.GetTranslation("kicker_output"), "FriendlyFireAutoban");
				return true;
			}
			else
			{
				return false;
			}
		}

		public Teamkiller AddAndGetTeamkiller(Player player)
		{
			int playerId = player.Id;
			string playerNickname = player.Nickname;
			string playerUserId = player.UserId;
			string playerIpAddress = player.IPAddress;

			if (!Plugin.Instance.Teamkillers.ContainsKey(playerUserId))
			{
				Log.Info("Adding Teamkiller entry for player #" + playerId + " " + playerNickname + " [" + playerUserId + "] [" + playerIpAddress + "]");
				Plugin.Instance.Teamkillers[playerUserId] = new Teamkiller(playerId, playerNickname, playerUserId, playerIpAddress);
			}
			else
			{
				Log.Info("Fetching Teamkiller entry for player #" + playerId + " " + playerNickname + " [" + playerUserId + "] [" + playerIpAddress + "]");
			}
			return Plugin.Instance.Teamkillers[playerUserId];
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckVote")]
		//[PipeMethod]
		/*public bool OnVoteTeamkill(Player killer)
		{
			if (Plugin.Instance.Config.OutAll)
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
					//Plugin.Instance.InvokeEvent("OnStartVote", "Ban " + killerNickname + "?", options, votes, counter);
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

	public struct RoleTuple
	{
		public RoleType KillerRole, VictimRole;

		public RoleTuple(RoleType killerRole, RoleType victimRole)
		{
			this.KillerRole = killerRole;
			this.VictimRole = victimRole;
		}

		public override string ToString()
		{
			return KillerRole + ":" + VictimRole;
		}
	}

	public class Teamkiller
	{
		public int PlayerId;
		public string Name;
		public string UserId;
		public string IpAddress;
		// Must keep track of team and role for when the player is sent to spectator and events are still running
		public short Team;
		public short Role;
		// For kdsafe
		public int Kills;
		public int Deaths;
		public List<Teamkill> Teamkills = new List<Teamkill>();
		//public Timer Timer;
		public int TimerCountdown = 0;

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
		public short KillerTeamRole;
		public string VictimName;
		public string VictimUserId;
		public short VictimTeamRole;
		public bool VictimDisarmed;
		public short DamageType;
		public int Duration;

		public Teamkill(string killerName, string killerSteamId, short killerTeamRole, string victimName, string victimSteamId, short victimTeamRole, bool victimDisarmed, short damageType, int duration)
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
