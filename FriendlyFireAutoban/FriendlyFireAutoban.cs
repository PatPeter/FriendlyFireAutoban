using FriendlyFireAutoban.EventHandlers;
using Smod2;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;

namespace FriendlyFireAutoban
{
	[PluginDetails(
		author = "PatPeter",
		name = "Friendly Fire Autoban",
		description = "Plugin that autobans players for friendly firing.",
		id = "patpeter.friendly.fire.autoban",
		version = "1.2.0",
		SmodMajor = 3,
		SmodMinor = 1,
		SmodRevision = 0
		)]
	class FriendlyFireAutobanPlugin : Plugin
	{
		internal bool duringRound = false;
		internal Dictionary<string, int> teamkillCounter = new Dictionary<string, int>();
		internal Dictionary<int, int> teamkillMatrix = new Dictionary<int, int>();

		public override void OnEnable()
		{
			this.Info("Friendly Fire Autoban 1.2.0 has loaded :)");
			this.Info("friendly_fire_autoban_enable value: " + this.GetConfigBool("friendly_fire_autoban_enable"));
			this.Info("friendly_fire_autoban_amount value: " + this.GetConfigInt("friendly_fire_autoban_amount"));
			this.Info("friendly_fire_autoban_length value: " + this.GetConfigInt("friendly_fire_autoban_length"));
			this.Info("friendly_fire_autoban_matrix value: " + this.GetConfigList("friendly_fire_autoban_matrix").ToString());
		}

		public override void OnDisable()
		{

		}

		public override void Register()
		{
			// Register Events
			this.AddEventHandler(typeof(IEventHandlerRoundStart), new RoundStartHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventHandlerRoundEnd), new RoundEndHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventHandlerPlayerDie), new PlayerDieHandler(this), Priority.Highest);
			// Register config settings
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_enable", true, Smod2.Config.SettingType.BOOL, true, "Enable Friendly Fire Autoban."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_amount", 5, Smod2.Config.SettingType.NUMERIC, true, "Friendly Fire Autoban amount of teamkills before a ban."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_length", 3600, Smod2.Config.SettingType.NUMERIC, true, "Friendly Fire Autoban length in seconds."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_matrix", new string[] { "1:1", "2:2", "1:3", "2:4", "3:1", "4:2" }, Smod2.Config.SettingType.LIST, true, "Friendly Fire Autoban matrix of killer:victim that count as teamkills."));
		}
	}
}
