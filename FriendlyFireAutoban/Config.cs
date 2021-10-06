using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendlyFireAutoban
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using Exiled.API.Interfaces;

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
		public bool OutAll { get; private set; } = false;

		/// <summary>
		/// Change system for processing teamkills:\n(1) basic counter that will ban the player instantly upon reaching a threshold,\n(2) timer-based counter that will ban a player after reaching the threshold but will forgive 1 teamkill every `friendly_fire_autoban_expire` seconds, or\n(3) allow users to teamkill as much as possible and ban them after they have gone `friendly_fire_autoban_expire` seconds without teamkilling (will ban on round end and player disconnect).
		/// </summary>
		[Description("Change system for processing teamkills:\n(1) basic counter that will ban the player instantly upon reaching a threshold,\n(2) timer-based counter that will ban a player after reaching the threshold but will forgive 1 teamkill every `friendly_fire_autoban_expire` seconds, or\n(3) allow users to teamkill as much as possible and ban them after they have gone `friendly_fire_autoban_expire` seconds without teamkilling (will ban on round end and player disconnect).")]
		public int System { get; private set; } = 3;

		/// <summary>
		/// Matrix of killer:victim team tuples that the plugin considers teamkills
		/// </summary>
		[Description("Matrix of killer:victim team tuples that the plugin considers teamkills")]
		public List<TeamTuple> Matrix { get; private set; } = new List<TeamTuple>() {
			new TeamTuple(Team.SCP, Team.SCP),
			new TeamTuple(Team.MTF, Team.MTF),
			new TeamTuple(Team.CHI, Team.CHI),
			new TeamTuple(Team.RSC, Team.RSC),
			new TeamTuple(Team.CDP, Team.CDP),
			new TeamTuple(Team.MTF, Team.RSC),
			new TeamTuple(Team.CHI, Team.CDP),
			new TeamTuple(Team.RSC, Team.MTF),
			new TeamTuple(Team.CDP, Team.CHI),
		};

		/// <summary>
		/// Amount of teamkills before a ban will be issued.
		/// </summary>
		[Description("Amount of teamkills before a ban will be issued.")]
		public int Amount { get; private set; } = 5;

		/// <summary>
		/// Length of ban in minutes.
		/// </summary>
		[Description("Length of ban in minutes.")]
		public int Length { get; private set; } = 1440;

		/// <summary>
		/// For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.
		/// </summary>
		[Description("For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.")]
		public int Expire { get; private set; } = 60;

		/// <summary>
		/// For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.
		/// </summary>
		[Description("For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.")]
		public Dictionary<int, int> Scaled { get; private set; } = new Dictionary<int, int>()
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
		public int NoGuns { get; private set; } = 0;

		/// <summary>
		/// Number of kills at which to put a player into spectator as a warning for teamkilling.
		/// </summary>
		[Description("Number of kills at which to put a player into spectator as a warning for teamkilling.")]
		public int ToSpec { get; private set; } = 0;

		/// <summary>
		/// Number of kills at which to kick the player as a warning for teamkilling.
		/// </summary>
		[Description("Number of kills at which to kick the player as a warning for teamkilling.")]
		public int Kicker { get; private set; } = 0;

		/// <summary>
		/// Groups that are immune to being autobanned.
		/// </summary>
		[Description("Groups that are immune to being autobanned.")]
		public HashSet<string> Immune { get; private set; } = new HashSet<string>()
		{
			"owner",
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
		public List<RoleTuple> RoleWL = new List<RoleTuple>
		{

		};

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
	}
}
