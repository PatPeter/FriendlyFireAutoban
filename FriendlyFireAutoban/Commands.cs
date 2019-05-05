using Smod2.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FriendlyFireAutoban
{
	class ToggleCommand : ICommandHandler
	{
		private readonly FriendlyFireAutobanPlugin plugin;

		public ToggleCommand(FriendlyFireAutobanPlugin plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Toggle Friendly Fire Autoban on and off.";
		}

		public string GetUsage()
		{
			return "FRIENDLY_FIRE_AUTOBAN_TOGGLE";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			Player caller = sender as Player;

			if (this.plugin.enable)
			{
				this.plugin.enable = false;
				return new string[] { "Friendly fire autoban has been disabled." };
			}
			else
			{
				this.plugin.enable = true;
				return new string[] { "Friendly fire autoban has been enabled." };
			}

		}
	}
}
