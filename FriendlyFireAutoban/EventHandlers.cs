using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using EXILED;
using EXILED.Extensions;
using Grenades;
using MEC;
using static BanHandler;

namespace FriendlyFireAutoban
{
	public class EventHandlers
	{
		public Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		public void OnRoundStart()
		{
			Log.Info("Round has started.");
			this.plugin.ProcessingDisconnect = false;
			this.plugin.DuringRound = true;
			// Remove if teamkills can be removed for memory cleanup
			/**
			 * MEMORY CLEANUP
			 */
			//this.plugin.Teamkillers = new Dictionary<string, Teamkiller>();
			//this.plugin.TeamkillTimers = new Dictionary<string, Timer>();
		}

		public void OnRoundEnd()
		{
			//if (EventPlugin.GetRoundDuration() >= 3)
			//{
			Log.Info("Set round end to false and check if ban system is #3 " + this.plugin.system);
			this.plugin.DuringRound = false;
			//}

			//Log.Info("Disposing of teamkill timers at round end.");
			//foreach (Timer timer in this.plugin.TeamkillTimers.Values)
			//{
			//	timer.Dispose();
			//}
			if (this.plugin.system == 3)
			{
				Log.Info("[SYSTEM 3] Iterating over players and issuing scaled bans on round end...");
				foreach (ReferenceHub player in Player.GetHubs())
				{
					string playerUserId = Player.GetUserId(player);
					string playerNickname = Player.GetNickname(player);
					if (this.plugin.Teamkillers.ContainsKey(playerUserId))
					{
						int teamkills = this.plugin.Teamkillers[playerUserId].Teamkills.Count;
						if (this.plugin.outall)
						{
							Log.Info("Player " + player.ToString() + " has committed " + teamkills + " teamkills.");
						}

						int banLength = this.plugin.GetScaledBanAmount(playerUserId);
						if (banLength > 0)
						{
							this.plugin.OnBan(player, playerNickname, banLength, this.plugin.Teamkillers[playerUserId].Teamkills);
						}
						else
						{
							if (this.plugin.outall)
							{
								Log.Info("Player " + playerUserId + " " + this.plugin.Teamkillers[playerUserId].Teamkills.Count + " teamkills is not bannable.");
							}
						}
					}
					else
					{
						if (this.plugin.outall)
						{
							Log.Info("Player " + playerNickname + " " + playerUserId + " has committed no teamkills.");
						}
					}
					this.plugin.Teamkillers.Remove(playerUserId);
				}

				// TODO: If a player was not processed in OnDisconnect, 
				// then they will remain in the teamkillers array.
				// In the future, this array needs to be post-processed
				// to ensure that all teamkillers are banned.
				//foreach (KeyValuePair<string, Teamkiller> teamkiller in this.plugin.Teamkillers)
				//{
					// Wait won't disconnected users periodically clean up users?
					// Yes it will, this isn't needed
				//}
			}


			/*
			 * MEMORY CLEANUP
			 * 
			 * Do not wipe arrays between rounds to preserve teamkills, deal with memory leak in next version
			 */
			//this.plugin.Teamkillers = new Dictionary<string, Teamkiller>();
			//this.plugin.TeamkillTimers = new Dictionary<string, Timer>();
			//this.plugin.ProcessingDisconnect = false;
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			ReferenceHub player = ev.Player;
			int playerId = Player.GetPlayerId(player);
			string playerNickname = Player.GetNickname(player);
			string playerUserId = Player.GetUserId(player);
			string playerIpAddress = Player.GetIpAddress(player);

			if (!this.plugin.Teamkillers.ContainsKey(playerUserId))
			{
				Log.Info("Adding Teamkiller entry for player #" + playerId + " " + playerNickname + " [" + playerUserId + "] [" + playerIpAddress + "]");
				this.plugin.Teamkillers[playerUserId] = new Teamkiller(playerId, playerNickname, playerUserId, playerIpAddress);
			}
			else
			{
				Log.Info("Player has rejoined the server #" + playerId + " " + playerNickname + " [" + playerUserId + "] [" + playerIpAddress + "]");
			}
		}

		public void OnPlayerLeave(PlayerLeaveEvent ev)
		{
			Log.Info("[OnPlayerLeave] Triggered, Enabled: " + this.plugin.enable + ", during round: " + this.plugin.DuringRound + ", processing player leave: " + this.plugin.ProcessingDisconnect);
			if (this.plugin.enable && // plugin must be enabled
				this.plugin.DuringRound && // must be during round, otherwise will process on 20+ player leaves on round restart
				!this.plugin.ProcessingDisconnect // mutual exclusion lock
			) {
				this.plugin.ProcessingDisconnect = true;

				List<Teamkiller> disconnectedUsers = this.plugin.Teamkillers.Values.Where(tker => !Player.GetHubs().Any(p => tker.UserId == Player.GetUserId(p))).ToList();

				foreach (Teamkiller teamkiller in disconnectedUsers)
				{
					// This should never occur because we are in mutual exclusion and list was obtained from this.plugin.Teamkillers
					if (this.plugin.Teamkillers.ContainsKey(teamkiller.UserId)) {
						if (this.plugin.Teamkillers[teamkiller.UserId].Teamkills.Count > 0)
						{
							int teamkills = this.plugin.Teamkillers[teamkiller.UserId].Teamkills.Count;
							if (this.plugin.outall)
							{
								Log.Info("Player " + teamkiller.Name + " that committed " + teamkills + " teamkills has left the server.");
							}

							// Only issued scaled bans for system #3
							if (this.plugin.system == 3)
							{
								int banLength = this.plugin.GetScaledBanAmount(teamkiller.UserId);
								if (banLength > 0)
								{
									long now = DateTime.Now.Ticks;

									BanDetails userBan = new BanDetails();
									userBan.OriginalName = teamkiller.Name;
									userBan.Id = teamkiller.UserId;
									// Calculate ticks
									userBan.Expires = now + (banLength * 60 * 10000000);
									userBan.Reason = string.Format(this.plugin.GetTranslation("offline_ban"), banLength, teamkills);
									userBan.Issuer = "FriendlyFireAutoban";
									userBan.IssuanceTime = now;
									BanHandler.IssueBan(userBan, BanType.UserId);
									Log.Info(teamkiller.Name + " / " + teamkiller.UserId + ": Banned " + banLength + " minutes for teamkilling " + teamkills + " players");

									BanDetails ipBan = new BanDetails();
									ipBan.OriginalName = teamkiller.Name;
									ipBan.Id = teamkiller.IpAddress;
									// Calculate ticks
									ipBan.Expires = now + (banLength * 60 * 10000000);
									ipBan.Reason = string.Format(this.plugin.GetTranslation("offline_ban"), banLength, teamkills);
									ipBan.Issuer = "FriendlyFireAutoban";
									ipBan.IssuanceTime = now;
									BanHandler.IssueBan(ipBan, BanType.IP);
									Log.Info(teamkiller.Name + " / " + teamkiller.IpAddress + ": Banned " + banLength + " minutes for teamkilling " + teamkills + " players");
								}
								else
								{
									if (this.plugin.outall)
									{
										Log.Info("Player " + teamkiller.Name + " " + this.plugin.Teamkillers[teamkiller.UserId].Teamkills.Count + " teamkills is not bannable.");
									}
								}
							}
						}
						else
						{
							if (this.plugin.outall)
							{
								Log.Info("Player " + teamkiller.UserId + " has committed no teamkills, remove Teamkiller entry.");
							}

							// If a player has committed no teamkills, then remove the Teamkiller entry as it is no longer needed
							// TODO: Consider whether this breaks kdsafe...
							this.plugin.Teamkillers.Remove(teamkiller.UserId);
						}
					}
				}

				this.plugin.ProcessingDisconnect = false;
			}
			else
			{
				Log.Info("Not processing OnPlayerLeave");
			}
		}

		public void OnPlayerDeath(ref PlayerDeathEvent ev)
		{
			ReferenceHub killer = ev.Killer;
			int killerPlayerId = Player.GetPlayerId(killer);
			string killerNickname = Player.GetNickname(killer);
			string killerUserId = Player.GetUserId(killer);
			string killerIpAddress = Player.GetIpAddress(killer);
			Team killerTeam = Player.GetTeam(killer);
			RoleType killerRole = Player.GetRole(killer);
			string killerOutput = killerNickname + " " + killerUserId + " " + killerIpAddress;
			ReferenceHub victim = ev.Player;
			int victimPlayerId = Player.GetPlayerId(victim);
			string victimNickname = Player.GetNickname(victim);
			string victimUserId = Player.GetUserId(victim);
			string victimIpAddress = Player.GetIpAddress(victim);
			Team victimTeam = Player.GetTeam(victim);
			RoleType victimRole = Player.GetRole(victim);
			bool victimIsHandcuffed = Player.IsHandCuffed(victim);
			string victimOutput = victimNickname + " " + victimUserId + " " + victimIpAddress;

			Log.Info(killerOutput + " killed " + victimOutput + " while plugin is enabled? " + this.plugin.enable + " and during round? " + this.plugin.DuringRound);

			if (!this.plugin.DuringRound)
			{
				Log.Info("Skipping OnPlayerDie " + killerOutput + " killed " + victimOutput + " for being outside of a round.");
				return;
			}

			if (this.plugin.enable)
			{
				// Should be completely impossible, but does not hurt
				if (!this.plugin.Teamkillers.ContainsKey(victimUserId))
				{
					this.plugin.Teamkillers[victimUserId] = new Teamkiller(victimPlayerId, victimNickname, victimUserId, victimIpAddress);
				}
				this.plugin.Teamkillers[victimUserId].Deaths++;

				// Should be completely impossible, but does not hurt
				if (!this.plugin.Teamkillers.ContainsKey(killerUserId))
				{
					this.plugin.Teamkillers[killerUserId] = new Teamkiller(killerPlayerId, killerNickname, killerUserId, killerIpAddress);
				}

				Log.Info("Was this a teamkill? " + this.plugin.isTeamkill(killer, victim));
				if (this.plugin.isTeamkill(killer, victim))
				{
					this.plugin.Teamkillers[killerUserId].Kills--;

					Teamkill teamkill = new Teamkill(killerNickname, killerUserId, killerRole, victimNickname, victimUserId, victimRole, victimIsHandcuffed, ev.Info.GetDamageType(), (int) EventPlugin.GetRoundDuration());
					this.plugin.TeamkillVictims[victimUserId] = teamkill;
					this.plugin.Teamkillers[killerUserId].Teamkills.Add(teamkill);

					Log.Info("Player " + killerOutput + " " + killerTeam.ToString() + " teamkilled " +
						victimOutput + " " + victimTeam.ToString() + ", for a total of " + this.plugin.Teamkillers[killerUserId].Teamkills.Count + " teamkills.");

					Player.Broadcast(victim, 10, string.Format(this.plugin.GetTranslation("victim_message"), killerNickname, DateTime.Today.ToString("yyyy-MM-dd hh:mm tt")), false);

					if (!this.plugin.banWhitelist.Contains(killerUserId))
					{
						float kdr = this.plugin.Teamkillers[killerUserId].GetKDR();
						// If kdr is greater than K/D safe amount, AND if the number of kils is greater than kdsafe to exclude low K/D values
						if (this.plugin.outall)
						{
							Log.Info("kdsafe set to: " + this.plugin.kdsafe);
							Log.Info("Player " + killerOutput + " KDR: " + kdr);
							Log.Info("Is KDR greater than kdsafe? " + (kdr > (float)this.plugin.kdsafe));
							Log.Info("Are kills greater than kdsafe? " + (this.plugin.Teamkillers[killerUserId].Kills > this.plugin.kdsafe));
						}

						if (this.plugin.kdsafe > 0 && kdr > (float)this.plugin.kdsafe && this.plugin.Teamkillers[killerUserId].Kills > this.plugin.kdsafe)
						{
							Player.Broadcast(killer, 5, string.Format(this.plugin.GetTranslation("killer_kdr_message"), victimNickname, teamkill.GetRoleDisplay(), kdr), false);
							return;
						}
						else if (this.plugin.warntk != -1)
						{
							string broadcast = string.Format(this.plugin.GetTranslation("killer_message"), victimNickname, teamkill.GetRoleDisplay()) + " ";
							if (this.plugin.warntk > 0)
							{
								int teamkillsBeforeBan = this.plugin.amount - this.plugin.Teamkillers[killerUserId].Teamkills.Count;
								if (teamkillsBeforeBan <= this.plugin.warntk)
								{
									broadcast += string.Format(this.plugin.GetTranslation("killer_warning"), teamkillsBeforeBan) + " ";
								}
							}
							else
							{
								broadcast += this.plugin.GetTranslation("killer_request") + " ";
							}
							Player.Broadcast(killer, 5, broadcast, false);
						}

						this.plugin.OnCheckRemoveGuns(ev.Killer);

						this.plugin.OnCheckToSpectator(ev.Killer);

						this.plugin.OnCheckUndead(ev.Killer, ev.Player);

						this.plugin.OnCheckKick(ev.Killer);

						//this.plugin.OnVoteTeamkill(ev.Killer);

					}
					else
					{
						Log.Info("Player " + killerOutput + " not being punished by FFA because the player is whitelisted.");
						return;
					}

					/*
					 * If ban system is #1, do not create timers and perform a ban based on a static number of teamkills
					 */
					if (this.plugin.system == 1 && this.plugin.Teamkillers[killerUserId].Teamkills.Count >= this.plugin.amount)
					{
						this.plugin.OnBan(killer, killerNickname, this.plugin.length, this.plugin.Teamkillers[killerUserId].Teamkills);
					}
					else
					{
						Timer t;
						if (this.plugin.TeamkillTimers.ContainsKey(killerUserId))
						{
							/*
							 * If ban system is #3, allow the player to continue teamkilling
							 */
							t = this.plugin.TeamkillTimers[killerUserId];
							t.Stop();
							t.Interval = this.plugin.expire * 1000;
							t.Start();
						}
						else
						{
							t = new Timer
							{
								Interval = this.plugin.expire * 1000,
								AutoReset = true,
								Enabled = true
							};
							t.Elapsed += delegate
							{
								if (this.plugin.enable)
								{
									/*
									 * If ban system is #3, every player teamkill cancels and restarts the timer
									 * Wait until the timer expires after the teamkilling has ended to find out 
									 * how much teamkilling the player has done.
									 */
									if (this.plugin.system == 3)
									{
										int banLength = this.plugin.GetScaledBanAmount(killerUserId);
										if (banLength > 0)
										{
											this.plugin.OnBan(killer, killerNickname, banLength, this.plugin.Teamkillers[killerUserId].Teamkills);
										}
										else
										{
											if (this.plugin.outall)
											{
												Log.Info("Player " + killerUserId + " " + this.plugin.Teamkillers[killerUserId].Teamkills.Count + " teamkills is not bannable.");
											}
										}
									}

									if (this.plugin.Teamkillers[killerUserId].Teamkills.Count > 0)
									{
										Teamkill firstTeamkill = this.plugin.Teamkillers[killerUserId].Teamkills[0];
										this.plugin.Teamkillers[killerUserId].Teamkills.RemoveAt(0);
										Log.Info("Player " + killerOutput + " " + killerTeam.ToString() + " teamkill expired, counter now at " + this.plugin.Teamkillers[killerUserId].Teamkills.Count + ".");
									}
									else
									{
										t.Enabled = false;
									}
								}
								else
								{
									t.Enabled = false;
								}
							};
							this.plugin.TeamkillTimers[killerUserId] = t;
						}

						/*
						 * If ban system is #2, allow the teamkills to expire
						 */
						if (this.plugin.system == 2 && this.plugin.Teamkillers[killerUserId].Teamkills.Count >= this.plugin.amount)
						{
							t.Stop();
							this.plugin.OnBan(killer, killerNickname, this.plugin.length, this.plugin.Teamkillers[killerUserId].Teamkills);
						}
					}
				}
				else
				{
					if (this.plugin.outall)
					{
						Log.Info("Player " + killerOutput + " " + killerTeam.ToString() + " killed " +
							victimOutput + " " + victimTeam.ToString() + " and it was not detected as a teamkill.");
					}

					this.plugin.Teamkillers[killerUserId].Kills++;
				}
			}
			else
			{
				Log.Info("Player " + killerOutput + " " + killerTeam.ToString() + " killed " +
					victimOutput + " " + victimTeam.ToString() + ", but FriendlyFireAutoban is not enabled.");
			}
		}

		public void OnPlayerHurt(ref PlayerHurtEvent ev)
		{
			if (this.plugin.enable)
			{
				ReferenceHub attacker = ev.Attacker;
				int attackerPlayerId = Player.GetPlayerId(ev.Attacker);
				String attackerUserId = Player.GetUserId(ev.Attacker);
				String attackerNickname = Player.GetNickname(attacker);

				ReferenceHub victim = ev.Player;
				int victimPlayerId = Player.GetPlayerId(victim);

				if (this.plugin.mirror > 0f && ev.DamageType != DamageTypes.Grenade && ev.DamageType != DamageTypes.Falldown)
				{
					Log.Info("Mirroring " + ev.Amount + " damage.");
					if (this.plugin.isTeamkill(attacker, victim) && !this.plugin.isImmune(attacker) && !this.plugin.banWhitelist.Contains(attackerUserId))
					{
						if (this.plugin.invert > 0)
						{
							if (this.plugin.Teamkillers.ContainsKey(attackerUserId) && this.plugin.Teamkillers[attackerUserId].Teamkills.Count >= this.plugin.invert)
							{
								if (this.plugin.outall)
								{
									Log.Info("Dealing damage to " + attackerNickname + ": " + (ev.Amount * this.plugin.mirror));
								}
								attacker.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Amount * this.plugin.mirror, attackerNickname, DamageTypes.Falldown, attackerPlayerId), attacker.gameObject);
							}
							// else do nothing
						}
						else
						{
							if (this.plugin.outall)
							{
								Log.Info("Dealing damage to " + attackerNickname + ": " + (ev.Amount * this.plugin.mirror));
							}
							attacker.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Amount * this.plugin.mirror, attackerNickname, DamageTypes.Falldown, attackerPlayerId), attacker.gameObject);
						}
					}
				}
				else if (victimPlayerId == attackerPlayerId && ev.DamageType == DamageTypes.Grenade)
				{
					if (this.plugin.outall)
					{
						Log.Info("[BOMBER] Player " + victimPlayerId + " damaged by his/her own grenade, bomber triggered.");
					}
					if (this.plugin.bomber == 2)
					{
						int damage = (int)ev.Amount;
						ev.Amount = 0;
						Timer t = new Timer
						{
							Interval = 1000,
							Enabled = true
						};
						t.Elapsed += delegate
						{
							if (this.plugin.outall)
							{
								Log.Info("[BOMBER] Player " + attackerPlayerId + " taking " + damage + " delayed damage after throwing a grenade.");
							}
							//Player.AddHealth(attacker, damage * -1, DamageTypes.Falldown);
							attacker.playerStats.HurtPlayer(new PlayerStats.HitInfo(damage, attackerNickname, DamageTypes.Falldown, attackerPlayerId), attacker.gameObject);
							t.Dispose();
						};
					}
					else if (this.plugin.bomber == 1)
					{
						ev.Amount = 0;
					}
				}
			}
		}

		public void OnPlayerSpawn(PlayerSpawnEvent ev)
		{
			if (this.plugin.enable)
			{
				this.plugin.OnCheckRemoveGuns(ev.Player);
			}
		}

		public void OnSetClass(SetClassEvent ev)
		{
			if (this.plugin.enable)
			{
				this.plugin.OnCheckRemoveGuns(ev.Player);
			}
		}

		public void OnPickupItem(ref PickupItemEvent ev)
		{
			if (this.plugin.enable)
			{
				this.plugin.OnCheckRemoveGuns(ev.Player);
			}
		}

		public void OnRACommand(ref RACommandEvent ev)
		{
			if ("FRIENDLY_FIRE_AUTOBAN_TOGGLE".Equals(ev.Command.ToUpper()))
			{
				CommandSender caller = ev.Sender;

				if (this.plugin.enable)
				{
					this.plugin.enable = false;
					Extensions.RAMessage(caller, this.plugin.GetTranslation("toggle_disable"), true);
				}
				else
				{
					this.plugin.enable = true;
					Extensions.RAMessage(caller, this.plugin.GetTranslation("toggle_enable"), true );
				}
			}
			// TODO: Add whitelist functionality back for global devs
		}

		public void OnConsoleCommand(ConsoleCommandEvent ev)
		{
			string command = ev.Command.Split(' ')[0];
			string[] quotedArgs = Regex.Matches(ev.Command, "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'")
				.Cast<Match>()
				.Select(m => {
					return Regex.Replace(Regex.Replace(m.Value, "^\'([^\']*)\'$", "$1"), "^\"([^\"]*)\"$", "$1");
				})
				.ToArray()
				.Skip(1)
				.ToArray();
			ReferenceHub player = ev.Player;
			String playerUserId = Player.GetUserId(player);

			if (this.plugin.outall)
			{
				Log.Info("Quoted Args for command: " + string.Join(" | ", quotedArgs));
			}

			if (command.Equals(this.plugin.GetTranslation("forgive_command")))
			{
				if (this.plugin.enable)
				{
					if (this.plugin.TeamkillVictims.ContainsKey(playerUserId) &&
						this.plugin.TeamkillVictims[playerUserId] != null)
					{
						Teamkill teamkill = this.plugin.TeamkillVictims[playerUserId];
						if (this.plugin.Teamkillers.ContainsKey(teamkill.KillerUserId))
						{
							int removedBans = this.plugin.Teamkillers[teamkill.KillerUserId].Teamkills.RemoveAll(x => x.Equals(teamkill));
							if (removedBans > 0)
							{
								// No need for broadcast with return message
								//ev.Player.PersonalBroadcast(5, "You forgave this player.", false);
								// TODO: Send a broadcast to the killer
								ev.ReturnMessage = string.Format(this.plugin.GetTranslation("forgive_success"), teamkill.KillerName, teamkill.GetRoleDisplay());
							}
							else
							{
								ev.ReturnMessage = string.Format(this.plugin.GetTranslation("forgive_duplicate"), teamkill.KillerName, teamkill.GetRoleDisplay());
							}
						}
						else
						{
							ev.ReturnMessage = this.plugin.GetTranslation("forgive_disconnect");
						}

						// No matter what, remove this teamkill cached in the array
						this.plugin.TeamkillVictims.Remove(playerUserId);
					}
					else
					{
						ev.ReturnMessage = this.plugin.GetTranslation("forgive_invalid");
					}
				}
				else
				{
					ev.ReturnMessage = this.plugin.GetTranslation("ffa_disabled");
				}
			}
			else if (command.Equals(this.plugin.GetTranslation("tks_command")))
			{
				if (this.plugin.enable)
				{
					if (quotedArgs.Length == 1)
					{
						List<Teamkiller> teamkillers = new List<Teamkiller>();
						try
						{
							if (Regex.Match(quotedArgs[0], "^[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]$").Success)
							{
								// https://stackoverflow.com/questions/55436309/how-do-i-use-linq-to-select-from-a-list-inside-a-map
								teamkillers = this.plugin.Teamkillers.Values.Where(
									x => x.UserId.Equals(quotedArgs[0])
								).ToList();
							}
							else
							{
								// https://stackoverflow.com/questions/55436309/how-do-i-use-linq-to-select-from-a-list-inside-a-map
								teamkillers = this.plugin.Teamkillers.Values.Where(
									x => x.Name.Contains(quotedArgs[0])
								).ToList();
							}
						}
						catch (Exception e)
						{
							if (this.plugin.outall)
							{
								Log.Error(e.Message);
								Log.Error(e.StackTrace);
							}
						}

						if (teamkillers.Count == 1)
						{
							string retval = "Player " + teamkillers[0].Name + " has a K/D ratio of " + teamkillers[0].Kills + ":" + teamkillers[0].Deaths + " or " + teamkillers[0].GetKDR() + ".\n";
							foreach (Teamkill tk in teamkillers[0].Teamkills)
							{
								retval +=
									string.Format(
										this.plugin.GetTranslation("tks_teamkill_entry"),
										(tk.Duration / 60) + ":" + (tk.Duration % 60),
										tk.KillerName,
										tk.VictimName,
										tk.GetRoleDisplay()
									) + "\n";
							}
							ev.ReturnMessage = retval;
						}
						else
						{
							ev.ReturnMessage = this.plugin.GetTranslation("tks_no_teamkills");
						}
					}
					else
					{
						ev.ReturnMessage = this.plugin.GetTranslation("tks_not_found");
					}
				}
				else
				{
					ev.ReturnMessage = this.plugin.GetTranslation("ffa_disabled");
				}
			}
		}
	}
}
