using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Features;

namespace FriendlyFireAutoban.ConsoleCommands
{
	class ForgiveCommand
	{
		[CommandHandler(typeof(ClientCommandHandler))]
		class AccpetInviteCommand : ICommand
		{
			public string Command => "forgive";

			public string[] Aliases => new string[] { "np" };

			public string Description => "Forgive a player for a teamkill using .forgive or .np.";

			public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
			{
				//string command = ev.Command.Split(' ')[0];
				//string command = ev.Name;
				string[] quotedArgs = Regex.Matches(string.Join(" ", arguments.Array), "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'")
					.Cast<Match>()
					.Select(m => {
						return Regex.Replace(Regex.Replace(m.Value, "^\'([^\']*)\'$", "$1"), "^\"([^\"]*)\"$", "$1");
					})
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
					if (Plugin.Instance.TeamkillVictims.ContainsKey(playerUserId) &&
						Plugin.Instance.TeamkillVictims[playerUserId] != null)
					{
						Teamkill teamkill = Plugin.Instance.TeamkillVictims[playerUserId];
						if (Plugin.Instance.Teamkillers.ContainsKey(teamkill.KillerUserId))
						{
							int removedBans = Plugin.Instance.Teamkillers[teamkill.KillerUserId].Teamkills.RemoveAll(x => x.Equals(teamkill));
							if (removedBans > 0)
							{
								// No need for broadcast with return message
								//ev.Player.PersonalBroadcast(5, "You forgave this player.", false);
								// TODO: Send a broadcast to the killer
								response = string.Format(Plugin.Instance.GetTranslation("forgive_success"), teamkill.KillerName, teamkill.GetRoleDisplay());
							}
							else
							{
								response = string.Format(Plugin.Instance.GetTranslation("forgive_duplicate"), teamkill.KillerName, teamkill.GetRoleDisplay());
							}
						}
						else
						{
							response = Plugin.Instance.GetTranslation("forgive_disconnect");
						}

						// No matter what, remove this teamkill cached in the array
						Plugin.Instance.TeamkillVictims.Remove(playerUserId);
						return true;
					}
					else
					{
						response = Plugin.Instance.GetTranslation("forgive_invalid");
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
