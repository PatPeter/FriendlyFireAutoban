using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
			this.plugin.Debug("friendly_fire_autoban_enable value: " + this.plugin.GetConfigBool("friendly_fire_autoban_enable"));
			this.plugin.Debug("friendly_fire_autoban_amount value: " + this.plugin.GetConfigInt("friendly_fire_autoban_amount"));
			this.plugin.Debug("friendly_fire_autoban_length value: " + this.plugin.GetConfigInt("friendly_fire_autoban_length"));
			this.plugin.Debug("friendly_fire_autoban_noguns value: " + this.plugin.GetConfigInt("friendly_fire_autoban_noguns"));
			this.plugin.Debug("friendly_fire_autoban_tospec value: " + this.plugin.GetConfigInt("friendly_fire_autoban_tospec"));
			this.plugin.duringRound = true;
			this.plugin.teamkillCounter = new Dictionary<string, int>();
			this.plugin.teamkillMatrix = new List<TeamkillTuple>();
			string[] teamkillMatrix = this.plugin.GetConfigList("friendly_fire_autoban_matrix");
			foreach (string pair in teamkillMatrix)
			{
				string[] tuple = pair.Split(':');
				if (tuple.Length != 2)
				{
					plugin.Debug("Tuple " + pair + " does not have a single : in it.");
					continue;
				}
				int tuple0 = -1, tuple1 = -1;
				if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
				{
					plugin.Debug("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
					continue;
				}

				this.plugin.teamkillMatrix.Add(new TeamkillTuple(tuple0, tuple1));
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
			this.plugin.duringRound = false;
			this.plugin.teamkillCounter = new Dictionary<string, int>();
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
				this.plugin.Debug("Skipping OnPlayerDie for being outside of a round.");
				return;
			}

			if (this.plugin.GetConfigBool("friendly_fire_autoban_enable"))
			{
				string[] killerNameParts = Regex.Split(ev.Killer.ToString(), @"::");
				if (killerNameParts.Length >= 4)
				{
					killerNameParts = new string[] { killerNameParts[0], "::" + killerNameParts[2], killerNameParts[3] };
				}
				string[] victimNameParts = Regex.Split(ev.Player.ToString(), @"::");
				if (victimNameParts.Length >= 4)
				{
					victimNameParts = new string[] { victimNameParts[0], "::" + victimNameParts[2], victimNameParts[3] };
				}

				if (isTeamkill(ev.Killer, ev.Player))
				{
					if (this.plugin.teamkillCounter.ContainsKey(ev.Killer.SteamId))
					{
						this.plugin.teamkillCounter[ev.Killer.SteamId]++;
						plugin.Info("Player " + String.Join(" ", killerNameParts) + " " + ev.Killer.TeamRole.Team.ToString() + " killed " +
							String.Join(" ", victimNameParts) + " " + ev.Player.TeamRole.Team.ToString() + ", for a total of " + this.plugin.teamkillCounter[ev.Killer.SteamId] + " teamkills.");
					}
					else
					{
						this.plugin.teamkillCounter[ev.Killer.SteamId] = 1;
						plugin.Info("Player " + String.Join(" ", killerNameParts) + " " + ev.Killer.TeamRole.Team.ToString() + " killed " +
							String.Join(" ", victimNameParts) + " " + ev.Player.TeamRole.Team.ToString() + ", for a total of 1 teamkill.");
					}

					if (this.plugin.GetConfigInt("friendly_fire_autoban_noguns") > 0 && this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_noguns"))
					{
						this.plugin.Info("Player " + String.Join(" ", killerNameParts) + " has had his/her guns removed for teamkilling.");
						List<Item> inv = ev.Killer.GetInventory();
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
								case ItemType.POSITRON_GRENADE:
									inv[i].Remove();
									break;
							}
						}
					}

					if (this.plugin.GetConfigInt("friendly_fire_autoban_tospec") > 0 && this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_tospec"))
					{
						this.plugin.Info("Player " + String.Join(" ", killerNameParts) + " has been moved to spectator for teamkilling.");
						ev.Killer.ChangeRole(Role.SPECTATOR);
					}

					if (this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_amount"))
					{
						this.plugin.Info("Player " + String.Join(" ", killerNameParts) + " has been banned for " + this.plugin.GetConfigInt("friendly_fire_autoban_length") + " minutes after teamkilling " + this.plugin.teamkillCounter[ev.Killer.SteamId] + " players.");
						ev.Killer.Ban(this.plugin.GetConfigInt("friendly_fire_autoban_length"));
					}
				}
				else
				{
					this.plugin.Debug("Player " + String.Join(" ", killerNameParts) + " " + ev.Killer.TeamRole.Team.ToString() + " killed " +
						String.Join(" ", victimNameParts) + " " + ev.Player.TeamRole.Team.ToString() + " and it was not detected as a teamkill.");
				}
			}
		}

		public bool isTeamkill(Player killer, Player victim)
		{
			int killerTeam = (int) killer.TeamRole.Team;
			int victimTeam = (int) victim.TeamRole.Team;

			if (killer.SteamId == victim.SteamId)
			{
				return false;
			}

			bool isTeamkill = false;
			foreach (TeamkillTuple teamkill in this.plugin.teamkillMatrix)
			{
				if (killerTeam == teamkill.killerRole && victimTeam == teamkill.victimRole)
				{
					isTeamkill = true;
				}
			}

			return isTeamkill;
		}
	}
}
