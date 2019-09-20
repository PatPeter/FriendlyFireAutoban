using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Timers;
using System;

namespace FriendlyFireAutoban.EventHandlers
{
    class RoundStartHandler : IEventHandlerRoundStart
	{
		private FriendlyFireAutobanPlugin plugin;

		public RoundStartHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin) plugin;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			this.plugin.ProcessingDisconnect = false;
			this.plugin.DuringRound = true;
			// Remove if teamkills can be removed for memory cleanup
			/**
			 * MEMORY CLEANUP
			 */
			this.plugin.Teamkillers = new Dictionary<string, Teamkiller>();
			this.plugin.TeamkillTimers = new Dictionary<string, Timer>();

			this.plugin.enable = this.plugin.GetConfigBool("friendly_fire_autoban_enable");
			this.plugin.outall = this.plugin.GetConfigBool("friendly_fire_autoban_outall");
			this.plugin.system = this.plugin.GetConfigInt("friendly_fire_autoban_system");

			this.plugin.matrix = new List<TeamTuple>();
			string[] teamkillMatrix = this.plugin.GetConfigList("friendly_fire_autoban_matrix");
			foreach (string pair in teamkillMatrix)
			{
				string[] tuple = pair.Split(':');
				if (tuple.Length != 2)
				{
					if (this.plugin.outall)
					{
						plugin.Info("Tuple " + pair + " does not have a single : in it.");
					}
					continue;
				}
				int tuple0 = -1, tuple1 = -1;
				if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
				{
					if (this.plugin.outall)
					{
						plugin.Info("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
					}
					continue;
				}

				this.plugin.matrix.Add(new TeamTuple(tuple0, tuple1));
			}

			this.plugin.amount = this.plugin.GetConfigInt("friendly_fire_autoban_amount");
			this.plugin.length = this.plugin.GetConfigInt("friendly_fire_autoban_length");
			this.plugin.expire = this.plugin.GetConfigInt("friendly_fire_autoban_expire");

			this.plugin.scaled = new Dictionary<int, int>();
			string[] teamkillScaled = this.plugin.GetConfigList("friendly_fire_autoban_scaled");
			foreach (string pair in teamkillScaled)
			{
				string[] tuple = pair.Split(':');
				if (tuple.Length != 2)
				{
					if (this.plugin.outall)
					{
						plugin.Info("Tuple " + pair + " does not have a single : in it.");
					}
					continue;
				}
				int tuple0 = -1, tuple1 = -1;
				if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
				{
					if (this.plugin.outall)
					{
						plugin.Info("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
					}
					continue;
				}

				if (!this.plugin.scaled.ContainsKey(tuple0))
				{
					this.plugin.scaled[tuple0] = tuple1;
				}
			}

			this.plugin.noguns = this.plugin.GetConfigInt("friendly_fire_autoban_noguns");
			this.plugin.tospec = this.plugin.GetConfigInt("friendly_fire_autoban_tospec");
			this.plugin.kicker = this.plugin.GetConfigInt("friendly_fire_autoban_kicker");
			this.plugin.bomber = this.plugin.GetConfigInt("friendly_fire_autoban_bomber");
			this.plugin.disarm = this.plugin.GetConfigBool("friendly_fire_autoban_disarm");

			this.plugin.rolewl = new List<RoleTuple>();
			string[] roleWhitelist = this.plugin.GetConfigList("friendly_fire_autoban_rolewl");
			foreach (string pair in roleWhitelist)
			{
				string[] tuple = pair.Split(':');
				if (tuple.Length != 2)
				{
					if (this.plugin.outall)
					{
						plugin.Info("Tuple " + pair + " does not have a single : in it.");
					}
					continue;
				}
				int tuple0 = -1, tuple1 = -1;
				if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
				{
					if (this.plugin.outall)
					{
						plugin.Info("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
					}
					continue;
				}

				this.plugin.rolewl.Add(new RoleTuple(tuple0, tuple1));
			}

			this.plugin.invert = this.plugin.GetConfigInt("friendly_fire_autoban_invert");
			this.plugin.mirror = this.plugin.GetConfigFloat("friendly_fire_autoban_mirror");
			this.plugin.undead = this.plugin.GetConfigInt("friendly_fire_autoban_undead");
			this.plugin.warntk = this.plugin.GetConfigInt("friendly_fire_autoban_warntk");
			this.plugin.votetk = this.plugin.GetConfigInt("friendly_fire_autoban_votetk");

			// Add back if we want to keep track of which teamkills are removed
			//foreach (Timer timer in this.plugin.teamkillTimers.Values)
			//{
			//	timer.Enabled = true;
			//}

			if (this.plugin.outall)
			{
				this.plugin.PrintConfigs();
			}
		}
	}

	class RoundEndHandler : IEventHandlerRoundEnd
	{
		private FriendlyFireAutobanPlugin plugin;

		public RoundEndHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin) plugin;
		}

		public void OnRoundEnd(RoundEndEvent ev)
		{
			if (ev.Round.Duration >= 3) {
				this.plugin.DuringRound = false;
			}
			
			foreach (Timer timer in this.plugin.TeamkillTimers.Values)
			{
				timer.Dispose();
			}
			if (this.plugin.system == 3)
			{
				foreach (Player player in ev.Server.GetPlayers())
				{
					if (this.plugin.Teamkillers.ContainsKey(player.SteamId))
					{
						int teamkills = this.plugin.Teamkillers[player.SteamId].Teamkills.Count;
						if (this.plugin.outall)
						{
							this.plugin.Info("Player " + player.ToString() + " has committed " + teamkills + " teamkills.");
						}

						int banLength = this.plugin.GetScaledBanAmount(player.SteamId);
						if (banLength > 0)
						{
							this.plugin.OnBan(player, player.Name, banLength, this.plugin.Teamkillers[player.SteamId].Teamkills);
						}
						else
						{
							if (this.plugin.outall)
							{
								this.plugin.Info("Player " + player.SteamId + " " + this.plugin.Teamkillers[player.SteamId].Teamkills.Count + " teamkills is not bannable.");
							}
						}
					}
					else
					{
						if (this.plugin.outall)
						{
							this.plugin.Info("Player " + player.ToString() + " has committed no teamkills.");
						}
					}
					this.plugin.Teamkillers.Remove(player.SteamId);
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
	}

	class PlayerDieHandler : IEventHandlerPlayerDie
	{
		private FriendlyFireAutobanPlugin plugin;

		public PlayerDieHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin) plugin;
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (!this.plugin.DuringRound)
			{
				this.plugin.Info("Skipping OnPlayerDie for being outside of a round.");
				return;
			}

			Player killer = ev.Killer;
			string killerOutput = killer.Name + " " + killer.SteamId + " " + killer.IpAddress;
			Player victim = ev.Player;
			string victimOutput = victim.Name + " " + victim.SteamId + " " + victim.IpAddress;

			if (this.plugin.isTeamkill(killer, victim))
			{
				if (this.plugin.enable)
				{
					Teamkill teamkill = new Teamkill(killer.Name, killer.SteamId, killer.TeamRole, victim.Name, victim.SteamId, victim.TeamRole, victim.IsHandcuffed(), ev.DamageTypeVar, this.plugin.Server.Round.Duration);
					this.plugin.TeamkillVictims[ev.Player.SteamId] = teamkill;
					
					if (this.plugin.Teamkillers.ContainsKey(killer.SteamId))
					{
						this.plugin.Teamkillers[killer.SteamId].Teamkills.Add(teamkill);
						plugin.Info("Player " + killerOutput + " " + killer.TeamRole.Team.ToString() + " teamkilled " +
							victimOutput + " " + victim.TeamRole.Team.ToString() + ", for a total of " + this.plugin.Teamkillers[killer.SteamId].Teamkills.Count + " teamkills.");
					}
					else
					{
						this.plugin.Teamkillers[killer.SteamId] = new Teamkiller(killer.PlayerId, killer.Name, killer.SteamId, killer.IpAddress);
						this.plugin.Teamkillers[killer.SteamId].Teamkills.Add(teamkill);
						plugin.Info("Player " + killerOutput + " " + killer.TeamRole.Team.ToString() + " teamkilled " +
							victimOutput + " " + victim.TeamRole.Team.ToString() + ", for a total of 1 teamkill.");
					}

					victim.PersonalBroadcast(10, string.Format(this.plugin.GetTranslation("victim_message"), killer.Name), false);

					if (this.plugin.warntk != -1)
					{
						string broadcast = string.Format(this.plugin.GetTranslation("killer_message"), victim.Name, teamkill.GetRoleDisplay()) + " ";
						if (this.plugin.warntk > 0)
						{
							int teamkillsBeforeBan = this.plugin.amount - this.plugin.Teamkillers[killer.SteamId].Teamkills.Count;
							if (teamkillsBeforeBan <= this.plugin.warntk)
							{
								broadcast += string.Format(this.plugin.GetTranslation("killer_warning"), teamkillsBeforeBan) + " ";
							}
						}
						else
						{
							broadcast += this.plugin.GetTranslation("killer_request") + " ";
						}
						killer.PersonalBroadcast(5, broadcast, false);
					}

					this.plugin.OnCheckRemoveGuns(ev.Killer);

					this.plugin.OnCheckToSpectator(ev.Killer);

					this.plugin.OnCheckUndead(ev.Killer, ev.Player);

					this.plugin.OnCheckKick(ev.Killer);

					this.plugin.OnVoteTeamkill(ev.Killer);

					/*
					 * If ban system is #1, do not create timers and perform a ban based on a static number of teamkills
					 */
					if (this.plugin.system == 1 && this.plugin.Teamkillers[killer.SteamId].Teamkills.Count >= this.plugin.amount)
					{
						this.plugin.OnBan(killer, killer.Name, this.plugin.length, this.plugin.Teamkillers[killer.SteamId].Teamkills);
					}
					else
					{
						Timer t;
						if (this.plugin.TeamkillTimers.ContainsKey(killer.SteamId))
						{
							/*
							 * If ban system is #3, allow the player to continue teamkilling
							 */
							t = this.plugin.TeamkillTimers[killer.SteamId];
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
										int banLength = this.plugin.GetScaledBanAmount(killer.SteamId);
										if (banLength > 0)
										{
											this.plugin.OnBan(killer, killer.Name, banLength, this.plugin.Teamkillers[killer.SteamId].Teamkills);
										}
										else
										{
											if (this.plugin.outall)
											{
												this.plugin.Info("Player " + killer.SteamId + " " + this.plugin.Teamkillers[killer.SteamId].Teamkills.Count + " teamkills is not bannable.");
											}
										}
									}

									if (this.plugin.Teamkillers[killer.SteamId].Teamkills.Count > 0)
									{
										Teamkill firstTeamkill = this.plugin.Teamkillers[killer.SteamId].Teamkills[0];
										this.plugin.Teamkillers[killer.SteamId].Teamkills.RemoveAt(0);
										this.plugin.Info("Player " + killerOutput + " " + killer.TeamRole.Team.ToString() + " teamkill expired, counter now at " + this.plugin.Teamkillers[killer.SteamId].Teamkills.Count + ".");
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
							this.plugin.TeamkillTimers[killer.SteamId] = t;
						}

						/*
						 * If ban system is #2, allow the teamkills to expire
						 */
						if (this.plugin.system == 2 && this.plugin.Teamkillers[killer.SteamId].Teamkills.Count >= this.plugin.amount)
						{
							t.Stop();
							this.plugin.OnBan(killer, killer.Name, this.plugin.length, this.plugin.Teamkillers[killer.SteamId].Teamkills);
						}
					}
				}
				else
				{
					plugin.Info("Player " + killerOutput + " " + killer.TeamRole.Team.ToString() + " teamkilled " +
						victimOutput + " " + victim.TeamRole.Team.ToString() + ".");
				}
			}
			else
			{
				if (this.plugin.outall)
				{
					this.plugin.Info("Player " + killerOutput + " " + killer.TeamRole.Team.ToString() + " killed " +
						victimOutput + " " + victim.TeamRole.Team.ToString() + " and it was not detected as a teamkill.");
				}
			}
		}
	}

	class PlayerHurtHandler : IEventHandlerPlayerHurt
	{
		private FriendlyFireAutobanPlugin plugin;

		public PlayerHurtHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (this.plugin.enable)
			{
				if (this.plugin.mirror > 0f)
				{
					if (this.plugin.isTeamkill(ev.Attacker, ev.Player))
					{
						if (this.plugin.invert > 0)
						{
							if (this.plugin.Teamkillers[ev.Attacker.SteamId].Teamkills.Count >= this.plugin.invert)
							{
								if (this.plugin.outall)
								{
									this.plugin.Info("Dealing damage to " + ev.Attacker.Name + ": " + (ev.Damage * this.plugin.mirror));
								}
								ev.Attacker.Damage((int)(ev.Damage * this.plugin.mirror));
							}
							// else do nothing
						}
						else
						{
							if (this.plugin.outall)
							{
								this.plugin.Info("Dealing damage to " + ev.Attacker.Name + ": " + (ev.Damage * this.plugin.mirror));
							}
							ev.Attacker.Damage((int)(ev.Damage * this.plugin.mirror));
						}
					}
				}
				else if (ev.Player.PlayerId == ev.Attacker.PlayerId && ev.DamageType == DamageType.FRAG)
				{
					if (this.plugin.bomber == 2)
					{
						int damage = (int)ev.Damage;
						ev.Damage = 0;
						Timer t = new Timer
						{
							Interval = 1000,
							Enabled = true
						};
						t.Elapsed += delegate
						{
							ev.Attacker.Damage(damage, DamageType.FALLDOWN);
							t.Dispose();
						};
					}
					else if (this.plugin.bomber == 1)
					{
						ev.Damage = 0;
					}
				}
			}
		}
	}

	class PlayerJoinHandler : IEventHandlerPlayerJoin
	{
		private FriendlyFireAutobanPlugin plugin;

		public PlayerJoinHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			Player player = ev.Player;
			if (!this.plugin.Teamkillers.ContainsKey(player.SteamId))
			{
				this.plugin.Teamkillers[player.SteamId] = new Teamkiller(player.PlayerId, player.Name, player.SteamId, player.IpAddress);
			}
		}
	}

	class PlayerDisconnectHandler : IEventHandlerDisconnect
	{
		private FriendlyFireAutobanPlugin plugin;

		public PlayerDisconnectHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnDisconnect(DisconnectEvent ev)
		{
			if (this.plugin.enable && this.plugin.system == 3 && this.plugin.DuringRound && !this.plugin.ProcessingDisconnect)
			{
				this.plugin.ProcessingDisconnect = true;

				List<Teamkiller> disconnectedUsers = this.plugin.Teamkillers.Values.Where(tker => !this.plugin.Server.GetPlayers().Any(p => tker.SteamId == p.SteamId)).ToList();

				foreach (Teamkiller teamkiller in disconnectedUsers)
				{
					if (this.plugin.Teamkillers.ContainsKey(teamkiller.SteamId) && this.plugin.Teamkillers[teamkiller.SteamId].Teamkills.Count > 0)
					{
						int teamkills = this.plugin.Teamkillers[teamkiller.SteamId].Teamkills.Count;
						if (this.plugin.outall)
						{
							this.plugin.Info("Player " + teamkiller.Name + " has committed " + teamkills + " teamkills.");
						}

						int banLength = this.plugin.GetScaledBanAmount(teamkiller.SteamId);
						if (banLength > 0)
						{
							PluginManager.Manager.Server.BanSteamId(
								teamkiller.Name,
								teamkiller.SteamId, 
								banLength, 
								string.Format(this.plugin.GetTranslation("offline_ban"), banLength, teamkills), 
								"FriendlyFireAutoban"
							);
							this.plugin.Info(teamkiller.Name + " / " + teamkiller.SteamId + ": Banned " + banLength + " minutes for teamkilling " + teamkills + " players");
							PluginManager.Manager.Server.BanIpAddress(
								teamkiller.Name, 
								teamkiller.IpAddress, 
								banLength,
								string.Format(this.plugin.GetTranslation("offline_ban"), banLength, teamkills), 
								"FriendlyFireAutoban"
							);
							this.plugin.Info(teamkiller.Name + " / " + teamkiller.IpAddress + ": Banned " + banLength + " minutes for teamkilling " + teamkills + " players");
							//this.plugin.Ban(player, player.Name, banLength, this.plugin.teamkillCounter[player.SteamId]);
						}
						else
						{
							if (this.plugin.outall)
							{
								this.plugin.Info("Player " + teamkiller.Name + " " + this.plugin.Teamkillers[teamkiller.SteamId].Teamkills.Count + " teamkills is not bannable.");
							}
						}
					}
					else
					{
						if (this.plugin.outall)
						{
							this.plugin.Info("Player " + teamkiller.SteamId + " has committed no teamkills.");
						}
					}

					this.plugin.Teamkillers.Remove(teamkiller.SteamId);
				}

				this.plugin.ProcessingDisconnect = false;
			}
		}
	}

	class SpawnHandler : IEventHandlerSpawn
	{
		private FriendlyFireAutobanPlugin plugin;

		public SpawnHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnSpawn(PlayerSpawnEvent ev)
		{
			if (this.plugin.enable)
			{
				this.plugin.OnCheckRemoveGuns(ev.Player);
			}
		}
	}

	class SetRoleHandler : IEventHandlerSetRole
	{
		private FriendlyFireAutobanPlugin plugin;

		public SetRoleHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (this.plugin.enable)
			{
				this.plugin.OnCheckRemoveGuns(ev.Player);
			}
		}
	}

	class PlayerPickupItemLateHandler : IEventHandlerPlayerPickupItemLate
	{
		private FriendlyFireAutobanPlugin plugin;

		public PlayerPickupItemLateHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnPlayerPickupItemLate(PlayerPickupItemLateEvent ev)
		{
			if (this.plugin.enable)
			{
				this.plugin.OnCheckRemoveGuns(ev.Player);
			}
		}
	}
}
