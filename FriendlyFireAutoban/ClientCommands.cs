using Smod2;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FriendlyFireAutoban
{
	class CallCommandHandler : IEventHandlerCallCommand
	{
		private FriendlyFireAutobanPlugin plugin;

		public CallCommandHandler(Plugin plugin)
		{
			this.plugin = (FriendlyFireAutobanPlugin)plugin;
		}

		public void OnCallCommand(PlayerCallCommandEvent ev)
		{
			string command = ev.Command.Split(' ')[0];
			string[] quotedArgs = Regex.Matches(ev.Command, "[^\\s\"\']+|\"([^\"]*)\"|\'([^\']*)\'")
				.Cast<Match>()
				.Select(m => m.Value)
				.ToArray()
				.Skip(1)
				.ToArray();

			switch (command)
			{
				case "forgive":
					if (this.plugin.enable)
					{
						if (this.plugin.TeamkillVictims.ContainsKey(ev.Player.SteamId))
						{
							Teamkill teamkill = this.plugin.TeamkillVictims[ev.Player.SteamId];
							if (teamkill != null)
							{

								if (this.plugin.Teamkillers.ContainsKey(teamkill.KillerSteamId))
								{
									int removedBans = this.plugin.Teamkillers[teamkill.KillerSteamId].Teamkills.RemoveAll(x => x.Equals(teamkill));
									if (removedBans > 0)
									{
										// No need for broadcast with return message
										//ev.Player.PersonalBroadcast(5, "You forgave this player.", false);
										// TODO: Send a broadcast to the killer
										ev.ReturnMessage = "You have forgiven " + teamkill.KillerName + " " + teamkill.GetRoleDisplay() + "!";
									}
									else
									{
										ev.ReturnMessage = "You already forgave " + teamkill.KillerName + " " + teamkill.GetRoleDisplay() + ".";
									}
								}
								else
								{
									ev.ReturnMessage = "The player has disconnected.";
								}
							}
							else
							{
								ev.ReturnMessage = "You have not been teamkilled yet.";
							}

							// No matter what, remove this teamkill cached in the array
							this.plugin.TeamkillVictims.Remove(ev.Player.SteamId);
						}
						else
						{
							ev.ReturnMessage = "There is no teamkill for you to forgive.";
						}
					}
					else
					{
						ev.ReturnMessage = "Friendly Fire Autoban is currently disabled.";
					}
					break;

				case "tks":

					if (this.plugin.enable)
					{
						if (quotedArgs.Length == 1)
						{
							List<Teamkill> teamkills = new List<Teamkill>();
							try
							{
								// https://stackoverflow.com/questions/55436309/how-do-i-use-linq-to-select-from-a-list-inside-a-map
								teamkills = this.plugin.Teamkillers.SelectMany(
									x => x.Value.Teamkills.Where(
										y => y.KillerName.Contains(quotedArgs[0])
									)
								).ToList();
							}
							catch (Exception e)
							{
								if (this.plugin.outall)
								{
									this.plugin.Error(e.Message);
									this.plugin.Error(e.StackTrace);
								}
							}

							if (teamkills.Count == 0)
							{
								ev.ReturnMessage = "No players by this name has any teamkills.";
							}
							else
							{
								string retval = "";
								foreach (Teamkill tk in teamkills)
								{
									retval += tk.KillerName + " teamkilled " + tk.VictimName + " " + tk.GetRoleDisplay() + ". \n";
								}
								ev.ReturnMessage = retval;
							}
						}
						else
						{
							ev.ReturnMessage = "Player name not provided or not quoted.";
						}
					}
					else
					{
						ev.ReturnMessage = "Friendly Fire Autoban is currently disabled.";
					}
					break;
			}
		}
	}
}
