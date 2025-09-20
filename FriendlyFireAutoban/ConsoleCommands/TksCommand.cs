using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandSystem;
using PluginAPI.Core;

namespace FriendlyFireAutoban.ConsoleCommands
{
	class TksCommand
	{
		[CommandHandler(typeof(ClientCommandHandler))]
		class AccpetInviteCommand : ICommand
		{
			public string Command => "tks";

			public string[] Aliases => new string[] { "teamkills" };

			public string Description => "View a player's teamkills.";

			public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
			{
				//string command = ev.Command.Split(' ')[0];
				//string command = ev.Name;
				string[] quotedArgs = Regex.Matches(string.Join(" ", arguments.Array), "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'")
					.Cast<Match>()
					.Select(m => m.Value)
					.ToArray()
					.Skip(1)
					.ToArray();
				Player player = Player.Get(((CommandSender)sender).SenderId);
				String playerUserId = player.UserId;

				if (Plugin.Instance.Config.OutAll)
				{
					Log.Info("Quoted Args for command: " + string.Join(" | ", quotedArgs));
				}

				if (Plugin.Instance.Config.IsEnabled)
				{
					if (quotedArgs.Length == 1)
					{
						List<Teamkiller> teamkillers = new List<Teamkiller>();
						try
						{
							if (Regex.Match(quotedArgs[0], "^[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]$").Success)
							{
								// https://stackoverflow.com/questions/55436309/how-do-i-use-linq-to-select-from-a-list-inside-a-map
								teamkillers = Plugin.Instance.Teamkillers.Values.Where(
									x => x.UserId.Equals(quotedArgs[0])
								).ToList();
							}
							else
							{
								// https://stackoverflow.com/questions/55436309/how-do-i-use-linq-to-select-from-a-list-inside-a-map
								teamkillers = Plugin.Instance.Teamkillers.Values.Where(
									x => x.Nickname.Contains(quotedArgs[0])
								).ToList();
							}
						}
						catch (Exception e)
						{
							if (Plugin.Instance.Config.OutAll)
							{
								Log.Error(e.Message);
								Log.Error(e.StackTrace);
							}
						}

						if (teamkillers.Count == 1)
						{
							string retval = "Player " + teamkillers[0].Nickname + " has a K/D ratio of " + teamkillers[0].Kills + ":" + teamkillers[0].Deaths + " or " + teamkillers[0].GetKDR() + ".\n";
							foreach (Teamkill tk in teamkillers[0].Teamkills)
							{
								retval +=
									string.Format(
										Plugin.Instance.GetTranslation("tks_teamkill_entry"),
										(tk.Duration / 60) + ":" + (tk.Duration % 60),
										tk.KillerName,
										tk.VictimName,
										tk.GetRoleDisplay()
									) + "\n";
							}
							response = retval;
							return true;
						}
						else
						{
							response = Plugin.Instance.GetTranslation("tks_no_teamkills");
							return false;
						}
					}
					else
					{
						response = Plugin.Instance.GetTranslation("tks_not_found");
						return false;
					}
				}
				else
				{
					response = Plugin.Instance.GetTranslation("ffa_disabled");
					return false;
				}
			}
		}
	}
}
