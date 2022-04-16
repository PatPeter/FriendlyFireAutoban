using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using static BanHandler;
using Exiled.Permissions.Extensions;
using Exiled.API.Enums;

namespace FriendlyFireAutoban
{
	class Plugin : Plugin<Config, Translation>
	{
		public static Plugin Instance { get; set; } = null;

		/*
		 * Public Instance Fields
		 */
		public EventHandlers EventHandlers;
		public override string Name { get; } = FriendlyFireAutoban.AssemblyInfo.Author;
		public override string Author { get; } = FriendlyFireAutoban.AssemblyInfo.Author;
		public override Version Version { get; } = new Version(FriendlyFireAutoban.AssemblyInfo.Version);
		public override string Prefix { get; } = FriendlyFireAutoban.AssemblyInfo.ConfigPrefix;
		public override Version RequiredExiledVersion { get; } = new Version(5, 1, 3);
		public override PluginPriority Priority { get; } = PluginPriority.Default;

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
				return (string) p.GetValue(Translation);
				//return Plugin.Instance.Config.Translations[name];
			}
			else
			{
				return $"INVALID TRANSLATION: {name}";
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
				Exiled.Events.Handlers.Server.ReloadedConfigs += EventHandlers.OnReloadedConfig;

				Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
				Exiled.Events.Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;

				Exiled.Events.Handlers.Player.Verified += EventHandlers.OnPlayerVerified;
				Exiled.Events.Handlers.Player.Destroying += EventHandlers.OnPlayerDestroying;

				Exiled.Events.Handlers.Player.Hurting += EventHandlers.OnPlayerHurting;
				Exiled.Events.Handlers.Player.Dying += EventHandlers.OnPlayerDying;

				Exiled.Events.Handlers.Player.Spawning += EventHandlers.OnPlayerSpawn;
				Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnSetClass;
				Exiled.Events.Handlers.Player.PickingUpItem += EventHandlers.OnPickupItem;

				//Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += EventHandlers.OnRACommand;
				//Exiled.Events.Handlers.Server.SendingConsoleCommand += EventHandlers.OnConsoleCommand;

				Log.Info($"{AssemblyInfo.Name} v{AssemblyInfo.Version} by {AssemblyInfo.Author} has been enabled!");
			}
			catch (Exception e)
			{
				//This try catch is redundant, as EXILED will throw an error before this block can, but is here as an example of how to handle exceptions/errors
				Log.Error($"There was an error loading the plugin: {e}");
			}
		}

		public override void OnDisabled()
		{
			Exiled.Events.Handlers.Server.ReloadedConfigs -= EventHandlers.OnReloadedConfig;

			Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;

			Exiled.Events.Handlers.Player.Verified -= EventHandlers.OnPlayerVerified;
			Exiled.Events.Handlers.Player.Destroying -= EventHandlers.OnPlayerDestroying;
			
			Exiled.Events.Handlers.Player.Hurting -= EventHandlers.OnPlayerHurting;
			Exiled.Events.Handlers.Player.Dying -= EventHandlers.OnPlayerDying;

			Exiled.Events.Handlers.Player.Spawning -= EventHandlers.OnPlayerSpawn;
			Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnSetClass;
			Exiled.Events.Handlers.Player.PickingUpItem -= EventHandlers.OnPickupItem;

			//Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= EventHandlers.OnRACommand;
			//Exiled.Events.Handlers.Server.SendingConsoleCommand -= EventHandlers.OnConsoleCommand;

			EventHandlers = null;
		}

		public void PrintConfigs()
		{
			Log.Info("friendly_fire_autoban.enable value: " + Plugin.Instance.Config.IsEnabled);
			Log.Info("friendly_fire_autoban.system value: " + Plugin.Instance.Config.System);

			Log.Info("friendly_fire_autoban.matrix default value: " + string.Join(";", Plugin.Instance.Config.Matrix));
			string matrix = "";
			foreach (TeamTuple tt in Plugin.Instance.Config.GetMatrixCache())
			{
				if (matrix.Length == 0)
				{
					matrix += tt;
				}
				else
				{
					matrix += ";" + tt;
				}
			}
			Log.Info("friendly_fire_autoban.matrix cached  value: " + matrix);

			Log.Info("friendly_fire_autoban.amount value: " + Plugin.Instance.Config.Amount);
			Log.Info("friendly_fire_autoban.length value: " + Plugin.Instance.Config.Length);
			Log.Info("friendly_fire_autoban.expire value: " + Plugin.Instance.Config.Expire);

			string scaled = "";
			foreach (KeyValuePair<int, int> scale in Plugin.Instance.Config.Scaled)
			{
				if (scaled.Length == 0)
				{
					scaled += scale.Key + ":" + scale.Value;
				}
				else
				{
					scaled += ',' + scale.Key + ":" + scale.Value;
				}
			}
			Log.Info("friendly_fire_autoban.scaled value: " + scaled);

			Log.Info("friendly_fire_autoban.noguns value: " + Plugin.Instance.Config.NoGuns);
			Log.Info("friendly_fire_autoban.tospec value: " + Plugin.Instance.Config.ToSpec);
			Log.Info("friendly_fire_autoban.kicker value: " + Plugin.Instance.Config.Kicker);
			Log.Info("friendly_fire_autoban.bomber value: " + Plugin.Instance.Config.Bomber);
			Log.Info("friendly_fire_autoban.disarm value: " + Plugin.Instance.Config.Disarm);

			Log.Info("friendly_fire_autoban.rolewl default value: " + string.Join(";", Plugin.Instance.Config.RoleWl));
			string roleTuples = "";
			foreach (RoleTuple rt in Plugin.Instance.Config.GetRoleWlCache())
			{
				if (roleTuples.Length == 0)
				{
					roleTuples += rt;
				}
				else
				{
					roleTuples += ";" + rt;
				}
			}
			Log.Info("friendly_fire_autoban.rolewl cached  value: " + roleTuples);

			Log.Info("friendly_fire_autoban.invert value: " + Plugin.Instance.Config.Invert);
			Log.Info("friendly_fire_autoban.mirror value: " + Plugin.Instance.Config.Mirror);
			Log.Info("friendly_fire_autoban.undead value: " + Plugin.Instance.Config.Undead);
			Log.Info("friendly_fire_autoban.warntk value: " + Plugin.Instance.Config.WarnTk);
			Log.Info("friendly_fire_autoban.votetk value: " + Plugin.Instance.Config.VoteTk);
			//Log.Info("friendly_fire_autoban.immune value: " + string.Join(",", Plugin.Instance.Config.Immune));
		}

		internal IEnumerator<float> FFACoRoutine()
		{
			for (; ; )
			{
				//List<Player> players = Player.List.Where(p => Plugin.Instance.Teamkillers.ContainsKey(p.UserId) && Plugin.Instance.Teamkillers[p.UserId].Teamkills.Count > 0).Distinct().ToList();
				List<Teamkiller> teamkillers = Plugin.Instance.Teamkillers.Values.Where(tker => tker.Teamkills.Count > 0).Distinct().ToList();

				foreach (Teamkiller killer in teamkillers)
				{
					string killerUserId = killer.UserId;
					string killerIpAddress = killer.IPAddress;
					string killerNickname = killer.Nickname;
					Team killerTeam = killer.Team;
					string killerOutput = $"{killerNickname} {killerUserId} {killerIpAddress}";
					Teamkiller killerTeamkiller = Plugin.Instance.Teamkillers[killer.UserId];

					if (killerTeamkiller.TimerCountdown > 0)
					{
						killerTeamkiller.TimerCountdown--;
						// Decrease teamkiller timer by 1 second
						//if (Plugin.Instance.Config.OutAll)
						//{
						//	Log.Info("Decrease timer for " + killerTeamkiller + " from " + killerTeamkiller.TimerCountdown + " to " + (killerTeamkiller.TimerCountdown - 1));
						//}
					}
					else if (killerTeamkiller.TimerCountdown == 0)
					{
						killerTeamkiller.TimerCountdown--;
						/*
						 * If ban system is #3, every player teamkill cancels and restarts the timer
						 * Wait until the timer expires after the teamkilling has ended to find out 
						 * how much teamkilling the player has done.
						 */
						if (Plugin.Instance.Config.System == 3)
						{
							int banLength = Plugin.Instance.GetScaledBanAmount(killerUserId);
							if (banLength > 0 && !killerTeamkiller.Banned)
							{
								Plugin.Instance.OnBan(killer, killerNickname, banLength);
								Log.Info($"Banned player {killerTeamkiller} for accumulating scaled ban amount {banLength} for {killerTeamkiller.Teamkills.Count} teamkills.");
								//continue;
							}
							//else
							//{
							//	if (Plugin.Instance.Config.OutAll)
							//	{
							//		Log.Info("Player " + killerUserId + " " + killerTeamkiller.Teamkills.Count + " teamkills is not bannable.");
							//	}
							//}
						}
						
						// Forgive teamkills in ban system #2 and #3
						// Continue to forgive teamkills after players leave the server
						if (Plugin.Instance.Config.System > 1)
						{
							Teamkill firstTeamkill = killerTeamkiller.Teamkills[0];
							killerTeamkiller.Teamkills.RemoveAt(0);
							Log.Info($"Player {killerOutput} {killerTeam} teamkill {firstTeamkill} expired, counter now at {killerTeamkiller.Teamkills.Count}.");
						}

						if (killerTeamkiller.Teamkills.Count > 0)
						{
							killerTeamkiller.TimerCountdown = Plugin.Instance.Config.Expire;
						}
					}
				}
				
				yield return Timing.WaitForSeconds(1f);
			}
		}

		internal bool isImmune(Player player)
		{
			//if (Plugin.Instance.Config.Immune.Contains(player.GroupName) || (player.GlobalBadge.HasValue ? Plugin.Instance.Config.Immune.Contains(player.GlobalBadge.Value.Text) : false))
			if (player.CheckPermission("ffa.immune"))
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

		internal bool isTeamkill(Player killer, Player victim, bool death)
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
				if (death) Log.Info($"{killerUserId} equals {victimUserId}, this is a suicide and not a teamkill.");
				return false;
			}

			if (Plugin.Instance.Config.Disarm && victim.IsCuffed)
			{
				victimTeam = this.InverseTeams[victimTeam];
				victimRole = this.InverseRoles[victimRole];
				if (death) Log.Info($"{victimUserId} is handcuffed, team inverted to {victimTeam} and role {victimRole}.");
			}

			//List<RoleTuple> roleTuples = new List<RoleTuple>();
			//foreach (string rawRoleTuple in Plugin.Instance.Config.RoleWL)
			//{
			//	string[] tuple = rawRoleTuple.Split(':');
			//
			//}

			foreach (RoleTuple roleTuple in Plugin.Instance.Config.GetRoleWlCache())
			{
				if (killerRole == roleTuple.KillerRole && victimRole == roleTuple.VictimRole)
				{
					if (death) Log.Info($"Killer role {killerRole} and victim role {victimRole} is whitelisted, not a teamkill.");
					return false;
				}
			}

			foreach (TeamTuple teamTuple in Plugin.Instance.Config.GetMatrixCache())
			{
				if (killerTeam == teamTuple.KillerTeam && victimTeam == teamTuple.VictimTeam)
				{
					if (death) Log.Info($"Team {killerTeam} killing {victimTeam} WAS detected as a teamkill.");
					return true;
				}
			}

			if (death) Log.Info($"Team {killerTeam} killing {victimTeam} was not detected as a teamkill.");
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
				if (Plugin.Instance.Config.OutAll) Log.Info($"Ban length set to {banLength}. Checking ban amount for key {banAmount}.");
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
		internal bool OnBan(Teamkiller teamkiller, string killerNickname, int banLength)
		{
			if (teamkiller.Banned)
			{
				Log.Warn("[OnBan] Attempted to log repeat ban for " + teamkiller);
				return false;
			}

			// If two players with the same UserId are on the server, this will cause a problem
			Player player = Player.List.Where(tk => tk.UserId == teamkiller.UserId).First();
			string killerUserId, killerIpAddress;
			bool immune;
			if (player != null)
			{
				killerUserId = player.UserId;
				killerIpAddress = player.IPAddress;
				immune = isImmune(player);
			}
			else
			{
				killerUserId = teamkiller.UserId;
				killerIpAddress = teamkiller.IPAddress;
				// TODO: Save UserGroup in Teamkiller so that the method can be used here?
				immune = false;
			}
			List<Teamkill> teamkills = teamkiller.Teamkills;

			if (immune)
			{
				Log.Info($"Admin/Moderator {killerNickname} has avoided a ban for {banLength} minutes after teamkilling {teamkills.Count} players during the round.");
				return false;
			}
			else if (Plugin.Instance.BanWhitelist.Contains(killerUserId))
			{
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} not being punished by FFA because the player is whitelisted.");
				return false;
			}
			else
			{
				string banReason;
				// If teamkills are more than 3, simply provide the count instead of listing each name off
				if (teamkills.Count > 3)
				{
					banReason = $"Banned {banLength} minutes for teamkilling {teamkills.Count} players";
				}
				else
				{
					banReason = $"Banned {banLength} minutes for teamkilling player(s) " + string.Join(", ", teamkills.Select(teamkill => teamkill.VictimName).ToArray());
				}

				if (player != null)
				{
					player.Ban(banLength, banReason, "FriendlyFireAutoban");
				}
				else
				// if (teamkiller.Disconnected)
				// If the player cannot be found, then by defintion it is a disconnected user
				// Continue to track the boolean for garbage collection
				{
					long now = DateTime.Now.Ticks;

					BanDetails userBan = new BanDetails();
					userBan.OriginalName = teamkiller.Nickname;
					userBan.Id = teamkiller.UserId;
					// Calculate ticks
					userBan.Expires = now + (banLength * 60 * 10000000);
					userBan.Reason = string.Format(Plugin.Instance.GetTranslation("offline_ban"), banLength, teamkills);
					userBan.Issuer = "FriendlyFireAutoban";
					userBan.IssuanceTime = now;
					BanHandler.IssueBan(userBan, BanType.UserId);
					Log.Info($"{teamkiller.Nickname} / {teamkiller.UserId}: Banned {banLength} minutes for teamkilling {teamkills} players");

					BanDetails ipBan = new BanDetails();
					ipBan.OriginalName = teamkiller.Nickname;
					ipBan.Id = teamkiller.IPAddress;
					// Calculate ticks
					ipBan.Expires = now + (banLength * 60 * 10000000);
					ipBan.Reason = string.Format(Plugin.Instance.GetTranslation("offline_ban"), banLength, teamkills);
					ipBan.Issuer = "FriendlyFireAutoban";
					ipBan.IssuanceTime = now;
					BanHandler.IssueBan(ipBan, BanType.IP);
					Log.Info($"{teamkiller.Nickname} / {teamkiller.UserId}: Banned {banLength} minutes for teamkilling {teamkills} players");
				}

				teamkiller.Banned = true;
				Log.Info($"Player {killerNickname} has been banned for {banLength} minutes after teamkilling {teamkills} players during the round.");
				Map.Broadcast(new Exiled.API.Features.Broadcast(string.Format(this.GetTranslation("banned_output"), killerNickname, teamkills.Count), 3), false);
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
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} has had his/her guns removed for teamkilling.");


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
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} has been moved to spectator for teamkilling {this.Teamkillers[killerUserId].Teamkills.Count} times.");
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
				Log.Info($"Player {victimNickname} {victimUserId} {victimIpAddress} has been respawned as {oldRole} after {killerNickname} {killerUserId} {killerIpAddress} teamkilled " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				killer.Broadcast(new Exiled.API.Features.Broadcast(string.Format(this.GetTranslation("undead_killer_output"), victimNickname), 5), false);
				victim.Broadcast(new Exiled.API.Features.Broadcast(string.Format(this.GetTranslation("undead_victim_output"), killerNickname), 5), false);
				Timer t = new Timer
				{
					Interval = 3000,
					Enabled = true
				};
				t.Elapsed += delegate
				{
					Log.Info($"Respawning victim {victimNickname} {victimUserId} {victimIpAddress} as {victimRole}...");
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
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} has been kicked for teamkilling " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
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
				Log.Info($"Adding Teamkiller entry for player #{playerId} {playerNickname} [{playerUserId}] [{playerIpAddress}]");
				Plugin.Instance.Teamkillers[playerUserId] = new Teamkiller(playerId, playerNickname, playerUserId, playerIpAddress);
			}
			//else
			//{
			//	if (Plugin.Config.OutAll)
			//	{
			//		Log.Info("Fetching Teamkiller entry for player #" + playerId + " " + playerNickname + " [" + playerUserId + "] [" + playerIpAddress + "]");
			//	}
			//}
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
				Log.Info("votetk > 0: " + Plugin.Instance.Config.VoteTk);
				Log.Info("Teamkiller count is greater than votetk? " + this.Teamkillers[killerUserId].Teamkills.Count);
				Log.Info("Teamkiller is immune? " + this.isImmune(killer));
			}
			if (Plugin.Instance.Config.VoteTk > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.VoteTk && !this.isImmune(killer))
			{
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} is being voted on a ban for teamkilling {this.Teamkillers[killerUserId].Teamkills.Count} times.");
				Dictionary<int, string> options = new Dictionary<int, string>();
				options[1] = "Yes";
				options[2] = "No";
				HashSet<string> votes = new HashSet<string>();
				Dictionary<int, int> counter = new Dictionary<int, int>();

				/*if (Voting != null && StartVote != null && !Voting.Invoke())
				{
					//Plugin.Instance.InvokeEvent("OnStartVote", $"Ban {killerNickname}?", options, votes, counter);
					Log.Info($"Running vote:  Ban {killerNickname}?");
					this.StartVote.Invoke($"Ban {killerNickname}?", options, votes, counter);
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
