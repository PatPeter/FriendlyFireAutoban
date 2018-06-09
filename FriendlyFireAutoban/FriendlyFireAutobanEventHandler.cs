using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;

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
			this.plugin.teamkillCounter = new Dictionary<string, int>();
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
			if (this.plugin.GetConfigBool("friendly_fire_autoban_enable") && this.plugin.duringRound && isTeamkill(ev.Player.TeamRole.Team, ev.Killer.TeamRole.Team))
			{
				if (this.plugin.teamkillCounter.ContainsKey(ev.Killer.SteamId)) {
					this.plugin.teamkillCounter[ev.Killer.SteamId]++;
					plugin.Info("Player " + ev.Killer.ToString() + " killed " + ev.Player.ToString() + ", for a total of " + this.plugin.teamkillCounter[ev.Killer.SteamId] + " teamkills.");
				}
				else
				{
					this.plugin.teamkillCounter[ev.Killer.SteamId] = 1;
					plugin.Info("Player " + ev.Killer.ToString() + " killed " + ev.Player.ToString() + ", for a total of 1 teamkill.");
				}

				if (this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_amount")) {
					plugin.Info("Player " + ev.Killer.ToString() + " has been banned for " + this.plugin.GetConfigInt("friendly_fire_autoban_length") + " seconds after teamkilling " + this.plugin.teamkillCounter[ev.Killer.SteamId] + " players.");
					ev.Killer.Ban(this.plugin.GetConfigInt("friendly_fire_autoban_length"));
				}
				ev.SpawnRagdoll = true;
			}
			else if (ev.Player.TeamRole.Role == Role.SCP_106)
			{
				ev.SpawnRagdoll = false;
			}
			else
			{
				ev.SpawnRagdoll = true;
			}
		}

		public bool isTeamkill(Team team1, Team team2)
		{
			if ((int)team1 == (int)team2)
			{
				return true;
			}
			else if (team1 == Team.NINETAILFOX && team2 == Team.SCIENTISTS)
			{
				return true;
			}
			else if (team1 == Team.SCIENTISTS && team2 == Team.NINETAILFOX)
			{
				return true;
			}
			else if (team1 == Team.CHAOS_INSURGENCY && team2 == Team.CLASSD)
			{
				return true;
			}
			else if (team1 == Team.CLASSD && team2 == Team.CHAOS_INSURGENCY)
			{
				return true;
			}
			else
			{
				return false;
			}

		}
	}
}
