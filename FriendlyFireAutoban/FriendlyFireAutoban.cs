using FriendlyFireAutoban.EventHandlers;
using Smod2;
using Smod2.Attributes;
using Smod2.Events;
using System.Collections.Generic;

namespace FriendlyFireAutoban
{
	[PluginDetails(
		author = "PatPeter",
		name = "Test",
		description = "Friendly Fire Autoban",
		id = "patpeter.friendly.fire.autoban",
		version = "1.0-build4",
		SmodMajor = 2,
		SmodMinor = 2,
		SmodRevision = 1
		)]
	class FriendlyFireAutobanPlugin : Plugin
	{
		internal bool duringRound = false;
		internal Dictionary<string, int> teamkillCounter = new Dictionary<string, int>();

		public override void OnEnable()
		{
			this.Info("Friendly Fire Autoban has loaded :)");
			this.Info("friendly_fire_autoban_enable value: " + this.GetConfigBool("friendly_fire_autoban_enable"));
			this.Info("friendly_fire_autoban_amount value: " + this.GetConfigInt("friendly_fire_autoban_amount"));
			this.Info("friendly_fire_autoban_length value: " + this.GetConfigInt("friendly_fire_autoban_length"));
		}

		public override void OnDisable()
		{

		}

		public override void Register()
		{
			// Register Events
			this.AddEventHandler(typeof(IEventRoundStart), new RoundStartHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventRoundEnd), new RoundEndHandler(this), Priority.Highest);
			this.AddEventHandler(typeof(IEventPlayerDie), new PlayerDieHandler(this), Priority.Highest);
			// Register config settings
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_enable", true, Smod2.Config.SettingType.BOOL, true, "Enable Friendly Fire Autoban."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_amount", 5, Smod2.Config.SettingType.NUMERIC, true, "Friendly Fire Autoban amount of teamkills before a ban."));
			this.AddConfig(new Smod2.Config.ConfigSetting("friendly_fire_autoban_length", 3600, Smod2.Config.SettingType.NUMERIC, true, "Friendly Fire Autoban length in seconds."));
		}
	}
}
