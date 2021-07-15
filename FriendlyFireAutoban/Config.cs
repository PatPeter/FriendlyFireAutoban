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
		public bool outall = false;
		public int system = 1;
		public List<TeamTuple> matrix = new List<TeamTuple>()
		{
			new TeamTuple(0, 0),
			new TeamTuple(1, 1),
			new TeamTuple(2, 2),
			new TeamTuple(3, 3),
			new TeamTuple(4, 4),
			new TeamTuple(1, 3),
			new TeamTuple(2, 4),
			new TeamTuple(3, 1),
			new TeamTuple(4, 2),
		};
		public int amount = 5;
		public int length = 1440;
		public int expire = 60;
		public Dictionary<int, int> scaled = new Dictionary<int, int>()
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
		public int noguns = 0;
		public int tospec = 0;
		public int kicker = 0;
		public HashSet<string> immune = new HashSet<string>()
		{
			"owner",
			"admin",
			"moderator"
		};
		public int bomber = 0;
		public bool disarm = false;
		public List<RoleTuple> rolewl = new List<RoleTuple>();
		public int invert = 0;
		public float mirror = 0;
		public int undead = 0;
		public int warntk = 0;
		public int votetk = 0;
		public int kdsafe = 0;

		/// <inheritdoc/>
		[Description("Enable or disable the plugin. Defaults to true.")]
		public bool IsEnabled { get; set; } = true;

		/// <summary>
		/// Output all debugging messages for FFA.
		/// </summary>
		[Description("Output all debugging messages for FFA.")]
		public bool Outall { get; private set; } = false;

		/// <summary>
		/// Change the system that FFA uses to ban.
		/// </summary>
		[Description("Change system for processing teamkills:\n(1) basic counter that will ban the player instantly upon reaching a threshold,\n(2) timer-based counter that will ban a player after reaching the threshold but will forgive 1 teamkill every `friendly_fire_autoban_expire` seconds, or\n(3) allow users to teamkill as much as possible and ban them after they have gone `friendly_fire_autoban_expire` seconds without teamkilling (will ban on round end and player disconnect).")]
		public int System { get; private set; } = 3;

		/// <summary>
		/// Gets the list of strings config.
		/// </summary>
		[Description("This is a list of strings config")]
		public List<string> Matrix { get; private set; } = new List<string>() { "First element", "Second element", "Third element" };

		/// <summary>
		/// Gets the float config.
		/// </summary>
		[Description("This is a float config")]
		public float Float { get; private set; } = 28.2f;

		/// <summary>
		/// Gets the list of ints config.
		/// </summary>
		[Description("This is a list of ints config")]
		public List<int> IntsList { get; private set; } = new List<int>() { 1, 2, 3 };

		/// <summary>
		/// Gets the dictionary of string as key and int as value config.
		/// </summary>
		[Description("This is a dictionary of strings as key and int as value config")]
		public Dictionary<string, int> StringIntDictionary { get; private set; } = new Dictionary<string, int>()
		{
			{ "First Key", 1 },
			{ "Second Key", 2 },
			{ "Third Key", 3 },
		};

		/// <summary>
		/// Gets the dictionary of string as key and <see cref="Dictionary{TKey, TValue}"/> as value config.
		/// </summary>
		[Description("This is a dictionary of strings as key and Dictionary<string, int> as value config")]
		public Dictionary<string, Dictionary<string, int>> NestedDictionaries { get; private set; } = new Dictionary<string, Dictionary<string, int>>()
		{
			{
				"First Key", new Dictionary<string, int>()
				{
					{ "First Key", 1 },
					{ "Second Key", 2 },
					{ "Third Key", 3 },
				}
			},
			{
				"Second Key", new Dictionary<string, int>()
				{
					{ "First Key", 4 },
					{ "Second Key", 5 },
					{ "Third Key", 6 },
				}
			},
			{
				"Third Key", new Dictionary<string, int>()
				{
					{ "First Key", 7 },
					{ "Second Key", 8 },
					{ "Third Key", 9 },
				}
			},
		};
	}
}
