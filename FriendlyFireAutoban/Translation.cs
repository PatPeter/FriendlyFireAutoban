using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;

namespace FriendlyFireAutoban
{
	class Translation : ITranslation
	{
		/// <summary>
		/// Ban Events - Victim Message
		/// </summary>
		[Description("Ban Events - Victim Message")]
		public readonly string victim_message = "<size=36>{0} <color=red>teamkilled</color> you at {1}. If this was an accidental teamkill, please press ~ and then type .forgive to prevent this user from being banned.</size>";

		/// <summary>
		/// Ban Events - Killer Message
		/// </summary>
		[Description("Ban Events - Killer Message")]
		public readonly string killer_message = "You teamkilled {0} {1}.";

		/// <summary>
		/// Ban Events - Killer KDR Message
		/// </summary>
		[Description("Ban Events - Killer KDR Message")]
		public readonly string killer_kdr_message = "You teamkilled {0} {1}. Because your K/D ratio is {2}, you will not be punished. Please watch your fire.";

		/// <summary>
		/// Ban Events - Killer Warning
		/// </summary>
		[Description("Ban Events - Killer Warning")]
		public readonly string killer_warning = "If you teamkill {0} more times you will be banned.";

		/// <summary>
		/// Ban Events - Killer Request
		/// </summary>
		[Description("Ban Events - Killer Request")]
		public readonly string killer_request = "Please do not teamkill.";

		/// <summary>
		/// Ban Events - NoGuns Output
		/// </summary>
		[Description("Ban Events - NoGuns Output")]
		public readonly string noguns_output = "Your guns have been removed for <color=red>teamkilling</color>. You will get them back when your teamkill expires.";

		/// <summary>
		/// Ban Events - ToSpec Output
		/// </summary>
		[Description("Ban Events - ToSpec Output")]
		public readonly string tospec_output = "You have been moved to spectate for <color=red>teamkilling</color>.";

		/// <summary>
		/// Ban Events - Undead Killer Output
		/// </summary>
		[Description("Ban Events - Undead Killer Output")]
		public readonly string undead_killer_output = "{0} has been respawned because you are <color=red>teamkilling</color> too much. If you continue, you will be banned.";

		/// <summary>
		/// Ban Events - Undead Victim Output
		/// </summary>
		[Description("Ban Events - Undead Victim Output")]
		public readonly string undead_victim_output = "You have been respawned after being teamkilled by {0}.";

		/// <summary>
		/// Ban Events - Kicker Output
		/// </summary>
		[Description("Ban Events - Kicker Output")]
		public readonly string kicker_output = "You will be kicked for <color=red>teamkilling</color>.";

		/// <summary>
		/// Ban Events - Banned Output
		/// </summary>
		[Description("Ban Events - Banned Output")]
		public readonly string banned_output = "Player {0} has been banned for <color=red>teamkilling</color> {1} players.";
		
		/// <summary>
		/// Ban Events - Offline Ban. DO NOT ADD BBCODE
		/// </summary>
		[Description("Ban Events - Offline Ban. DO NOT ADD BBCODE")]
		public readonly string offline_ban = "Banned {0} minutes for teamkilling {1} players";

		/// <summary>
		/// Teamkiller/Teamkill - Role Disarmed
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Disarmed")]
		public readonly string role_disarmed = "DISARMED ";

		/// <summary>
		/// Teamkiller/Teamkill - Role Separator
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Separator")]
		public readonly string role_separator = "on";

		/// <summary>
		/// Teamkiller/Teamkill - Role DClass
		/// </summary>
		[Description("Teamkiller/Teamkill - Role DClass")]
		public readonly string role_dclass = "<color=orange>D-CLASS</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Scientist
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Scientist")]
		public readonly string role_scientist = "<color=yellow>SCIENTIST</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Guard
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Guard")]
		public readonly string role_guard = "<color=silver>GUARD</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Cadet
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Cadet")]
		public readonly string role_cadet = "<color=cyan>CADET</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Lieutenant
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Lieutenant")]
		public readonly string role_lieutenant = "<color=aqua>LIEUTENANT</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Commander
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Commander")]
		public readonly string role_commander = "<color=blue>COMMANDER</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role NTF Scientist
		/// </summary>
		[Description("Teamkiller/Teamkill - Role NTF Scientist")]
		public readonly string role_ntf_scientist = "<color=aqua>NTF SCIENTIST</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Chaos
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Chaos")]
		public readonly string role_chaos = "<color=green>CHAOS</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Tutorial
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Tutorial")]
		public readonly string role_tutorial = "<color=lime>TUTORIAL</color>";

		/// <summary>
		/// Commands - Toggle Description
		/// </summary>
		[Description("Commands - Toggle Description")]
		public readonly string toggle_description = "Toggle Friendly Fire Autoban on and off.";

		/// <summary>
		/// Commands - Toggle Disable
		/// </summary>
		[Description("Commands - Toggle Disable")]
		public readonly string toggle_disable = "Friendly fire Autoban has been disabled.";

		/// <summary>
		/// Commands - Toggle Enable
		/// </summary>
		[Description("Commands - Toggle Enable")]
		public readonly string toggle_enable = "Friendly fire Autoban has been enabled.";

		/// <summary>
		/// Commands - Whitelist Description
		/// </summary>
		[Description("Commands - Whitelist Description")]
		public readonly string whitelist_description = "Whitelist a user from being banned by FFA until the end of the round.";

		/// <summary>
		/// Commands - Whitelist Error
		/// </summary>
		[Description("Commands - Whitelist Error")]
		public readonly string whitelist_error = "A single name or Steam ID must be provided.";

		/// <summary>
		/// Commands - Whitelist Add
		/// </summary>
		[Description("Commands - Whitelist Add")]
		//public readonly string I18N_WHITELIST_ADD = "whitelist_add";
		public readonly string whitelist_add = "Added player {0} ({1}) to ban whitelist.";

		/// <summary>
		/// Commands - Whitelist Remove
		/// </summary>
		[Description("Commands - Whitelist Remove")]
		//public readonly string I18N_WHITELIST_REMOVE = "whitelist_remove";
		public readonly string whitelist_remove = "Removed player {0} ({1}) from ban whitelist.";

		/// <summary>
		/// Client Commands - Forgive Command
		/// </summary>
		[Description("Client Commands - Forgive Command")]
		public readonly string forgive_command = "forgive";

		/// <summary>
		/// Client Commands - Forgive Success
		/// </summary>
		[Description("Client Commands - Forgive Success")]
		public readonly string forgive_success = "You have forgiven {0} {1}!";

		/// <summary>
		/// Client Commands - Forgive Duplicate
		/// </summary>
		[Description("Client Commands - Forgive Duplicate")]
		public readonly string forgive_duplicate = "You already forgave {0} {1}.";

		/// <summary>
		/// Client Commands - Forgive Disconnec
		/// </summary>
		[Description("Client Commands - Forgive Disconnect")]
		public readonly string forgive_disconnect = "The player has disconnected.";

		/// <summary>
		/// Client Commands - Forgive Invalid
		/// </summary>
		[Description("Client Commands - Forgive Invalid")]
		public readonly string forgive_invalid = "You have not been teamkilled yet.";

		/// <summary>
		/// Client Commands - TKs Command
		/// </summary>
		[Description("Client Commands - TKs Command")]
		public readonly string tks_command = "tks";

		/// <summary>
		/// Client Commands - TKs No Teamkills
		/// </summary>
		[Description("Client Commands - TKs No Teamkills")]
		public readonly string tks_no_teamkills = "No players by this name or Steam ID has any teamkills.";

		/// <summary>
		/// Client Commands - TKs Teamkill Entry
		/// </summary>
		[Description("Client Commands - TKs Teamkill Entry")]
		public readonly string tks_teamkill_entry = "({0}) {1} teamkilled {2} {3}.";

		/// <summary>
		/// Client Commands - TKs Not Found
		/// </summary>
		[Description("Client Commands - TKs Not Found")]
		public readonly string tks_not_found = "Player name not provided or not quoted.";

		/// <summary>
		/// Client Commands - FFA Disabled
		/// </summary>
		[Description("Client Commands - FFA Disabled")]
		public readonly string ffa_disabled = "Friendly Fire Autoban is currently disabled.";
	}
}
