using System;
using System.Collections.Generic;
using System.Linq;
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
			this.plugin.ProcessingDisconnect = false;
			this.plugin.DuringRound = true;
			// Remove if teamkills can be removed for memory cleanup
			/**
			 * MEMORY CLEANUP
			 */
			this.plugin.Teamkillers = new Dictionary<string, Teamkiller>();
			this.plugin.TeamkillTimers = new Dictionary<string, Timer>();
		}

		public void OnRoundEnd()
		{
			//if (EventPlugin.GetRoundDuration() >= 3)
			//{
			this.plugin.DuringRound = false;
			//}

			foreach (Timer timer in this.plugin.TeamkillTimers.Values)
			{
				timer.Dispose();
			}
			if (this.plugin.system == 3)
			{
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
				//foreach (Teamkiller teamkiller in this.plugin.Teamkillers)
				//{
				//
				//}
			}


			/*
			 * MEMORY CLEANUP
			 */
			this.plugin.Teamkillers = new Dictionary<string, Teamkiller>();
			this.plugin.TeamkillTimers = new Dictionary<string, Timer>();
			this.plugin.ProcessingDisconnect = false;
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
		}

		public void OnPlayerLeave(PlayerLeaveEvent ev)
		{
			if (this.plugin.enable && this.plugin.system == 3 && this.plugin.DuringRound && !this.plugin.ProcessingDisconnect)
			{
				this.plugin.ProcessingDisconnect = true;

				List<Teamkiller> disconnectedUsers = this.plugin.Teamkillers.Values.Where(tker => !Player.GetHubs().Any(p => tker.UserId == Player.GetUserId(p))).ToList();

				foreach (Teamkiller teamkiller in disconnectedUsers)
				{
					if (this.plugin.Teamkillers.ContainsKey(teamkiller.UserId) && this.plugin.Teamkillers[teamkiller.UserId].Teamkills.Count > 0)
					{
						int teamkills = this.plugin.Teamkillers[teamkiller.UserId].Teamkills.Count;
						if (this.plugin.outall)
						{
							Log.Info("Player " + teamkiller.Name + " has committed " + teamkills + " teamkills.");
						}

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
					else
					{
						if (this.plugin.outall)
						{
							Log.Info("Player " + teamkiller.UserId + " has committed no teamkills.");
						}
					}

					this.plugin.Teamkillers.Remove(teamkiller.UserId);
				}

				this.plugin.ProcessingDisconnect = false;
			}
		}

		public void OnPlayerDeath(ref PlayerDeathEvent ev)
		{

			ReferenceHub killer = ev.Killer;
			string killerNickname = Player.GetNickname(killer);
			string killerUserId = Player.GetUserId(killer);
			string killerIpAddress = Player.GetIpAddress(killer);
			string killerOutput = killerNickname + " " + killerUserId + " " + killerIpAddress;
			ReferenceHub victim = ev.Player;
			string victimNickname = Player.GetNickname(victim);
			string victimUserId = Player.GetUserId(victim);
			string victimIpAddress = Player.GetIpAddress(victim);
			string victimOutput = victimNickname + " " + victimUserId + " " + victimIpAddress;

			if (!this.plugin.DuringRound)
			{
				Log.Info("Skipping OnPlayerDie " + killerOutput + " killed " + victimOutput + " for being outside of a round.");
				return;
			}

			if (this.plugin.enable)
			{
				if (!this.plugin.Teamkillers.ContainsKey(victimUserId))
				{
					this.plugin.Teamkillers[victimUserId] = new Teamkiller(Player.GetPlayerId(victim), Player.GetNickname(victim), victimUserId, Player.GetIpAddress(victim));
				}
				this.plugin.Teamkillers[victimUserId].Deaths++;

				if (!this.plugin.Teamkillers.ContainsKey(killerUserId))
				{
					this.plugin.Teamkillers[killerUserId] = new Teamkiller(Player.GetPlayerId(killer), Player.GetNickname(killer), killerUserId, Player.GetIpAddress(killer));
				}

				if (this.plugin.isTeamkill(killer, victim))
				{
					this.plugin.Teamkillers[killerUserId].Kills--;

					Teamkill teamkill = new Teamkill(Player.GetNickname(killer), killerUserId, Player.GetRole(killer), Player.GetNickname(victim), victimUserId, Player.GetRole(victim), Player.IsHandCuffed(victim), ev.Info.GetDamageType(), (int) EventPlugin.GetRoundDuration());
					this.plugin.TeamkillVictims[Player.GetUserId(ev.Player)] = teamkill;
					this.plugin.Teamkillers[killerUserId].Teamkills.Add(teamkill);

					Log.Info("Player " + killerOutput + " " + Player.GetTeam(killer).ToString() + " teamkilled " +
						victimOutput + " " + Player.GetTeam(victim).ToString() + ", for a total of " + this.plugin.Teamkillers[killerUserId].Teamkills.Count + " teamkills.");

					Player.Broadcast(victim, 10, string.Format(this.plugin.GetTranslation("victim_message"), Player.GetNickname(killer), DateTime.Today.ToString("yyyy-MM-dd hh:mm tt")), false);

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
							Player.Broadcast(killer, 5, string.Format(this.plugin.GetTranslation("killer_kdr_message"), Player.GetNickname(victim), teamkill.GetRoleDisplay(), kdr), false);
							return;
						}
						else if (this.plugin.warntk != -1)
						{
							string broadcast = string.Format(this.plugin.GetTranslation("killer_message"), Player.GetNickname(victim), teamkill.GetRoleDisplay()) + " ";
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
						this.plugin.OnBan(killer, Player.GetNickname(killer), this.plugin.length, this.plugin.Teamkillers[killerUserId].Teamkills);
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
											this.plugin.OnBan(killer, Player.GetNickname(killer), banLength, this.plugin.Teamkillers[killerUserId].Teamkills);
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
										Log.Info("Player " + killerOutput + " " + Player.GetTeam(killer).ToString() + " teamkill expired, counter now at " + this.plugin.Teamkillers[killerUserId].Teamkills.Count + ".");
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
							this.plugin.OnBan(killer, Player.GetNickname(killer), this.plugin.length, this.plugin.Teamkillers[killerUserId].Teamkills);
						}
					}
				}
				else
				{
					if (this.plugin.outall)
					{
						Log.Info("Player " + killerOutput + " " + Player.GetTeam(killer).ToString() + " killed " +
							victimOutput + " " + Player.GetTeam(victim).ToString() + " and it was not detected as a teamkill.");
					}

					this.plugin.Teamkillers[killerUserId].Kills++;
				}
			}
			else
			{
				Log.Info("Player " + killerOutput + " " + Player.GetTeam(killer).ToString() + " killed " +
					victimOutput + " " + Player.GetTeam(victim).ToString() + ".");
			}
		}

		public void OnPlayerHurt(ref PlayerHurtEvent ev)
		{
			if (this.plugin.enable)
			{
				ReferenceHub attacker = ev.Attacker;
				ReferenceHub player = ev.Player;
				if (this.plugin.mirror > 0f && ev.DamageType != DamageTypes.Grenade && ev.DamageType != DamageTypes.Falldown)
				{
					if (this.plugin.isTeamkill(ev.Attacker, ev.Player) && !this.plugin.isImmune(ev.Attacker) && !this.plugin.banWhitelist.Contains(Player.GetUserId(attacker)))
					{
						if (this.plugin.invert > 0)
						{
							if (this.plugin.Teamkillers.ContainsKey(Player.GetUserId(ev.Attacker)) && this.plugin.Teamkillers[Player.GetUserId(ev.Attacker)].Teamkills.Count >= this.plugin.invert)
							{
								//if (this.plugin.outall)
								//{
								//	Log.Info("Dealing damage to " + ev.Attacker.Name + ": " + (ev.Damage * this.plugin.mirror));
								//}
								attacker.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Amount * this.plugin.mirror, Player.GetNickname(attacker), DamageTypes.Falldown, Player.GetPlayerId(attacker)), attacker.gameObject);
							}
							// else do nothing
						}
						else
						{
							//if (this.plugin.outall)
							//{
							//	Log.Info("Dealing damage to " + ev.Attacker.Name + ": " + (ev.Damage * this.plugin.mirror));
							//}
							attacker.playerStats.HurtPlayer(new PlayerStats.HitInfo(ev.Amount * this.plugin.mirror, Player.GetNickname(attacker), DamageTypes.Falldown, Player.GetPlayerId(attacker)), attacker.gameObject);
						}
					}
				}
				else if (Player.GetPlayerId(ev.Player) == Player.GetPlayerId(ev.Attacker) && ev.DamageType == DamageTypes.Grenade)
				{
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
							//Player.AddHealth(attacker, damage * -1, DamageTypes.Falldown);
							attacker.playerStats.HurtPlayer(new PlayerStats.HitInfo(damage, Player.GetNickname(attacker), DamageTypes.Falldown, Player.GetPlayerId(attacker)), attacker.gameObject);
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

		public void OnConsoleCommand(ConsoleCommandEvent ev)
		{

		}
	}
}
