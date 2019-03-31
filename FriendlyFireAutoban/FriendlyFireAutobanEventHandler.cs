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
			this.plugin.duringRound = true;
			// Remove if teamkills can be removed for memory cleanup
			/**
			 * MEMORY CLEANUP
			 */
			this.plugin.ipAddressToSteamList = new Dictionary<string, List<string>>();
			this.plugin.teamkillCounter = new Dictionary<string, List<Teamkill>>();
			this.plugin.teamkillTimers = new Dictionary<string, Timer>();

			this.plugin.enable = this.plugin.GetConfigBool("friendly_fire_autoban_enable");
			this.plugin.outall = this.plugin.GetConfigBool("friendly_fire_autoban_outall");
			this.plugin.system = this.plugin.GetConfigInt("friendly_fire_autoban_system");
			this.plugin.amount = this.plugin.GetConfigInt("friendly_fire_autoban_amount");
			this.plugin.length = this.plugin.GetConfigInt("friendly_fire_autoban_length");
			this.plugin.expire = this.plugin.GetConfigInt("friendly_fire_autoban_expire");
			this.plugin.noguns = this.plugin.GetConfigInt("friendly_fire_autoban_noguns");
			this.plugin.tospec = this.plugin.GetConfigInt("friendly_fire_autoban_tospec");
			this.plugin.kicker = this.plugin.GetConfigInt("friendly_fire_autoban_kicker");
			this.plugin.bomber = this.plugin.GetConfigInt("friendly_fire_autoban_bomber");
			if (this.plugin.outall)
			{
				this.plugin.Info("friendly_fire_autoban_enable value: " + this.plugin.GetConfigBool("friendly_fire_autoban_enable"));
				this.plugin.Info("friendly_fire_autoban_system value: " + this.plugin.GetConfigInt("friendly_fire_autoban_system"));
				this.plugin.Info("friendly_fire_autoban_amount value: " + this.plugin.GetConfigInt("friendly_fire_autoban_amount"));
				this.plugin.Info("friendly_fire_autoban_amount value: " + this.plugin.GetConfigInt("friendly_fire_autoban_amount"));
				this.plugin.Info("friendly_fire_autoban_length value: " + this.plugin.GetConfigInt("friendly_fire_autoban_length"));
				this.plugin.Info("friendly_fire_autoban_expire value: " + this.plugin.GetConfigInt("friendly_fire_autoban_expire"));
				this.plugin.Info("friendly_fire_autoban_noguns value: " + this.plugin.GetConfigInt("friendly_fire_autoban_noguns"));
				this.plugin.Info("friendly_fire_autoban_tospec value: " + this.plugin.GetConfigInt("friendly_fire_autoban_tospec"));
				this.plugin.Info("friendly_fire_autoban_kicker value: " + this.plugin.GetConfigInt("friendly_fire_autoban_kicker"));
				this.plugin.Info("friendly_fire_autoban_bomber value: " + this.plugin.GetConfigInt("friendly_fire_autoban_bomber"));
			}

			this.plugin.teamkillMatrix = new List<TeamkillTuple>();
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

				this.plugin.teamkillMatrix.Add(new TeamkillTuple(tuple0, tuple1));
			}

			this.plugin.teamkillScaled = new Dictionary<int, int>();
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

				if (!this.plugin.teamkillScaled.ContainsKey(tuple0))
				{
					this.plugin.teamkillScaled[tuple0] = tuple1;
				}
			}

			// Add back if we want to keep track of which teamkills are removed
			//foreach (Timer timer in this.plugin.teamkillTimers.Values)
			//{
			//	timer.Enabled = true;
			//}
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
				this.plugin.duringRound = false;
			}

			//if (this.plugin.GetConfigInt("friendly_fire_autoban_system") == 2)
			//{
			foreach (Timer timer in this.plugin.teamkillTimers.Values)
			{
				//timer.Enabled = false;
				timer.Dispose();
			}
			//} else 
			if (this.plugin.system == 3)
			{
				foreach (Player player in ev.Server.GetPlayers())
				{
					if (this.plugin.teamkillCounter.ContainsKey(player.SteamId))
					{
						int teamkills = this.plugin.teamkillCounter[player.SteamId].Count;
						if (this.plugin.outall)
						{
							this.plugin.Info("Player " + player.ToString() + " has committed " + teamkills + " teamkills.");
						}

						int banLength = this.plugin.GetScaledBanAmount(player.SteamId);
						if (banLength > 0)
						{
							//int banLength = this.plugin.teamkillScaled[teamkills];
							this.plugin.Ban(player, player.Name, banLength, this.plugin.teamkillCounter[player.SteamId]);
						}
						else
						{
							if (this.plugin.outall)
							{
								this.plugin.Info("Player " + player.SteamId + " " + this.plugin.teamkillCounter[player.SteamId].Count + " teamkills is not bannable.");
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
				}
			}

			/*
			 * MEMORY CLEANUP
			 */
			this.plugin.ipAddressToSteamList = new Dictionary<string, List<string>>();
			this.plugin.teamkillCounter = new Dictionary<string, List<Teamkill>>();
			this.plugin.teamkillTimers = new Dictionary<string, Timer>();
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
			if (!this.plugin.duringRound)
			{
				this.plugin.Info("Skipping OnPlayerDie for being outside of a round.");
				return;
			}

			Player killer = ev.Killer;
			string killerOutput = killer.Name + " " + killer.SteamId + " " + killer.IpAddress;
			Player victim = ev.Player;
			string victimOutput = victim.Name + " " + victim.SteamId + " " + victim.IpAddress;

			/*string[] killerNameParts = Regex.Split(killer.ToString(), @"::");
			if (killerNameParts.Length >= 4)
			{
				killerNameParts = new string[] { killerNameParts[0], "::" + killerNameParts[2], killerNameParts[3] };
			}
			string[] victimNameParts = Regex.Split(victim.ToString(), @"::");
			if (victimNameParts.Length >= 4)
			{
				victimNameParts = new string[] { victimNameParts[0], "::" + victimNameParts[2], victimNameParts[3] };
			}*/

			if (isTeamkill(killer, victim))
			{
				if (this.plugin.enable)
				{
					Teamkill teamkill = new Teamkill(killer.Name, killer.SteamId, killer.TeamRole, victim.Name, victim.SteamId, victim.TeamRole, victim.IsHandcuffed(), ev.DamageTypeVar);
					this.plugin.teamkillVictims[ev.Player.SteamId] = teamkill;
					
					if (this.plugin.teamkillCounter.ContainsKey(killer.SteamId))
					{
						this.plugin.teamkillCounter[killer.SteamId].Add(teamkill);
						plugin.Info("Player " + killerOutput + " " + killer.TeamRole.Team.ToString() + " teamkilled " +
							victimOutput + " " + victim.TeamRole.Team.ToString() + ", for a total of " + this.plugin.teamkillCounter[killer.SteamId].Count + " teamkills.");
					}
					else
					{
						this.plugin.teamkillCounter[killer.SteamId] = new List<Teamkill>();
						this.plugin.teamkillCounter[killer.SteamId].Add(teamkill);
						plugin.Info("Player " + killerOutput + " " + killer.TeamRole.Team.ToString() + " teamkilled " +
							victimOutput + " " + victim.TeamRole.Team.ToString() + ", for a total of 1 teamkill.");
					}

					string broadcast = "You teamkilled " + victim.Name + " " + teamkill.getRoleDisplay() + ". ";
					if (this.plugin.amount - this.plugin.teamkillCounter[killer.SteamId].Count == 2)
					{
						broadcast += "If you teamkill 2 more times you will be banned. ";
					}
					else if (this.plugin.amount - this.plugin.teamkillCounter[killer.SteamId].Count == 1)
					{
						broadcast += "If you teamkill 1 more time you will be banned. ";
					}
					else
					{
						broadcast += "Please do not teamkill. ";
					}
					killer.PersonalBroadcast(5, broadcast, false);

					this.plugin.CheckRemoveGuns(ev.Killer);

					if (this.plugin.tospec > 0 && this.plugin.teamkillCounter[killer.SteamId].Count >= this.plugin.tospec && !this.plugin.isImmune(killer))
					{
						killer.PersonalBroadcast(5, "You have been moved to spectate for teamkilling.", false);
						this.plugin.Info("Player " + killerOutput + " has been moved to spectator for teamkilling " + this.plugin.teamkillCounter[killer.SteamId].Count + " times.");
						killer.ChangeRole(Role.SPECTATOR);
					}

					if (this.plugin.kicker > 0 && this.plugin.teamkillCounter[killer.SteamId].Count == this.plugin.kicker && !this.plugin.isImmune(killer))
					{
						killer.PersonalBroadcast(1, "You will be kicked for teamkilling.", false);
						this.plugin.Info("Player " + killerOutput + " has been kicked for teamkilling " + this.plugin.teamkillCounter[killer.SteamId].Count + " times.");
						killer.Ban(0);
					}

					/*
					 * If ban system is #1, do not create timers and perform a ban based on a static number of teamkills
					 */
					if (this.plugin.system == 1 && this.plugin.teamkillCounter[killer.SteamId].Count >= this.plugin.amount)
					{
						this.plugin.Ban(killer, killerOutput, this.plugin.length, this.plugin.teamkillCounter[killer.SteamId]);
					}
					else
					{
						Timer t;
						if (this.plugin.teamkillTimers.ContainsKey(killer.SteamId))
						{
							/*
							 * If ban system is #3, allow the player to continue teamkilling
							 */
							t = this.plugin.teamkillTimers[killer.SteamId];
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
											//int banLength = this.plugin.teamkillScaled[teamkills];
											this.plugin.Ban(killer, killer.Name, banLength, this.plugin.teamkillCounter[killer.SteamId]);
										}
										else
										{
											//if (this.plugin.outall)
											//{
											this.plugin.Info("Player " + killer.SteamId + " " + this.plugin.teamkillCounter[killer.SteamId].Count + " teamkills is not bannable.");
											//}
										}
									}

									if (this.plugin.teamkillCounter[killer.SteamId].Count > 0)
									{
										Teamkill firstTeamkill = this.plugin.teamkillCounter[killer.SteamId][0];
										this.plugin.teamkillCounter[killer.SteamId].RemoveAt(0);
										this.plugin.Info("Player " + killerOutput + " " + killer.TeamRole.Team.ToString() + " teamkill expired, counter now at " + this.plugin.teamkillCounter[killer.SteamId].Count + ".");
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
							this.plugin.teamkillTimers[killer.SteamId] = t;
						}

						/*
						 * If ban system is #2, allow the teamkills to expire
						 */
						if (this.plugin.system == 2 && this.plugin.teamkillCounter[killer.SteamId].Count >= this.plugin.amount)
						{
							t.Stop();
							this.plugin.Ban(killer, killerOutput, this.plugin.length, this.plugin.teamkillCounter[killer.SteamId]);
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

		public bool isTeamkill(Player killer, Player victim)
		{
			int killerTeam = (int) killer.TeamRole.Team;
			int victimTeam = (int) victim.TeamRole.Team;

			if (String.Equals(killer.SteamId, victim.SteamId))
			{
				return false;
			}
			
			foreach (TeamkillTuple teamkill in this.plugin.teamkillMatrix)
			{
				if (killerTeam == teamkill.killerRole && victimTeam == teamkill.victimRole)
				{
					return true;
				}
			}

			return false;
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
				if (ev.Player.PlayerId == ev.Attacker.PlayerId && ev.DamageType == DamageType.FRAG)
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
			if (this.plugin.enable && this.plugin.system == 3)
			{
				if (!this.plugin.ipAddressToSteamList.ContainsKey(ev.Player.IpAddress))
				{
					this.plugin.ipAddressToSteamList[ev.Player.IpAddress] = new List<string>();
				}
				this.plugin.ipAddressToSteamList[ev.Player.IpAddress].Add(ev.Player.SteamId);
			}
		}
	}

	/*class PlayerConnectHandler : IEventHandlerConnect
	{
		private FriendlyFireAutobanPlugin plugin;

		public PlayerConnectHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnConnect(ConnectEvent ev)
		{
			
		}
	}*/

	class PlayerDisconnectHandler : IEventHandlerDisconnect
	{
		private FriendlyFireAutobanPlugin plugin;

		public PlayerDisconnectHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnDisconnect(DisconnectEvent ev)
		{
			if (this.plugin.enable && this.plugin.system == 3)
			{
				if (this.plugin.outall) this.plugin.Info("Player " + ev.Connection.IpAddress + " disconnected before he could be banned!");
				string steamId = "";
				if (this.plugin.ipAddressToSteamList.ContainsKey(ev.Connection.IpAddress))
				{
					if (this.plugin.ipAddressToSteamList[ev.Connection.IpAddress].Count == 1)
					{
						steamId = this.plugin.ipAddressToSteamList[ev.Connection.IpAddress][0];
						this.plugin.ipAddressToSteamList.Remove(ev.Connection.IpAddress);
						if (this.plugin.outall) this.plugin.Info("Single Steam ID found in ipAddressToSteamList: " + steamId);
					}
					else
					{
						HashSet<string> onlineSteamIds = new HashSet<string>(this.plugin.Server.GetPlayers().Select(player => player.SteamId).ToList());
						HashSet<string> ipSteamIds = new HashSet<string>(this.plugin.ipAddressToSteamList[ev.Connection.IpAddress]);
						List<string> singleSteamId = ipSteamIds.Where(s => !onlineSteamIds.Contains(s)).ToList();
						if (this.plugin.outall) this.plugin.Info("onlineSteamIds " + onlineSteamIds.ToString());
						if (this.plugin.outall) this.plugin.Info("ipSteamIds " + ipSteamIds.ToString());
						if (singleSteamId.Count == 1)
						{

							steamId = singleSteamId[0];
							// Remove the Steam ID while leaving the other Steam IDs in the array
							this.plugin.ipAddressToSteamList[ev.Connection.IpAddress].RemoveAll(x => x == steamId);
							if (this.plugin.outall) this.plugin.Info("Steam ID from multiple Steam IDs found: " + steamId);
						}
						else
						{
							this.plugin.Info("Did not properly clean up IP addresses, multiple disconnected IPs in one array.");
							if (this.plugin.outall) this.plugin.ipAddressToSteamList.Remove(ev.Connection.IpAddress);
						}
					}
				}
				else
				{
					this.plugin.Info("ipAddressToSteamList does not contain " + ev.Connection.IpAddress);
				}

				if (this.plugin.teamkillCounter.ContainsKey(steamId) && this.plugin.teamkillCounter[steamId].Count > 0)
				{
					int teamkills = this.plugin.teamkillCounter[steamId].Count;
					if (this.plugin.outall)
					{
						this.plugin.Info("Player " + this.plugin.teamkillCounter[steamId][0].killerName + " has committed " + teamkills + " teamkills.");
					}

					int banLength = this.plugin.GetScaledBanAmount(steamId);
					if (banLength > 0)
					{
						PluginManager.Manager.Server.BanSteamId(this.plugin.teamkillCounter[steamId][0].killerName, steamId, banLength, "Banned " + banLength + " minutes for teamkilling " + teamkills + " players", "FriendlyFireAutoban");
						this.plugin.Info(this.plugin.teamkillCounter[steamId][0].killerName + " / " + steamId + ": Banned " + banLength + " minutes for teamkilling " + teamkills + " players");
						PluginManager.Manager.Server.BanIpAddress(this.plugin.teamkillCounter[steamId][0].killerName, ev.Connection.IpAddress, banLength, "Banned " + banLength + " minutes for teamkilling " + teamkills + " players", "FriendlyFireAutoban");
						this.plugin.Info(this.plugin.teamkillCounter[steamId][0].killerName + " / " + ev.Connection.IpAddress + ": Banned " + banLength + " minutes for teamkilling " + teamkills + " players");
						//this.plugin.Ban(player, player.Name, banLength, this.plugin.teamkillCounter[player.SteamId]);
					}
					else
					{
						if (this.plugin.outall)
						{
							this.plugin.Info("Player " + this.plugin.teamkillCounter[steamId][0].killerName + " " + this.plugin.teamkillCounter[steamId].Count + " teamkills is not bannable.");
						}
					}
				}
				else
				{
					if (this.plugin.outall)
					{
						this.plugin.Info("Player " + steamId + " has committed no teamkills.");
					}
				}
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
				this.plugin.CheckRemoveGuns(ev.Player);
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
				this.plugin.CheckRemoveGuns(ev.Player);
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
				this.plugin.CheckRemoveGuns(ev.Player);
			}
		}
	}

	class CallCommandHandler : IEventHandlerCallCommand
	{
		private FriendlyFireAutobanPlugin plugin;

		public CallCommandHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnCallCommand(PlayerCallCommandEvent ev)
		{
			string command = ev.Command.Split(' ')[0];
			string[] quotedArgs = Regex.Matches(ev.Command, "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'")
				.Cast<Match>()
				.Select(m => m.Value)
				.ToArray()
				.Skip(1)
				.ToArray();

			switch (command)
			{
				case "forgive":
					if (this.plugin.enable)
					{
						if (this.plugin.teamkillVictims.ContainsKey(ev.Player.SteamId))
						{
							Teamkill teamkill = this.plugin.teamkillVictims[ev.Player.SteamId];

							if (this.plugin.teamkillCounter.ContainsKey(teamkill.killerSteamId))
							{
								int teamkillIndex = -1;
								// https://stackoverflow.com/questions/19164310/is-there-a-more-efficient-linq-statement-to-reverse-search-for-a-condition-in-a
								for (int i = this.plugin.teamkillCounter[teamkill.killerSteamId].Count; i > 0; i++)
								{
									if (teamkill.Equals(this.plugin.teamkillCounter[teamkill.killerSteamId][i - 1]))
									{
										teamkillIndex = i - 1;
									}
								}

								if (teamkillIndex > -1)
								{
									// No need for broadcast with return message
									//ev.Player.PersonalBroadcast(5, "You forgave this player.", false);
									// TODO: Send a broadcast to the killer
									ev.ReturnMessage = "You have forgiven " + this.plugin.teamkillCounter[teamkill.killerSteamId][teamkillIndex].killerName + " " + this.plugin.teamkillCounter[teamkill.killerSteamId][teamkillIndex].getRoleDisplay() + ".";
									this.plugin.teamkillCounter[teamkill.killerSteamId].RemoveAt(teamkillIndex);
								}
								else
								{
									ev.ReturnMessage = "You already forgave " + teamkill.killerName + " or this teamkill has expired.";
								}
							}
							else
							{
								ev.ReturnMessage = "The player has disconnected.";
							}

							// No matter what, remove this teamkill cached in the array
							this.plugin.teamkillVictims.Remove(ev.Player.SteamId);
						}
						else
						{
							ev.ReturnMessage = "There is no teamkill for you to forgive.";
						}
					}
					else
					{
						ev.ReturnMessage = "Friendly Fire Autoban is currently disabled.";
					}
					break;

				case "tks":

					if (this.plugin.enable)
					{
						if (quotedArgs.Length == 1)
						{
							List<Teamkill> teamkills = new List<Teamkill>();
							try
							{
								// https://stackoverflow.com/questions/55436309/how-do-i-use-linq-to-select-from-a-list-inside-a-map
								teamkills = this.plugin.teamkillCounter.Values.SelectMany(x => x.Where(y => y.killerName.Contains(quotedArgs[1]))).ToList();
							}
							catch (Exception e)
							{
								if (this.plugin.outall)
								{
									this.plugin.Error(e.Message);
									this.plugin.Error(e.StackTrace);
								}
							}

							if (teamkills.Count == 0)
							{
								ev.ReturnMessage = "No players by this name has any teamkills.";
							}
							else
							{
								string retval = "";
								foreach (Teamkill tk in teamkills)
								{
									retval += tk.killerName + " teamkilled " + tk.victimName + " " + tk.getRoleDisplay() + ". \n";
								}
								ev.ReturnMessage = retval;
							}
						}
						else
						{
							ev.ReturnMessage = "Player name not provided or not quoted.";
						}
					}
					else
					{
						ev.ReturnMessage = "Friendly Fire Autoban is currently disabled.";
					}
					break;
			}
		}
	}
}
