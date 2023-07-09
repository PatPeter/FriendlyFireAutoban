using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using MEC;
using static BanHandler;
using FriendlyFireAutoban;
using PlayerRoles;
using PluginAPI.Core;
using Mirror;
using PluginAPI.Core.Attributes;
using PlayerStatsSystem;
using PluginAPI.Events;
using InventorySystem.Items.Pickups;
using PluginAPI.Core.Items;
using PluginAPI.Enums;
using PlayerRoles.PlayableScps.Scp939;
using System.Runtime.CompilerServices;
using static RoundSummary;

namespace FriendlyFireAutoban
{
	internal class EventHandlers
	{
		internal Plugin plugin;
		internal EventHandlers(Plugin plugin) => this.plugin = plugin;

        [PluginEvent(PluginAPI.Enums.ServerEventType.RoundStart)]
        public void OnRoundStart()
		{
			Log.Info("Round has started.");
			Plugin.Instance.ProcessingDisconnect = false;
			Plugin.Instance.DuringRound = true;
			Plugin.Instance.FFAHandle = Timing.RunCoroutine(Plugin.Instance.FFACoRoutine());
			// Remove if teamkills can be removed for memory cleanup
			/**
			 * MEMORY CLEANUP
			 */
			//Plugin.Instance.Teamkillers = new ConcurrentDictionary<string, Teamkiller>();
			//Plugin.Instance.TeamkillTimers = new ConcurrentDictionary<string, Timer>();

			if (Plugin.Instance.Config.OutAll)
			{
				Plugin.Instance.PrintConfigs();
			}
		}

		[PluginEvent(PluginAPI.Enums.ServerEventType.RoundEnd)]
		public void OnRoundEnd(LeadingTeam leadingTeam)
		{
			//if (EventPlugin.GetRoundDuration() >= 3)
			//{
			//Log.Info("Set round end to false and check if ban system is #3 " + Plugin.Instance.Config.System);
			Plugin.Instance.DuringRound = false;
			Timing.KillCoroutines(Plugin.Instance.FFAHandle);
			//}

			//Log.Info("Disposing of teamkill timers at round end.");
			//foreach (Timer timer in Plugin.Instance.TeamkillTimers.Values)
			//{
			//	timer.Dispose();
			//}

			// Do all of this logic in the coroutine
			/*if (Plugin.Instance.Config.System == 3)
			{
				Log.Info("[SYSTEM 3] Iterating over players and issuing scaled bans on round end...");
				foreach (Player player in Player.List)
				{
					string playerUserId = player.UserId;
					string playerNickname = player.Nickname;
					if (Plugin.Instance.Teamkillers.ContainsKey(playerUserId))
					{
						int teamkills = Plugin.Instance.Teamkillers[playerUserId].Teamkills.Count;
						if (Plugin.Instance.Config.OutAll)
						{
							Log.Info("Player " + player.ToString() + " has committed " + teamkills + " teamkills.");
						}

						int banLength = Plugin.Instance.GetScaledBanAmount(playerUserId);
						if (banLength > 0)
						{
							Plugin.Instance.OnBan(player, playerNickname, banLength, Plugin.Instance.Teamkillers[playerUserId].Teamkills);
						}
						else
						{
							if (Plugin.Instance.Config.OutAll)
							{
								Log.Info("Player " + playerUserId + " " + Plugin.Instance.Teamkillers[playerUserId].Teamkills.Count + " teamkills is not bannable.");
							}
						}
					}
					else
					{
						if (Plugin.Instance.Config.OutAll)
						{
							Log.Info("Player " + playerNickname + " " + playerUserId + " has committed no teamkills.");
						}
					}
					Plugin.Instance.Teamkillers.Remove(playerUserId);
				}

				// TODO: If a player was not processed in OnDisconnect, 
				// then they will remain in the teamkillers array.
				// In the future, this array needs to be post-processed
				// to ensure that all teamkillers are banned.
				//foreach (KeyValuePair<string, Teamkiller> teamkiller in Plugin.Instance.Teamkillers)
				//{
					// Wait won't disconnected users periodically clean up users?
					// Yes it will, this isn't needed
				//}
			}*/


			/*
			 * MEMORY CLEANUP
			 * 
			 * Do not wipe arrays between rounds to preserve teamkills, deal with memory leak in next version
			 */
			//Plugin.Instance.Teamkillers = new ConcurrentDictionary<string, Teamkiller>();
			//Plugin.Instance.TeamkillTimers = new ConcurrentDictionary<string, Timer>();
			//Plugin.Instance.ProcessingDisconnect = false;
		}

		[PluginEvent(PluginAPI.Enums.ServerEventType.PlayerJoined)]
        public void OnPlayerVerified(Player player)
		{
			Teamkiller teamkiller = Plugin.Instance.AddAndGetTeamkiller(player);
			if (teamkiller != null)
			{
				teamkiller.Banned = false;
				teamkiller.Disconnected = false;
			}
		}

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerLeft)]
        public void OnPlayerDestroying(Player player)
		{
			Log.Info("[OnPlayerLeave] Triggered, Enabled: " + Plugin.Instance.Config.IsEnabled + ", during round: " + Plugin.Instance.DuringRound + ", processing player leave: " + Plugin.Instance.ProcessingDisconnect);
			// Flag player as a disconnected user
			Plugin.Instance.Teamkillers.Values.Where(tker => tker.UserId == player.UserId).First().Disconnected = true;

			/*if (Plugin.Instance.Config.IsEnabled && // plugin must be enabled
				Plugin.Instance.DuringRound && // must be during round, otherwise will process on 20+ player leaves on round restart
				!Plugin.Instance.ProcessingDisconnect // mutual exclusion lock
			) {
				Plugin.Instance.ProcessingDisconnect = true;

				List<Teamkiller> disconnectedUsers = Plugin.Instance.Teamkillers.Values.Where(tker => !Player.List.Any(p => tker.UserId == p.UserId)).ToList();

				foreach (Teamkiller teamkiller in disconnectedUsers)
				{
					// This should never occur because we are in mutual exclusion and list was obtained from Plugin.Instance.Teamkillers
					if (Plugin.Instance.Teamkillers.ContainsKey(teamkiller.UserId)) {
						if (Plugin.Instance.Teamkillers[teamkiller.UserId].Teamkills.Count > 0)
						{
							int teamkills = Plugin.Instance.Teamkillers[teamkiller.UserId].Teamkills.Count;
							if (Plugin.Instance.Config.OutAll)
							{
								Log.Info("Player " + teamkiller.Nickname + " that committed " + teamkills + " teamkills has left the server.");
							}

							// Only issued scaled bans for system #3
							if (Plugin.Instance.Config.System == 3)
							{
								int banLength = Plugin.Instance.GetScaledBanAmount(teamkiller.UserId);
								if (banLength > 0)
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
									Log.Info(teamkiller.Nickname + " / " + teamkiller.UserId + ": Banned " + banLength + " minutes for teamkilling " + teamkills + " players");

									BanDetails ipBan = new BanDetails();
									ipBan.OriginalName = teamkiller.Nickname;
									ipBan.Id = teamkiller.IPAddress;
									// Calculate ticks
									ipBan.Expires = now + (banLength * 60 * 10000000);
									ipBan.Reason = string.Format(Plugin.Instance.GetTranslation("offline_ban"), banLength, teamkills);
									ipBan.Issuer = "FriendlyFireAutoban";
									ipBan.IssuanceTime = now;
									BanHandler.IssueBan(ipBan, BanType.IP);
									Log.Info(teamkiller.Nickname + " / " + teamkiller.IPAddress + ": Banned " + banLength + " minutes for teamkilling " + teamkills + " players");
								}
								else
								{
									if (Plugin.Instance.Config.OutAll)
									{
										Log.Info("Player " + teamkiller.Nickname + " " + Plugin.Instance.Teamkillers[teamkiller.UserId].Teamkills.Count + " teamkills is not bannable.");
									}
								}
							}
						}
						else
						{
							if (Plugin.Instance.Config.OutAll)
							{
								Log.Info("Player " + teamkiller.UserId + " has committed no teamkills, remove Teamkiller entry.");
							}

							// If a player has committed no teamkills, then remove the Teamkiller entry as it is no longer needed
							// TODO: Consider whether this breaks kdsafe...
							Plugin.Instance.Teamkillers.Remove(teamkiller.UserId);
						}
					}
				}

				Plugin.Instance.ProcessingDisconnect = false;
			}
			else
			{
				Log.Info("Not processing OnPlayerLeave");
			}*/
		}

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerDying)]
        public void OnPlayerDying(Player player, Player attacker, DamageHandlerBase damageHandler)
		{
			if (attacker == null || player == null)
			{
				return;
			}

			Player killer = attacker;
			int killerPlayerId = killer.PlayerId;
			string killerNickname = killer.Nickname;
			string killerUserId = killer.UserId;
			string killerIpAddress = killer.IpAddress;
			Team killerTeam = killer.Team;
			RoleTypeId killerRole = killer.Role;
			string killerOutput = killerNickname + " " + killerUserId + " " + killerIpAddress;

			Player victim = player;
			int victimPlayerId = victim.PlayerId;
			string victimNickname = victim.Nickname;
			string victimUserId = victim.UserId;
			string victimIpAddress = victim.IpAddress;
			Team victimTeam = victim.Team;
			RoleTypeId victimRole = victim.Role;
			bool victimIsHandcuffed = victim.IsDisarmed;
			string victimOutput = victimNickname + " " + victimUserId + " " + victimIpAddress;

			if (Plugin.Instance.Config.OutAll)
			{
				Log.Info(killerOutput + " killed " + victimOutput + " while plugin is enabled? " + Plugin.Instance.Config.IsEnabled + " and during round? " + Plugin.Instance.DuringRound);
			}

			if (!Plugin.Instance.DuringRound)
			{
				Log.Info("Skipping OnPlayerDie " + killerOutput + " killed " + victimOutput + " for being outside of a round.");
				return;
			}

			if (Plugin.Instance.Config.IsEnabled)
			{
				// Should be completely impossible, but does not hurt
				if (!Plugin.Instance.Teamkillers.ContainsKey(victimUserId))
				{
					Plugin.Instance.Teamkillers[victimUserId] = new Teamkiller(victimPlayerId, victimNickname, victimUserId, victimIpAddress);
				}

				Teamkiller victimTeamkiller = Plugin.Instance.Teamkillers[victimUserId];
				victimTeamkiller.Deaths++;

				// Should be completely impossible, but does not hurt
				if (!Plugin.Instance.Teamkillers.ContainsKey(killerUserId))
				{
					Plugin.Instance.Teamkillers[killerUserId] = new Teamkiller(killerPlayerId, killerNickname, killerUserId, killerIpAddress);
				}

				Teamkiller killerTeamkiller = Plugin.Instance.Teamkillers[killerUserId];

				if (Plugin.Instance.Config.OutAll) {
					Log.Info("Was this a teamkill? " + Plugin.Instance.IsTeamkill(killer, victim, true));
				}
				if (Plugin.Instance.IsTeamkill(killer, victim, true))
				{
					killerTeamkiller.Kills--;

					Teamkill teamkill = new Teamkill(
						DateTime.Now.Ticks, 
						killerNickname, 
						killerUserId, 
						(short)killerRole, 
						victimNickname, 
						victimUserId, 
						(short)victimRole, 
						victimIsHandcuffed, 
						0, 
						Round.Duration.Seconds
					);
					Plugin.Instance.TeamkillVictims[victimUserId] = teamkill;
					killerTeamkiller.Teamkills.Add(teamkill);

					// Not a debug log, do not add to OutAll
					Log.Info("Player " + killerOutput + " " + killerTeam.ToString() + " teamkilled " +
						victimOutput + " " + victimTeam.ToString() + ", for a total of " + killerTeamkiller.Teamkills.Count + " teamkills.");

					BroadcastUtil.PlayerBroadcast(victim, string.Format(Plugin.Instance.GetTranslation("victim_message"), killerNickname, DateTime.Now.ToString("yyyy-MM-dd hh:mm tt")), 10);

					if (!Plugin.Instance.BanWhitelist.Contains(killerUserId))
					{
						float kdr = killerTeamkiller.GetKDR();
						// If kdr is greater than K/D safe amount, AND if the number of kils is greater than kdsafe to exclude low K/D values
						if (Plugin.Instance.Config.OutAll)
						{
							Log.Info("kdsafe set to: " + Plugin.Instance.Config.KdSafe);
							Log.Info("Player " + killerOutput + " KDR: " + kdr);
							Log.Info("Is KDR greater than kdsafe? " + (kdr > (float)Plugin.Instance.Config.KdSafe));
							Log.Info("Are kills greater than kdsafe? " + (killerTeamkiller.Kills > Plugin.Instance.Config.KdSafe));
						}

						if (Plugin.Instance.Config.KdSafe > 0 && kdr > (float)Plugin.Instance.Config.KdSafe && killerTeamkiller.Kills > Plugin.Instance.Config.KdSafe)
						{
							BroadcastUtil.PlayerBroadcast(killer, string.Format(Plugin.Instance.GetTranslation("killer_kdr_message"), victimNickname, teamkill.GetRoleDisplay(), kdr), 5);
							return;
						}
						else if (Plugin.Instance.Config.WarnTk != -1)
						{
							string broadcast = string.Format(Plugin.Instance.GetTranslation("killer_message"), victimNickname, teamkill.GetRoleDisplay()) + " ";
							if (Plugin.Instance.Config.WarnTk > 0)
							{
								int teamkillsBeforeBan = Plugin.Instance.Config.Amount - killerTeamkiller.Teamkills.Count;
								if (teamkillsBeforeBan <= Plugin.Instance.Config.WarnTk)
								{
									broadcast += string.Format(Plugin.Instance.GetTranslation("killer_warning"), teamkillsBeforeBan) + " ";
								}
							}
							else
							{
								broadcast += Plugin.Instance.GetTranslation("killer_request") + " ";
							}
							BroadcastUtil.PlayerBroadcast(killer, broadcast, 5);
						}

						Plugin.Instance.OnCheckRemoveGuns(killer);

						Plugin.Instance.OnCheckToSpectator(killer);

						Plugin.Instance.OnCheckUndead(killer, victim);

						Plugin.Instance.OnCheckKick(killer);

						//Plugin.Instance.OnVoteTeamkill(ev.Killer);

						/*
						 * If ban system is #1, do not create timers and perform a ban based on a static number of teamkills
						 */
						if (Plugin.Instance.Config.System == 1 && killerTeamkiller.Teamkills.Count >= Plugin.Instance.Config.Amount)
						{
							Plugin.Instance.OnBan(killerTeamkiller, killerNickname, Plugin.Instance.Config.Length);
						}
						else
						{
							// If the player has teamkilled again, reset the timer to the default
							// for both ban systems #2 and #3
							Log.Info("Set teamkiller " + killerTeamkiller + " forgiveness countdown to " + Plugin.Instance.Config.Expire);
							killerTeamkiller.TimerCountdown = Plugin.Instance.Config.Expire;

							/*
							 * If ban system is #2, allow the teamkills to expire,
							 * but if the player teamkills faster than kills can expire
							 * ban the player.
							 */
							if (Plugin.Instance.Config.System == 2 && killerTeamkiller.Teamkills.Count >= Plugin.Instance.Config.Amount)
							{
								Plugin.Instance.OnBan(killerTeamkiller, killerNickname, Plugin.Instance.Config.Length);
							}
						}
						return;
					}
					else
					{
						Log.Info("Player " + killerOutput + " not being punished by FFA because the player is whitelisted.");
						return;
					}
				}
				else
				{
					if (Plugin.Instance.Config.OutAll)
					{
						Log.Info("Player " + killerOutput + " " + killerTeam.ToString() + " killed " +
							victimOutput + " " + victimTeam.ToString() + " and it was not detected as a teamkill.");
					}

					killerTeamkiller.Kills++;
					return;
				}
			}
			else
			{
				Log.Info("Player " + killerOutput + " " + killerTeam.ToString() + " killed " +
					victimOutput + " " + victimTeam.ToString() + ", but FriendlyFireAutoban is not enabled.");
				return;
			}
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerDamage)]
        public void OnPlayerHurting(Player target, Player assaulter, DamageHandlerBase damageHandler)
		{
			if (assaulter == null || target == null)
			{
				return;
			}

			Player attacker = assaulter;
			int attackerPlayerId = attacker.PlayerId;
			String attackerUserId = attacker.UserId;
			String attackerNickname = attacker.Nickname;

			Player victim = target;
			int victimPlayerId = victim.PlayerId;

			UniversalDamageHandler universalDamageHandler = null;
			try
			{
				universalDamageHandler = damageHandler as UniversalDamageHandler;
			}
			catch
			{
				Log.Warning("Could not cast damageHandler as UniversalDamageHandler.");
			}

			AttackerDamageHandler attackerDamgeHandler = null;
			try
			{
				attackerDamgeHandler = damageHandler as AttackerDamageHandler;
			}
			catch
			{
				Log.Warning("Could not cast damageHandler as AttackerDamageHandler.");
			}

            if (damageHandler == null || universalDamageHandler == null || attackerDamgeHandler == null)
			{
				return;
			}

            if (Plugin.Instance.Config.Mirror > 0f && universalDamageHandler.TranslationId != DeathTranslations.Falldown.Id) // && ev.DamageType != DamageTypes.Grenade
			{
				//Log.Info("Mirroring " + ev.Amount + " of " + ev.DamageType.ToString() + " damage.");
				if (Plugin.Instance.IsTeamkill(attacker, victim, false) && !Plugin.Instance.isImmune(attacker) && !Plugin.Instance.BanWhitelist.Contains(attackerUserId))
				{
					if (Plugin.Instance.Config.Invert > 0)
					{
						if (Plugin.Instance.Teamkillers.ContainsKey(attackerUserId) && Plugin.Instance.Teamkillers[attackerUserId].Teamkills.Count >= Plugin.Instance.Config.Invert)
						{
							//if (Plugin.Instance.Config.OutAll)
							//{
							//	Log.Info("Dealing damage to " + attackerNickname + ": " + (ev.Amount * Plugin.Instance.Config.Mirror));
							//}
							//attacker.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Amount * Plugin.Instance.mirror, attackerNickname, DamageTypes.Falldown, attackerPlayerId), attacker.gameObject);
							Timing.CallDelayed(0.5f, () => attacker.Damage(attackerDamgeHandler.Damage * Plugin.Instance.Config.Mirror, DeathTranslations.Falldown.LogLabel));
						}
						// else do nothing
					}
					else
					{
						//if (Plugin.Instance.Config.OutAll)
						//{
						//	Log.Info("Dealing damage to " + attackerNickname + ": " + (ev.Amount * Plugin.Instance.Config.Mirror));
						//}
						//attacker.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Amount * Plugin.Instance.mirror, attackerNickname, DamageTypes.Falldown, attackerPlayerId), attacker.gameObject);
						Timing.CallDelayed(0.5f, () => attacker.Damage(attackerDamgeHandler.Damage * Plugin.Instance.Config.Mirror, DeathTranslations.Falldown.LogLabel));
					}
				}
			}
			else if (victimPlayerId == attackerPlayerId && universalDamageHandler.TranslationId != DeathTranslations.Explosion.Id)
			{
				if (Plugin.Instance.Config.OutAll)
				{
					Log.Info("[BOMBER] Player " + victimPlayerId + " damaged by his/her own grenade, bomber triggered.");
				}
				if (Plugin.Instance.Config.Bomber == 2)
				{
					int damage = (int)attackerDamgeHandler.Damage;
                    attackerDamgeHandler.Damage = 0;
					Timing.CallDelayed(0.5f, () => attacker.Damage(damage, DeathTranslations.Falldown.LogLabel));
				}
				else if (Plugin.Instance.Config.Bomber == 1)
				{
                    attackerDamgeHandler.Damage = 0;
				}
			}
		}

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerSpawn)]
        public void OnPlayerSpawn(Player spawnedPlayer, RoleTypeId role)
		{
			Player player = spawnedPlayer;
			if (player == null)
			{
				Log.Warning("[OnPlayerSpawn] Null player passed into event.");
				return;
			}

			if (Plugin.Instance.Config.IsEnabled)
			{
				// Every time a player respawns, if that player is not spectator, update team/role
				// Therefore when mirror/bomber are triggered, we can use the cached team/role
				Team playerTeam = player.Team;
				RoleTypeId playerRole = player.Role;

				Teamkiller teamkiller = Plugin.Instance.AddAndGetTeamkiller(player);
				if (teamkiller != null && playerTeam != Team.Dead)
				{
					teamkiller.Team = playerTeam;
					teamkiller.PlayerRole = playerRole;
				}

				Plugin.Instance.OnCheckRemoveGuns(player);
			}
		}

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerChangeRole)]
        public void OnSetClass(Player changeRolePlayer, PlayerRoleBase oldRole, RoleTypeId RoleTypeId, RoleChangeReason changeReason)
		{
			Player player = changeRolePlayer;
			if (player == null)
			{
				Log.Warning("[OnSetClass] Null player passed into event.");
				return;
			}

			if (Plugin.Instance.Config.IsEnabled)
			{
				// Every time a player respawns, if that player is not spectator, update team/role
				// Therefore when mirror/bomber are triggered, we can use the cached team/role
				Team playerTeam = player.Team;
				RoleTypeId playerRole = player.Role;

				Teamkiller teamkiller = Plugin.Instance.AddAndGetTeamkiller(player);
				if (teamkiller != null && playerTeam != Team.Dead)
				{
					teamkiller.Team = playerTeam;
					teamkiller.PlayerRole = playerRole;
				}

				Plugin.Instance.OnCheckRemoveGuns(player);
			}
		}

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerSearchPickup)]
        public void OnPickupItem(Player player, ItemPickupBase item)
		{
			if (Plugin.Instance.Config.IsEnabled)
			{
				Timing.CallDelayed(0.5f, () => Plugin.Instance.OnCheckRemoveGuns(player));
			}
		}

		/*public void OnRACommand(SendingRemoteAdminCommandEventArgs ev)
		{
			if ("FRIENDLY_FIRE_AUTOBAN_TOGGLE".Equals(ev.Name.ToUpper()))
			{
				CommandSender caller = ev.CommandSender;

				if (Plugin.Instance.Config.IsEnabled)
				{
					Plugin.Instance.Config.IsEnabled = false;
					Extensions.RAMessage(caller, Plugin.Instance.GetTranslation("toggle_disable"), true);
				}
				else
				{
					Plugin.Instance.Config.IsEnabled = true;
					Extensions.RAMessage(caller, Plugin.Instance.GetTranslation("toggle_enable"), true );
				}
			}
			// TODO: Add whitelist functionality back for global devs
		}*/
	}
}
