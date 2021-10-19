using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;

namespace FriendlyFireAutoban
{
	/// <inheritdoc cref="ITranslation"/>
	class Translation : ITranslation
	{
		/// <summary>
		/// Ban Events - Victim Message
		/// </summary>
		[Description("Ban Events - Victim Message")]
		public string victim_message { get; set; } = "<size=36>{0} <color=red>teamkilled</color> you at {1}. If this was an accidental teamkill, please press ~ and then type .forgive to prevent this user from being banned.</size>";

		/// <summary>
		/// Ban Events - Killer Message
		/// </summary>
		[Description("Ban Events - Killer Message")]
		public string killer_message { get; set; } = "You teamkilled {0} {1}.";

		/// <summary>
		/// Ban Events - Killer KDR Message
		/// </summary>
		[Description("Ban Events - Killer KDR Message")]
		public string killer_kdr_message { get; set; } = "You teamkilled {0} {1}. Because your K/D ratio is {2}, you will not be punished. Please watch your fire.";

		/// <summary>
		/// Ban Events - Killer Warning
		/// </summary>
		[Description("Ban Events - Killer Warning")]
		public string killer_warning { get; set; } = "If you teamkill {0} more times you will be banned.";

		/// <summary>
		/// Ban Events - Killer Request
		/// </summary>
		[Description("Ban Events - Killer Request")]
		public string killer_request { get; set; } = "Please do not teamkill.";

		/// <summary>
		/// Ban Events - NoGuns Output
		/// </summary>
		[Description("Ban Events - NoGuns Output")]
		public string noguns_output { get; set; } = "Your guns have been removed for <color=red>teamkilling</color>. You will get them back when your teamkill expires.";

		/// <summary>
		/// Ban Events - ToSpec Output
		/// </summary>
		[Description("Ban Events - ToSpec Output")]
		public string tospec_output { get; set; } = "You have been moved to spectate for <color=red>teamkilling</color>.";

		/// <summary>
		/// Ban Events - Undead Killer Output
		/// </summary>
		[Description("Ban Events - Undead Killer Output")]
		public string undead_killer_output { get; set; } = "{0} has been respawned because you are <color=red>teamkilling</color> too much. If you continue, you will be banned.";

		/// <summary>
		/// Ban Events - Undead Victim Output
		/// </summary>
		[Description("Ban Events - Undead Victim Output")]
		public string undead_victim_output { get; set; } = "You have been respawned after being teamkilled by {0}.";

		/// <summary>
		/// Ban Events - Kicker Output
		/// </summary>
		[Description("Ban Events - Kicker Output")]
		public string kicker_output { get; set; } = "You will be kicked for <color=red>teamkilling</color>.";

		/// <summary>
		/// Ban Events - Banned Output
		/// </summary>
		[Description("Ban Events - Banned Output")]
		public string banned_output { get; set; } = "Player {0} has been banned for <color=red>teamkilling</color> {1} players.";
		
		/// <summary>
		/// Ban Events - Offline Ban. DO NOT ADD BBCODE
		/// </summary>
		[Description("Ban Events - Offline Ban. DO NOT ADD BBCODE")]
		public string offline_ban { get; set; } = "Banned {0} minutes for teamkilling {1} players";

		/// <summary>
		/// Teamkiller/Teamkill - Role Disarmed
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Disarmed")]
		public string role_disarmed { get; set; } = "DISARMED ";

		/// <summary>
		/// Teamkiller/Teamkill - Role Separator
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Separator")]
		public string role_separator { get; set; } = "on";

		/// <summary>
		/// Teamkiller/Teamkill - Role DClass
		/// </summary>
		[Description("Teamkiller/Teamkill - Role DClass")]
		public string role_dclass { get; set; } = "<color=orange>D-CLASS</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Scientist
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Scientist")]
		public string role_scientist { get; set; } = "<color=yellow>SCIENTIST</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Guard
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Guard")]
		public string role_guard { get; set; } = "<color=silver>GUARD</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Cadet
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Cadet")]
		public string role_cadet { get; set; } = "<color=cyan>CADET</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Lieutenant
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Lieutenant")]
		public string role_lieutenant { get; set; } = "<color=aqua>LIEUTENANT</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Commander
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Commander")]
		public string role_commander { get; set; } = "<color=blue>COMMANDER</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role NTF Scientist
		/// </summary>
		[Description("Teamkiller/Teamkill - Role NTF Scientist")]
		public string role_ntf_scientist { get; set; } = "<color=aqua>NTF SCIENTIST</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Chaos
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Chaos")]
		public string role_chaos { get; set; } = "<color=green>CHAOS</color>";

		/// <summary>
		/// Teamkiller/Teamkill - Role Tutorial
		/// </summary>
		[Description("Teamkiller/Teamkill - Role Tutorial")]
		public string role_tutorial { get; set; } = "<color=lime>TUTORIAL</color>";

		/// <summary>
		/// Commands - Toggle Description
		/// </summary>
		[Description("Commands - Toggle Description")]
		public string toggle_description { get; set; } = "Toggle Friendly Fire Autoban on and off.";

		/// <summary>
		/// Commands - Toggle Disable
		/// </summary>
		[Description("Commands - Toggle Disable")]
		public string toggle_disable { get; set; } = "Friendly fire Autoban has been disabled.";

		/// <summary>
		/// Commands - Toggle Enable
		/// </summary>
		[Description("Commands - Toggle Enable")]
		public string toggle_enable { get; set; } = "Friendly fire Autoban has been enabled.";

		/// <summary>
		/// Commands - Whitelist Description
		/// </summary>
		[Description("Commands - Whitelist Description")]
		public string whitelist_description { get; set; } = "Whitelist a user from being banned by FFA until the end of the round.";

		/// <summary>
		/// Commands - Whitelist Error
		/// </summary>
		[Description("Commands - Whitelist Error")]
		public string whitelist_error { get; set; } = "A single name or Steam ID must be provided.";

		/// <summary>
		/// Commands - Whitelist Add
		/// </summary>
		[Description("Commands - Whitelist Add")]
		//public string I18N_WHITELIST_ADD = "whitelist_add";
		public string whitelist_add { get; set; } = "Added player {0} ({1}) to ban whitelist.";

		/// <summary>
		/// Commands - Whitelist Remove
		/// </summary>
		[Description("Commands - Whitelist Remove")]
		//public string I18N_WHITELIST_REMOVE = "whitelist_remove";
		public string whitelist_remove { get; set; } = "Removed player {0} ({1}) from ban whitelist.";

		/// <summary>
		/// Client Commands - Forgive Command
		/// </summary>
		[Description("Client Commands - Forgive Command")]
		public string forgive_command { get; set; } = "forgive";

		/// <summary>
		/// Client Commands - Forgive Success
		/// </summary>
		[Description("Client Commands - Forgive Success")]
		public string forgive_success { get; set; } = "You have forgiven {0} {1}!";

		/// <summary>
		/// Client Commands - Forgive Duplicate
		/// </summary>
		[Description("Client Commands - Forgive Duplicate")]
		public string forgive_duplicate { get; set; } = "You already forgave {0} {1}.";

		/// <summary>
		/// Client Commands - Forgive Disconnec
		/// </summary>
		[Description("Client Commands - Forgive Disconnect")]
		public string forgive_disconnect { get; set; } = "The player has disconnected.";

		/// <summary>
		/// Client Commands - Forgive Invalid
		/// </summary>
		[Description("Client Commands - Forgive Invalid")]
		public string forgive_invalid { get; set; } = "You have not been teamkilled yet.";

		/// <summary>
		/// Client Commands - TKs Command
		/// </summary>
		[Description("Client Commands - TKs Command")]
		public string tks_command { get; set; } = "tks";

		/// <summary>
		/// Client Commands - TKs No Teamkills
		/// </summary>
		[Description("Client Commands - TKs No Teamkills")]
		public string tks_no_teamkills { get; set; } = "No players by this name or Steam ID has any teamkills.";

		/// <summary>
		/// Client Commands - TKs Teamkill Entry
		/// </summary>
		[Description("Client Commands - TKs Teamkill Entry")]
		public string tks_teamkill_entry { get; set; } = "({0}) {1} teamkilled {2} {3}.";

		/// <summary>
		/// Client Commands - TKs Not Found
		/// </summary>
		[Description("Client Commands - TKs Not Found")]
		public string tks_not_found { get; set; } = "Player name not provided or not quoted.";

		/// <summary>
		/// Client Commands - FFA Disabled
		/// </summary>
		[Description("Client Commands - FFA Disabled")]
		public string ffa_disabled { get; set; } = "Friendly Fire Autoban is currently disabled.";
	}
}
