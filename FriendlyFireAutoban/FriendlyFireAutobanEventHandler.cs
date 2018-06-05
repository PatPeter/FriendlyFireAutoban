using Smod2;
using Smod2.API;
using Smod2.Events;
using System.Collections.Generic;

namespace FriendlyFireAutoban.EventHandlers
{
    class RoundStartHandler : IEventRoundStart
    {
		private FriendlyFireAutobanPlugin plugin;

		public RoundStartHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin) plugin;
		}

		public void OnRoundStart(Server server)
		{
			this.plugin.duringRound = true;
			this.plugin.teamkillCounter = new Dictionary<string, int>();
		}
	}

	class RoundEndHandler : IEventRoundEnd
	{
		private FriendlyFireAutobanPlugin plugin;

		public RoundEndHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin) plugin;
		}

		public void OnRoundEnd(Server server, Round round)
		{
			this.plugin.duringRound = false;
			this.plugin.teamkillCounter = new Dictionary<string, int>();
		}
	}

	class PlayerDieHandler : IEventPlayerDie
	{
		private FriendlyFireAutobanPlugin plugin;

		public PlayerDieHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin) plugin;
		}

		public void OnPlayerDie(Player player, Player killer, out bool spawnRagdoll)
		{
			if (this.plugin.GetConfigBool("friendly_fire_autoban_enabled") && this.plugin.duringRound && (int) player.Class.Team == (int) killer.Class.Team)
			{
				if (this.plugin.teamkillCounter.ContainsKey(killer.SteamId)) {
					this.plugin.teamkillCounter[killer.SteamId]++;
					plugin.Info("Player " + killer.ToString() + " killed " + player.ToString() + ", for a total of " + this.plugin.teamkillCounter[killer.SteamId] + " teamkills.");
				}
				else
				{
					this.plugin.teamkillCounter[killer.SteamId] = 1;
					plugin.Info("Player " + killer.ToString() + " killed " + player.ToString() + ", for a total of 1 teamkill.");
				}

				if (this.plugin.teamkillCounter[killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_amount")) {
					plugin.Info("Player " + killer.ToString() + " has been banned for " + this.plugin.GetConfigInt("friendly_fire_autoban_length") + " seconds after teamkilling " + this.plugin.teamkillCounter[killer.SteamId] + " players.");
					killer.Ban(this.plugin.GetConfigInt("friendly_fire_autoban_length"));
				}
				spawnRagdoll = true;
			}
			else if (player.Class.ClassType == Classes.SCP_106)
			{
				spawnRagdoll = false;
			}
			else
			{
				spawnRagdoll = true;
			}
		}
	}
}
