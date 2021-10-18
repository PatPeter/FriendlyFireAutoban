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
		/*
		 * Static Fields
		 */
		private static readonly Lazy<Plugin> LazyInstance = new Lazy<Plugin>(() => new Plugin());
		/// <summary>
		/// Gets the lazy instance.
		/// </summary>
		public static Plugin Instance => LazyInstance.Value;

		/*
		 * Public Instance Fields
		 */
		public EventHandlers EventHandlers;
		public override string Name { get; } = "Friendly Fire Autoban";

		/*
		 * Internal Instance Fields
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

		private Plugin()
		{
		}

		public string GetTranslation(string name)
		{
			Type t = typeof(FriendlyFireAutoban.Translation);
			PropertyInfo p = t.GetProperty(name);
			// Plugin.Instance.Config.Translations.ContainsKey(name)
			if (p != null)
			{
				return (string) p.GetValue(this);
				//return Plugin.Instance.Config.Translations[name];
			}
			else
			{
				return "INVALID TRANSLATION: " + name;
			}
		}

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

				Log.Info(AssemblyInfo.Name + " v" + AssemblyInfo.Version + " by " + AssemblyInfo.Author + " has been enabled!");
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

		internal IEnumerator<float> FFACoRoutine()
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

		internal bool isImmune(Player player)
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

		internal bool isTeamkill(Player killer, Player victim)
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

			//List<RoleTuple> roleTuples = new List<RoleTuple>();
			//foreach (string rawRoleTuple in Plugin.Instance.Config.RoleWL)
			//{
			//	string[] tuple = rawRoleTuple.Split(':');
			//
			//}

			foreach (RoleTuple roleTuple in Plugin.Instance.Config.GetRoleWL())
			{
				if (killerRole == roleTuple.KillerRole && victimRole == roleTuple.VictimRole)
				{
					Log.Info("Killer role " + killerRole + " and victim role " + victimRole + " is whitelisted, not a teamkill.");
					return false;
				}
			}

			foreach (TeamTuple teamTuple in Plugin.Instance.Config.GetMatrix())
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
		internal int GetScaledBanAmount(string userId)
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
		internal bool OnBan(Player player, string playerName, int banLength, List<Teamkill> teamkills)
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
		internal bool OnCheckRemoveGuns(Player killer)
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
		internal bool OnCheckToSpectator(Player killer)
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
		internal bool OnCheckUndead(Player killer, Player victim)
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
		internal bool OnCheckKick(Player killer)
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

		internal Teamkiller AddAndGetTeamkiller(Player player)
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
		internal bool OnVoteTeamkill(Player killer)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IPAddress;

			if (Plugin.Instance.Config.OutAll)
			{
				Log.Info("votetk > 0: " + Plugin.Instance.Config.VoteTK);
				Log.Info("Teamkiller count is greater than votetk? " + this.Teamkillers[killerUserId].Teamkills.Count);
				Log.Info("Teamkiller is immune? " + this.isImmune(killer));
			}
			if (Plugin.Instance.Config.VoteTK > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.VoteTK && !this.isImmune(killer))
			{
				Log.Info("Player " + killerNickname + " " + killerUserId + " " + killerIpAddress + " is being voted on a ban for teamkilling " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				Dictionary<int, string> options = new Dictionary<int, string>();
				options[1] = "Yes";
				options[2] = "No";
				HashSet<string> votes = new HashSet<string>();
				Dictionary<int, int> counter = new Dictionary<int, int>();

				/*if (Voting != null && StartVote != null && !Voting.Invoke())
				{
					//Plugin.Instance.InvokeEvent("OnStartVote", "Ban " + killerNickname + "?", options, votes, counter);
					Log.Info("Running vote: " + "Ban " + killerNickname + "?");
					this.StartVote.Invoke("Ban " + killerNickname + "?", options, votes, counter);
					return true;
				}
				else
				{
					Log.Warn("patpeter.callvote Voting PipeLink is broken. Cannot start vote.");
					return false;
				}*/
				Log.Warn("patpeter.callvote Voting PipeLink is broken. Cannot start vote.");
				return false;
			}
			else
			{
				return false;
			}
		}
	}
}
