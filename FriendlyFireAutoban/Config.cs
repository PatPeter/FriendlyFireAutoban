using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Exiled.API.Interfaces;

namespace FriendlyFireAutoban
{
	/// <inheritdoc cref="IConfig"/>
	public sealed class Config : IConfig
	{
		/// <inheritdoc/>
		[Description("Enable or disable the plugin. Defaults to true.")]
		public bool IsEnabled { get; set; } = true;

		/// <summary>
		/// Output all debugging messages for FFA.
		/// </summary>
		[Description("Output all debugging messages for FFA.")]
		public bool OutAll { get; set; } = false;

		/// <summary>
		/// Change system for processing teamkills:
		/// # 1. basic counter that will ban the player instantly upon reaching a threshold,
		/// # 2. timer-based counter that will ban a player after reaching the threshold but will forgive 1 teamkill every `friendly_fire_autoban_expire` seconds, or
		/// # 3. allow users to teamkill as much as possible and ban them after they have gone `friendly_fire_autoban_expire` seconds without teamkilling (will ban on round end and player disconnect).
		/// </summary>
		[Description("Change system for processing teamkills:\n  # (1) basic counter that will ban the player instantly upon reaching a threshold,\n  # (2) timer-based counter that will ban a player after reaching the threshold but will forgive 1 teamkill every `friendly_fire_autoban_expire` seconds, or\n  # (3) allow users to teamkill as much as possible and ban them after they have gone `friendly_fire_autoban_expire` seconds without teamkilling (will ban on round end and player disconnect).")]
		public int System { get; set; } = 2;

		/// <summary>
		/// Matrix of killer:victim team tuples that the plugin considers teamkills
		/// </summary>
		[Description("Matrix of killer:victim team tuples that the plugin considers teamkills")]
		public List<String> Matrix { get; set; } = new List<string>() {
			((int)Team.SCP + ":" + (int)Team.SCP),
			((int)Team.MTF + ":" + (int)Team.MTF),
			((int)Team.CHI + ":" + (int)Team.CHI),
			((int)Team.RSC + ":" + (int)Team.RSC),
			((int)Team.CDP + ":" + (int)Team.CDP),
			((int)Team.MTF + ":" + (int)Team.RSC),
			((int)Team.CHI + ":" + (int)Team.CDP),
			((int)Team.RSC + ":" + (int)Team.MTF),
			((int)Team.CDP + ":" + (int)Team.CHI),
		};

		internal List<TeamTuple> GetMatrix()
		{
			List<TeamTuple> retval = new List<TeamTuple>();
			foreach (string tuple in Matrix)
			{
				int[] parts = tuple.Split(':').Select(int.Parse).ToArray();
				if (parts.Length != 2)
				{
					continue;
				}

				if (Enum.IsDefined(typeof(Team), parts[0]) && Enum.IsDefined(typeof(Team), parts[0]))
				{
					retval.Add(new TeamTuple((Team)parts[0], (Team)parts[0]));
				}
			}
			return retval;
		}

		/// <summary>
		/// Amount of teamkills before a ban will be issued.
		/// </summary>
		[Description("Amount of teamkills before a ban will be issued.")]
		public int Amount { get; set; } = 5;

		/// <summary>
		/// Length of ban in minutes.
		/// </summary>
		[Description("Length of ban in minutes.")]
		public int Length { get; set; } = 1440;

		/// <summary>
		/// For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.
		/// </summary>
		[Description("For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.")]
		public int Expire { get; set; } = 60;

		/// <summary>
		/// For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.
		/// </summary>
		[Description("For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.")]
		public Dictionary<int, int> Scaled { get; set; } = new Dictionary<int, int>()
		{
			{ 4, 1440 },
			{ 5, 4320 },
			{ 6, 4320 },
			{ 7, 10080 },
			{ 8, 10080 },
			{ 9, 43800 },
			{ 10, 43800 },
			{ 11, 129600 },
			{ 12, 129600 },
			{ 13, 525600 }
		};

		/// <summary>
		/// Number of kills to remove the player's guns as a warning for teamkilling, and will remove guns every time the player picks them up or spawns with them. In ban system #1, this will remove the player's guns for the rest of the round.
		/// </summary>
		[Description("Number of kills to remove the player's guns as a warning for teamkilling, and will remove guns every time the player picks them up or spawns with them. In ban system #1, this will remove the player's guns for the rest of the round.")]
		public int NoGuns { get; set; } = 0;

		/// <summary>
		/// Number of kills at which to put a player into spectator as a warning for teamkilling.
		/// </summary>
		[Description("Number of kills at which to put a player into spectator as a warning for teamkilling.")]
		public int ToSpec { get; set; } = 0;

		/// <summary>
		/// Number of kills at which to kick the player as a warning for teamkilling.
		/// </summary>
		[Description("Number of kills at which to kick the player as a warning for teamkilling.")]
		public int Kicker { get; set; } = 0;

		/// <summary>
		/// Groups that are immune to being autobanned.
		/// </summary>
		[Description("Groups that are immune to being autobanned.")]
		public HashSet<string> Immune { get; set; } = new HashSet<string>()
		{
			//"owner",
			"admin",
			"moderator"
		};

		/// <summary>
		/// Whether to delay grenade damage of thrower by one second [experimental] (2), make player immune to grenade damage (1), or keep disabled (0).
		/// </summary>
		[Description("Whether to delay grenade damage of thrower by one second [experimental] (2), make player immune to grenade damage (1), or keep disabled (0).")]
		public int Bomber = 0;

		/// <summary>
		/// Whether disarmed players should be considered members of the opposite team and role.
		/// </summary>
		[Description("Whether disarmed players should be considered members of the opposite team and role.")]
		public bool Disarm = false;

		/// <summary>
		/// Matrix of `killer:victim` role tuples that the plugin will NOT consider teamkills.<br><br>If you want NTF to be able to teamkill based on the chain of command, use this value (on one line): <br>12:11,12:4,12:13,12:15,<br>4:11,4:13,4:15,<br>11:13,11:15,13:15
		/// </summary>
		[Description("Matrix of `killer:victim` role tuples that the plugin will NOT consider teamkills. If you want NTF to be able to teamkill based on the chain of command, use this value (on one line): <br>12:11,12:4,12:13,12:15,<br>4:11,4:13,4:15,<br>11:13,11:15,13:15")]
		public List<ValueTuple<int, int>> RoleWL = new List<ValueTuple<int, int>>
		{

		};

		internal List<RoleTuple> GetRoleWL()
		{
			List<RoleTuple> retval = new List<RoleTuple>();
			foreach (string tuple in Matrix)
			{
				int[] parts = tuple.Split(':').Select(int.Parse).ToArray();
				if (parts.Length != 2)
				{
					continue;
				}

				if (Enum.IsDefined(typeof(RoleType), parts[0]) && Enum.IsDefined(typeof(RoleType), parts[0]))
				{
					retval.Add(new RoleTuple((RoleType)parts[0], (RoleType)parts[0]));
				}
			}
			return retval;
		}

		/// <summary>
		/// Reverse Friendly Fire. If greater than 0, value of mirror will only apply after this many teamkills.
		/// </summary>
		[Description("Reverse Friendly Fire. If greater than 0, value of mirror will only apply after this many teamkills.")]
		public int Invert = 0;

		/// <summary>
		/// Whether damage should be mirrored back to a teamkiller, with values greater than (1) being considered a multiplier.
		/// </summary>
		[Description("Whether damage should be mirrored back to a teamkiller, with values greater than (1) being considered a multiplier.")]
		public float Mirror = 0;

		/// <summary>
		/// Respawns teamkilled players after this many teamkills.
		/// </summary>
		[Description("Respawns teamkilled players after this many teamkills.")]
		public int Undead = 0;

		/// <summary>
		/// How many teamkills before a ban should a teamkiller be warned (>=1), give a generic warning (0), or give no warning (-1).
		/// </summary>
		[Description("How many teamkills before a ban should a teamkiller be warned (>=1), give a generic warning (0), or give no warning (-1).")]
		public int WarnTK = 0;

		/// <summary>
		/// [not implemented yet] The number of teamkills at which to call a vote via the callvote plugin to ban a user by the ban amount.
		/// </summary>
		[Description("[not implemented yet] The number of teamkills at which to call a vote via the callvote plugin to ban a user by the ban amount.")]
		public int VoteTK = 0;

		/// <summary>
		/// The K/D ratio at which players will be immune from pre-ban and ban punishments. Takes effect when kills are greater than kdsafe, i.e. set to 2 requires a minimum of 4:2 (not 2:1), set to 3 requires a minimum of 6:2 (not 3:1), etc.
		/// </summary>
		[Description("The K/D ratio at which players will be immune from pre-ban and ban punishments. Takes effect when kills are greater than kdsafe, i.e. set to 2 requires a minimum of 4:2 (not 2:1), set to 3 requires a minimum of 6:2 (not 3:1), etc.")]
		public int KDSafe = 0;

		/// <summary>
		/// Translations for using Friendly Fire Autoban in other languages.
		/// </summary>
		/*[Description("Translations for using Friendly Fire Autoban in other languages.")]
		public Dictionary<string, string> Translations = new Dictionary<string, string>
		{
			// Ban Events
			{ "victim_message", "<size=36>{0}, <color=red>teamkilled</color> you at {1}. If this was an accidental teamkill, please press ~ and then type .forgive to prevent this user from being banned.</size>" },
			{ "killer_message", "You teamkilled {0}, {1}." },
			{ "killer_kdr_message", "You teamkilled {0}, {1}. Because your K/D ratio is {2}, you will not be punished. Please watch your fire." },
			{ "killer_warning", "If you teamkill {0}, more times you will be banned." },
			{ "killer_request", "Please do not teamkill." },
			{ "noguns_output", "Your guns have been removed for <color=red>teamkilling</color>. You will get them back when your teamkill expires." },
			{ "tospec_output", "You have been moved to spectate for <color=red>teamkilling</color>." },
			{ "undead_killer_output", "{0}, has been respawned because you are <color=red>teamkilling</color> too much. If you continue, you will be banned." },
			{ "undead_victim_output", "You have been respawned after being teamkilled by {0}." },
			{ "kicker_output", "You will be kicked for <color=red>teamkilling</color>." },
			{ "banned_output", "Player {0}, has been banned for <color=red>teamkilling</color> {1}, players." }, 

			// OFFLINE BAN, DO NOT ADD BBCODE
			{ "offline_ban", "Banned {0}, minutes for teamkilling {1}, players" }, 

			// Teamkiller/Teamkill
			{ "role_disarmed", "DISARMED " },
			{ "role_separator", "on" },
			{ "role_dclass", "<color=orange>D-CLASS</color>" },
			{ "role_scientist", "<color=yellow>SCIENTIST</color>" },
			{ "role_guard", "<color=silver>GUARD</color>" },
			{ "role_cadet", "<color=cyan>CADET</color>" },
			{ "role_lieutenant", "<color=aqua>LIEUTENANT</color>" },
			{ "role_commander", "<color=blue>COMMANDER</color>" },
			{ "role_ntf_scientist", "<color=aqua>NTF SCIENTIST</color>" },
			{ "role_chaos", "<color=green>CHAOS</color>" },
			{ "role_tutorial", "<color=lime>TUTORIAL</color>" }, 

			// Commands
			{ "toggle_description", "Toggle Friendly Fire Autoban on and off." },
			{ "toggle_disable", "Friendly fire Autoban has been disabled." },
			{ "toggle_enable", "Friendly fire Autoban has been enabled." },

			{ "whitelist_description", "Whitelist a user from being banned by FFA until the end of the round." },
			{ "whitelist_error", "A single name or Steam ID must be provided." },
			{ "whitelist_add", "Added player {0}, ({1}) to ban whitelist." },
			{ "whitelist_remove", "Removed player {0}, ({1}) from ban whitelist." }, 

			
			//Client Commands
			{ "forgive_command", "forgive" },
			{ "forgive_success", "You have forgiven {0}, {1}!" },
			{ "forgive_duplicate", "You already forgave {0}, {1}." },
			{ "forgive_disconnect", "The player has disconnected." },
			{ "forgive_invalid", "You have not been teamkilled yet." },

			{ "tks_command", "tks" },
			{ "tks_no_teamkills", "No players by this name or Steam ID has any teamkills." },
			{ "tks_teamkill_entry", "({0}) {1}, teamkilled {2}, {3}." },
			{ "tks_not_found", "Player name not provided or not quoted." },
			{ "ffa_disabled", "Friendly Fire Autoban is currently disabled." },
		};*/
	}
}
