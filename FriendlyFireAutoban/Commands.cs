using Smod2.API;
using Smod2.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
			return this.plugin.GetTranslation("toggle_description");
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
				return new string[] { this.plugin.GetTranslation("toggle_disable") };
			}
			else
			{
				this.plugin.enable = true;
				return new string[] { this.plugin.GetTranslation("toggle_enable") };
			}
		}
	}

	class WhitelistCommand : ICommandHandler
	{
		private readonly FriendlyFireAutobanPlugin plugin;

		public WhitelistCommand(FriendlyFireAutobanPlugin plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return this.plugin.GetTranslation("whitelist_description");
		}

		public string GetUsage()
		{
			return "FRIENDLY_FIRE_AUTOBAN_WHITELIST";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			Player caller = sender as Player;

			if (args.Length == 1)
			{
				List<Teamkiller> teamkillers = new List<Teamkiller>();
				try
				{
					if (Regex.Match(args[0], "^[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]$").Success)
					{
						// https://stackoverflow.com/questions/55436309/how-do-i-use-linq-to-select-from-a-list-inside-a-map
						teamkillers = this.plugin.Teamkillers.Values.Where(
							x => x.SteamId.Equals(args[0])
						).ToList();
					}
					else
					{
						// https://stackoverflow.com/questions/55436309/how-do-i-use-linq-to-select-from-a-list-inside-a-map
						teamkillers = this.plugin.Teamkillers.Values.Where(
							x => x.Name.Contains(args[0])
						).ToList();
					}
				}
				catch (Exception e)
				{
					if (this.plugin.outall)
					{
						this.plugin.Error(e.Message);
						this.plugin.Error(e.StackTrace);
					}
				}

				if (teamkillers.Count == 1)
				{
					if (!this.plugin.banWhitelist.Contains(teamkillers[0].SteamId))
					{
						this.plugin.banWhitelist.Add(teamkillers[0].SteamId);
						return new string[] { string.Format(this.plugin.GetTranslation("whitelist_add"), teamkillers[0].Name, teamkillers[0].SteamId) };
					}
					else
					{
						this.plugin.banWhitelist.Remove(teamkillers[0].SteamId);
						return new string[] { string.Format(this.plugin.GetTranslation("whitelist_remove"), teamkillers[0].Name, teamkillers[0].SteamId) };
					}
				}
				else
				{
					return new string[] { this.plugin.GetTranslation("whitelist_error") };
				}
			}
			else
			{
				return new string[] { this.plugin.GetTranslation("whitelist_error") };
			}
		}
	}
}
