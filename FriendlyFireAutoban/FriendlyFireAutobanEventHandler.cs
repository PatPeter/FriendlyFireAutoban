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
			if (this.plugin.GetConfigBool("friendly_fire_autoban_enable") && 
				this.plugin.duringRound && 
				isTeamkill(ev.Killer, ev.Player))
			{
				if (this.plugin.teamkillCounter.ContainsKey(ev.Killer.SteamId)) {
					this.plugin.teamkillCounter[ev.Killer.SteamId]++;
					plugin.Info("Player " + ev.Killer.ToString() + " (" + ev.Killer.TeamRole.ToString() + ") killed " + ev.Player.ToString() + " (" + ev.Player.TeamRole.ToString() + "), for a total of " + this.plugin.teamkillCounter[ev.Killer.SteamId] + " teamkills.");
				}
				else
				{
					this.plugin.teamkillCounter[ev.Killer.SteamId] = 1;
					plugin.Info("Player " + ev.Killer.ToString() + " (" + ev.Killer.TeamRole.ToString() + ") killed " + ev.Player.ToString() + " (" + ev.Player.TeamRole.ToString() + "), for a total of 1 teamkill.");
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

		public bool isTeamkill(Player killer, Player victim)
		{
			int killerTeam = (int) killer.TeamRole.Team;
			int victimTeam = (int) victim.TeamRole.Team;

			if (killer.SteamId == victim.SteamId)
			{
				return false;
			}

			bool isTeamkill = false;
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
				if (killerTeam == tuple0 && victimTeam == tuple1)
				{
					isTeamkill = true;
				}
			}
			return isTeamkill;
		}
	}
}
