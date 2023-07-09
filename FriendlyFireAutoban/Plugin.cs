using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using MEC;
using static BanHandler;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using System.Runtime.CompilerServices;
using PluginAPI.Core.Items;
using InventorySystem.Items;
using PluginAPI.Events;
using JetBrains.Annotations;
using System.Collections.Concurrent;

namespace FriendlyFireAutoban
{
	public class Plugin
	{
		public static Plugin Instance { get; private set; }

        [PluginConfig]
        public Config Config;

        internal EventHandlers EventHandlers;

        /*
		 * Public Instance Fields
		 */
        public string Name { get; } = FriendlyFireAutoban.AssemblyInfo.Author;
		public string Author { get; } = FriendlyFireAutoban.AssemblyInfo.Author;
		public Version Version { get; } = new Version(FriendlyFireAutoban.AssemblyInfo.Version);
		public string Prefix { get; } = FriendlyFireAutoban.AssemblyInfo.ConfigPrefix;
		//public Version RequiredExiledVersion { get; } = new Version(5, 1, 3);
		//public PluginPriority Priority { get; } = PluginPriority.Default;

        /*
		 * Internal Instance Fields
		 */
        internal bool DuringRound = false;
		internal bool ProcessingDisconnect = false;
		internal CoroutineHandle FFAHandle = new CoroutineHandle();

		internal IDictionary<string, Teamkiller> Teamkillers = new ConcurrentDictionary<string, Teamkiller>();
		internal IDictionary<string, Teamkill> TeamkillVictims = new ConcurrentDictionary<string, Teamkill>();

		internal ISet<string> BanWhitelist = new HashSet<string>();

		readonly internal IDictionary<Team, Team> InverseTeams = new Dictionary<Team, Team>()
		{
			{ Team.SCPs, Team.SCPs },
			{ Team.FoundationForces, Team.ChaosInsurgency },
			{ Team.ChaosInsurgency, Team.FoundationForces },
			{ Team.Scientists, Team.ClassD },
			{ Team.ClassD, Team.Scientists },
			{ Team.Dead, Team.Dead },
			{ Team.OtherAlive, Team.OtherAlive },
		};
		readonly internal IDictionary<RoleTypeId, RoleTypeId> InverseRoles = new Dictionary<RoleTypeId, RoleTypeId>()
		{
			{ RoleTypeId.None, RoleTypeId.None },
			{ RoleTypeId.Spectator, RoleTypeId.Spectator },
			{ RoleTypeId.Tutorial, RoleTypeId.Tutorial },
			// ClassD/Scientist
			{ RoleTypeId.ClassD, RoleTypeId.Scientist },
			{ RoleTypeId.Scientist, RoleTypeId.ClassD },
			// NTF to Chaos
			{ RoleTypeId.FacilityGuard, RoleTypeId.ChaosConscript },
			{ RoleTypeId.NtfPrivate, RoleTypeId.ChaosConscript },
			{ RoleTypeId.NtfSpecialist, RoleTypeId.ChaosRifleman },
			{ RoleTypeId.NtfSergeant, RoleTypeId.ChaosRepressor },
			{ RoleTypeId.NtfCaptain, RoleTypeId.ChaosMarauder },
			// Chaos to NTF
			{ RoleTypeId.ChaosConscript, RoleTypeId.NtfPrivate },
			{ RoleTypeId.ChaosRifleman, RoleTypeId.NtfSpecialist },
			{ RoleTypeId.ChaosRepressor, RoleTypeId.NtfSergeant },
			{ RoleTypeId.ChaosMarauder, RoleTypeId.NtfCaptain },
			// SCPs
			{ RoleTypeId.Scp049, RoleTypeId.Scp049 },
			{ RoleTypeId.Scp0492, RoleTypeId.Scp0492 },
			{ RoleTypeId.Scp079, RoleTypeId.Scp079 },
			{ RoleTypeId.Scp096, RoleTypeId.Scp096 },
			{ RoleTypeId.Scp106, RoleTypeId.Scp106 },
			{ RoleTypeId.Scp173, RoleTypeId.Scp173 },
			{ RoleTypeId.Scp939, RoleTypeId.Scp939 },
		};

		public string GetTranslation(string name)
		{
			Type t = typeof(FriendlyFireAutoban.Translation);
			PropertyInfo p = t.GetProperty(name);
			// Plugin.Instance.Config.Translations.ContainsKey(name)
			if (p != null)
			{
				//return (string) p.GetValue(Translation);
				return Plugin.Instance.Config.Translations[name];
			}
            else
            {
                return $"INVALID TRANSLATION: {name}";
            }
        }

        [PluginEntryPoint(FriendlyFireAutoban.AssemblyInfo.Name, FriendlyFireAutoban.AssemblyInfo.Version, FriendlyFireAutoban.AssemblyInfo.Description, FriendlyFireAutoban.AssemblyInfo.Author)]
        public void OnEnabled()
        {
			Instance = this;
			Config = new Config();

			try
            {
                Log.Debug("Initializing event handlers..");
                //Set instance varible to a new instance, this should be nulled again in OnDisable
                EventHandlers = new EventHandlers(this);
				EventManager.RegisterEvents(this, EventHandlers);
				//Hook the events you will be using in the plugin. You should hook all events you will be using here, all events should be unhooked in OnDisabled
				/*Exiled.Events.Handlers.Server.ReloadedConfigs += EventHandlers.OnReloadedConfig;

				Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
				Exiled.Events.Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;

				Exiled.Events.Handlers.Player.Verified += EventHandlers.OnPlayerVerified;
				Exiled.Events.Handlers.Player.Destroying += EventHandlers.OnPlayerDestroying;

				Exiled.Events.Handlers.Player.Hurting += EventHandlers.OnPlayerHurting;
				Exiled.Events.Handlers.Player.Dying += EventHandlers.OnPlayerDying;

				Exiled.Events.Handlers.Player.Spawning += EventHandlers.OnPlayerSpawn;
				Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnSetClass;
				Exiled.Events.Handlers.Player.PickingUpItem += EventHandlers.OnPickupItem;*/

				//Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += EventHandlers.OnRACommand;
				//Exiled.Events.Handlers.Server.SendingConsoleCommand += EventHandlers.OnConsoleCommand;

				Log.Info($"{AssemblyInfo.Name} v{AssemblyInfo.Version} by {AssemblyInfo.Author} has been enabled!");
			}
			catch (Exception e)
			{
				//This try catch is redundant, as EXILED will throw an error before this block can, but is here as an example of how to handle exceptions/errors
				Log.Error($"There was an error loading the plugin: {e}");
			}
		}

        [PluginUnload]
        public void OnDisabled()
		{
			/*Exiled.Events.Handlers.Server.ReloadedConfigs -= EventHandlers.OnReloadedConfig;

			Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;

			Exiled.Events.Handlers.Player.Verified -= EventHandlers.OnPlayerVerified;
			Exiled.Events.Handlers.Player.Destroying -= EventHandlers.OnPlayerDestroying;
			
			Exiled.Events.Handlers.Player.Hurting -= EventHandlers.OnPlayerHurting;
			Exiled.Events.Handlers.Player.Dying -= EventHandlers.OnPlayerDying;

			Exiled.Events.Handlers.Player.Spawning -= EventHandlers.OnPlayerSpawn;
			Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnSetClass;
			Exiled.Events.Handlers.Player.PickingUpItem -= EventHandlers.OnPickupItem;*/

			//Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= EventHandlers.OnRACommand;
			//Exiled.Events.Handlers.Server.SendingConsoleCommand -= EventHandlers.OnConsoleCommand;

			EventHandlers = null;
		}

        [PluginReload]
        public void OnReloadedConfig()
        {
            //This is only fired when you use the EXILED reload command, the reload command will call OnDisable, OnReload, reload the plugin, then OnEnable in that order. There is no GAC bypass, so if you are updating a plugin, it must have a unique assembly name, and you need to remove the old version from the plugins folder
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.is_enabled: {Plugin.Instance.Config.IsEnabled}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.out_all: {Plugin.Instance.Config.OutAll}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.system: {Plugin.Instance.Config.System}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.matrix: {string.Join(",", Plugin.Instance.Config.Matrix)}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.amount: {Plugin.Instance.Config.Amount}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.length: {Plugin.Instance.Config.Length}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.expire: {Plugin.Instance.Config.Expire}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.scaled: {string.Join(",", Plugin.Instance.Config.Scaled)}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.no_guns: {Plugin.Instance.Config.NoGuns}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.to_spec: {Plugin.Instance.Config.ToSpec}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.kicker: {Plugin.Instance.Config.Kicker}");
            //Log.Debug($"{AssemblyInfo.ConfigPrefix}.immune: {string.Join(",", Plugin.Instance.Config.Immune)}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.bomber: {Plugin.Instance.Config.Bomber}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.disarm: {Plugin.Instance.Config.Disarm}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.role_wl: {string.Join(",", Plugin.Instance.Config.RoleWl)}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.invert: {Plugin.Instance.Config.Invert}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.mirror: {Plugin.Instance.Config.Mirror}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.undead: {Plugin.Instance.Config.Undead}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.warn_tk: {Plugin.Instance.Config.WarnTk}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.vote_tk: {Plugin.Instance.Config.VoteTk}");
            Log.Debug($"{AssemblyInfo.ConfigPrefix}.kd_safe: {Plugin.Instance.Config.KdSafe}");
        }

        public void PrintConfigs()
		{
			Log.Info("friendly_fire_autoban.enable value: " + Plugin.Instance.Config.IsEnabled);
			Log.Info("friendly_fire_autoban.system value: " + Plugin.Instance.Config.System);

			Log.Info("friendly_fire_autoban.matrix default value: " + string.Join(";", Plugin.Instance.Config.Matrix));
			string matrix = "";
			foreach (TeamTuple tt in Plugin.Instance.Config.GetMatrixCache())
			{
				if (matrix.Length == 0)
				{
					matrix += tt;
				}
				else
				{
					matrix += ";" + tt;
				}
			}
			Log.Info("friendly_fire_autoban.matrix cached  value: " + matrix);

			Log.Info("friendly_fire_autoban.amount value: " + Plugin.Instance.Config.Amount);
			Log.Info("friendly_fire_autoban.length value: " + Plugin.Instance.Config.Length);
			Log.Info("friendly_fire_autoban.expire value: " + Plugin.Instance.Config.Expire);

			string scaled = "";
			foreach (KeyValuePair<int, int> scale in Plugin.Instance.Config.Scaled)
			{
				if (scaled.Length == 0)
				{
					scaled += scale.Key + ":" + scale.Value;
				}
				else
				{
					scaled += ',' + scale.Key + ":" + scale.Value;
				}
			}
			Log.Info("friendly_fire_autoban.scaled value: " + scaled);

			Log.Info("friendly_fire_autoban.noguns value: " + Plugin.Instance.Config.NoGuns);
			Log.Info("friendly_fire_autoban.tospec value: " + Plugin.Instance.Config.ToSpec);
			Log.Info("friendly_fire_autoban.kicker value: " + Plugin.Instance.Config.Kicker);
			Log.Info("friendly_fire_autoban.bomber value: " + Plugin.Instance.Config.Bomber);
			Log.Info("friendly_fire_autoban.disarm value: " + Plugin.Instance.Config.Disarm);

			Log.Info("friendly_fire_autoban.rolewl default value: " + string.Join(";", Plugin.Instance.Config.RoleWl));
			string roleTuples = "";
			foreach (RoleTuple rt in Plugin.Instance.Config.GetRoleWlCache())
			{
				if (roleTuples.Length == 0)
				{
					roleTuples += rt;
				}
				else
				{
					roleTuples += ";" + rt;
				}
			}
			Log.Info("friendly_fire_autoban.rolewl cached  value: " + roleTuples);

			Log.Info("friendly_fire_autoban.invert value: " + Plugin.Instance.Config.Invert);
			Log.Info("friendly_fire_autoban.mirror value: " + Plugin.Instance.Config.Mirror);
			Log.Info("friendly_fire_autoban.undead value: " + Plugin.Instance.Config.Undead);
			Log.Info("friendly_fire_autoban.warntk value: " + Plugin.Instance.Config.WarnTk);
			Log.Info("friendly_fire_autoban.votetk value: " + Plugin.Instance.Config.VoteTk);
			//Log.Info("friendly_fire_autoban.immune value: " + string.Join(",", Plugin.Instance.Config.Immune));
		}

		internal IEnumerator<float> FFACoRoutine()
		{
			for (; ; )
			{
				//List<Player> players = Player.List.Where(p => Plugin.Instance.Teamkillers.ContainsKey(p.UserId) && Plugin.Instance.Teamkillers[p.UserId].Teamkills.Count > 0).Distinct().ToList();
				List<Teamkiller> teamkillers = Plugin.Instance.Teamkillers.Values.Where(tker => tker.Teamkills.Count > 0).Distinct().ToList();

				foreach (Teamkiller killer in teamkillers)
				{
					string killerUserId = killer.UserId;
					string killerIpAddress = killer.IPAddress;
					string killerNickname = killer.Nickname;
					Team killerTeam = killer.Team;
					string killerOutput = $"{killerNickname} {killerUserId} {killerIpAddress}";
					Teamkiller killerTeamkiller = Plugin.Instance.Teamkillers[killer.UserId];

					if (killerTeamkiller.TimerCountdown > 0)
					{
						killerTeamkiller.TimerCountdown--;
						// Decrease teamkiller timer by 1 second
						//if (Plugin.Instance.Config.OutAll)
						//{
						//	Log.Info("Decrease timer for " + killerTeamkiller + " from " + killerTeamkiller.TimerCountdown + " to " + (killerTeamkiller.TimerCountdown - 1));
						//}
					}
					else if (killerTeamkiller.TimerCountdown == 0)
					{
						killerTeamkiller.TimerCountdown--;
						/*
						 * If ban system is #3, every player teamkill cancels and restarts the timer
						 * Wait until the timer expires after the teamkilling has ended to find out 
						 * how much teamkilling the player has done.
						 */
						if (Plugin.Instance.Config.System == 3)
						{
							int banLength = Plugin.Instance.GetScaledBanAmount(killerUserId);
							if (banLength > 0 && !killerTeamkiller.Banned)
							{
								Plugin.Instance.OnBan(killer, killerNickname, banLength);
								Log.Info($"Banned player {killerTeamkiller} for accumulating scaled ban amount {banLength} for {killerTeamkiller.Teamkills.Count} teamkills.");
								//continue;
							}
							//else
							//{
							//	if (Plugin.Instance.Config.OutAll)
							//	{
							//		Log.Info("Player " + killerUserId + " " + killerTeamkiller.Teamkills.Count + " teamkills is not bannable.");
							//	}
							//}
						}
						
						// Forgive teamkills in ban system #2 and #3
						// Continue to forgive teamkills after players leave the server
						if (Plugin.Instance.Config.System > 1)
						{
							Teamkill firstTeamkill = killerTeamkiller.Teamkills[0];
							killerTeamkiller.Teamkills.RemoveAt(0);
							Log.Info($"Player {killerOutput} {killerTeam} teamkill {firstTeamkill} expired, counter now at {killerTeamkiller.Teamkills.Count}.");
						}

						if (killerTeamkiller.Teamkills.Count > 0)
						{
							killerTeamkiller.TimerCountdown = Plugin.Instance.Config.Expire;
						}
					}
				}
				
				yield return Timing.WaitForSeconds(1f);
			}
		}

		internal bool isImmune(Player player)
		{
			UserGroup ug = player.ReferenceHub.serverRoles.Group;

			//if (Plugin.Instance.Config.Immune.Contains(player.GroupName) || (player.GlobalBadge.HasValue ? Plugin.Instance.Config.Immune.Contains(player.GlobalBadge.Value.Text) : false))
			/*if (player.CheckPermission("ffa.immune"))
			{
				return true;
			}
			else
			{
				return false;
			}*/

			ISet<string> immuneRanks = Plugin.Instance.Config.Immune;
			foreach (string rank in immuneRanks)
			{
				if (Plugin.Instance.Config.OutAll)
				{
					Log.Info("Does immune rank " + rank + " equal " + ug.BadgeText + "?");
				}
				if (String.Equals(rank, ug.BadgeText, StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsTeamkill(Player killer, Player victim, bool death)
		{
			Teamkiller teamkiller = Plugin.Instance.AddAndGetTeamkiller(killer);
			if (teamkiller == null)
			{
				Log.Warning("[IsTeamkill] Null player returned from AddAndGetTeamkiller.");
				return false;
			}

			string killerUserId = killer.UserId;
			Team killerTeam = killer.Team;
			RoleTypeId killerRole = killer.Role;

			string victimUserId = victim.UserId;
			Team victimTeam = victim.Team;
			RoleTypeId victimRole = victim.Role;

			if (string.Equals(killerUserId, victimUserId))
			{
				if (death) Log.Info($"{killerUserId} equals {victimUserId}, this is a suicide and not a teamkill.");
				return false;
			}

			if (Plugin.Instance.Config.Disarm && victim.IsDisarmed)
			{
				victimTeam = this.InverseTeams[victimTeam];
				victimRole = this.InverseRoles[victimRole];
				if (death) Log.Info($"{victimUserId} is handcuffed, team inverted to {victimTeam} and role {victimRole}.");
			}

			//List<RoleTuple> roleTuples = new List<RoleTuple>();
			//foreach (string rawRoleTuple in Plugin.Instance.Config.RoleWL)
			//{
			//	string[] tuple = rawRoleTuple.Split(':');
			//
			//}

			foreach (RoleTuple roleTuple in Plugin.Instance.Config.GetRoleWlCache())
			{
				if (killerRole == roleTuple.KillerRole && victimRole == roleTuple.VictimRole)
				{
					if (death) Log.Info($"Killer role {killerRole} and victim role {victimRole} is whitelisted, not a teamkill.");
					return false;
				}
			}

			foreach (TeamTuple teamTuple in Plugin.Instance.Config.GetMatrixCache())
			{
				if (killerTeam == teamTuple.KillerTeam && victimTeam == teamTuple.VictimTeam)
				{
					if (death) Log.Info($"Team {killerTeam} killing {victimTeam} WAS detected as a teamkill.");
					return true;
				}
			}

			if (death) Log.Info($"Team {killerTeam} killing {victimTeam} was not detected as a teamkill.");
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId"></param>
		/// <returns>scaled ban amount in minutes</returns>
		internal int GetScaledBanAmount(string userId)
		{
			int banLength = 0;
			foreach (int banAmount in Plugin.Instance.Config.Scaled.Keys.OrderBy(k => k))
			{
				if (Plugin.Instance.Config.OutAll) Log.Info($"Ban length set to {banLength}. Checking ban amount for key {banAmount}.");
				// If ban kills is less than player's kills, set the banLength
				// This will ensure that players who teamkill more than the maximum
				// will still serve the maximum ban length
				if (banAmount < this.Teamkillers[userId].Teamkills.Count)
				{
					if (Plugin.Instance.Config.OutAll) Log.Info("Ban amount is less than player teamkills.");
					banLength = Plugin.Instance.Config.Scaled[banAmount];
				}
				// Exact ban amount match is found, set
				else if (banAmount == this.Teamkillers[userId].Teamkills.Count)
				{
					if (Plugin.Instance.Config.OutAll) Log.Info("Ban amount is equal to player teamkills.");
					banLength = Plugin.Instance.Config.Scaled[banAmount];
					break;
				}
				// If the smallest ban amount is larger than the player's bans,
				// then the player will not be banned.
				// If banAmount has not been found, it will still be set to 0
				else if (banAmount > this.Teamkillers[userId].Teamkills.Count)
				{
					if (Plugin.Instance.Config.OutAll) Log.Info("Ban amount is greater than player teamkills.");
					break;
				}
			}
			return banLength;
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnBan")]
		//[PipeMethod]
		internal bool OnBan(Teamkiller teamkiller, string killerNickname, int banLength)
		{
			if (teamkiller.Banned)
			{
				Log.Warning("[OnBan] Attempted to log repeat ban for " + teamkiller);
				return false;
			}

			// If two players with the same UserId are on the server, this will cause a problem
			Player player = Player.GetPlayers().Where(tk => tk.UserId == teamkiller.UserId).First();
			string killerUserId, killerIpAddress;
			bool immune;
			if (player != null)
			{
				killerUserId = player.UserId;
				killerIpAddress = player.IpAddress;
				immune = isImmune(player);
			}
			else
			{
				killerUserId = teamkiller.UserId;
				killerIpAddress = teamkiller.IPAddress;
				// TODO: Save UserGroup in Teamkiller so that the method can be used here?
				immune = false;
			}
			List<Teamkill> teamkills = teamkiller.Teamkills;

			if (immune)
			{
				Log.Info($"Admin/Moderator {killerNickname} has avoided a ban for {banLength} minutes after teamkilling {teamkills.Count} players during the round.");
				return false;
			}
			else if (Plugin.Instance.BanWhitelist.Contains(killerUserId))
			{
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} not being punished by FFA because the player is whitelisted.");
				return false;
			}
			else
			{
				string banReason;
				// If teamkills are more than 3, simply provide the count instead of listing each name off
				if (teamkills.Count > 3)
				{
					banReason = $"Banned {banLength} minutes for teamkilling {teamkills.Count} players";
				}
				else
				{
					banReason = $"Banned {banLength} minutes for teamkilling player(s) " + string.Join(", ", teamkills.Select(teamkill => teamkill.VictimName).ToArray());
				}

				if (player != null)
				{
					player.Ban(banReason, banLength); // "FriendlyFireAutoban"
				}
				else
				// if (teamkiller.Disconnected)
				// If the player cannot be found, then by defintion it is a disconnected user
				// Continue to track the boolean for garbage collection
				{
					long now = DateTime.Now.Ticks;

					BanDetails userBan = new BanDetails();
					userBan.OriginalName = teamkiller.Nickname;
					userBan.Id = teamkiller.UserId;
					// Calculate ticks
					userBan.Expires = now + (banLength * 60 * 10000000);
					userBan.Reason = string.Format(Plugin.Instance.GetTranslation("offline_ban"), banLength, teamkills);
					userBan.Issuer = "FriendlyFireAutoban";
					userBan.IssuanceTime = now;
					BanHandler.IssueBan(userBan, BanType.UserId);
					Log.Info($"{teamkiller.Nickname} / {teamkiller.UserId}: Banned {banLength} minutes for teamkilling {teamkills} players");

					BanDetails ipBan = new BanDetails();
					ipBan.OriginalName = teamkiller.Nickname;
					ipBan.Id = teamkiller.IPAddress;
					// Calculate ticks
					ipBan.Expires = now + (banLength * 60 * 10000000);
					ipBan.Reason = string.Format(Plugin.Instance.GetTranslation("offline_ban"), banLength, teamkills);
					ipBan.Issuer = "FriendlyFireAutoban";
					ipBan.IssuanceTime = now;
					BanHandler.IssueBan(ipBan, BanType.IP);
					Log.Info($"{teamkiller.Nickname} / {teamkiller.UserId}: Banned {banLength} minutes for teamkilling {teamkills} players");
				}

				teamkiller.Banned = true;
				Log.Info($"Player {killerNickname} has been banned for {banLength} minutes after teamkilling {teamkills} players during the round.");
				BroadcastUtil.MapBroadcast(string.Format(this.GetTranslation("banned_output"), killerNickname, teamkills.Count), 3);

				return true;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckRemoveGuns")]
		//[PipeMethod]
		internal bool OnCheckRemoveGuns(Player killer)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IpAddress;
			if (Plugin.Instance.Config.NoGuns > 0 && this.Teamkillers.ContainsKey(killerUserId) && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.NoGuns && !this.isImmune(killer))
			{
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} has had his/her guns removed for teamkilling.");

				List<ItemBase> itemsToRemove = new List<ItemBase>();
				foreach (ItemBase i in killer.Items)
				{
					switch (i.ItemTypeId)
					{
						case ItemType.GunAK:
						case ItemType.GunCOM15:
						case ItemType.GunCOM18:
						case ItemType.GunE11SR:
						case ItemType.GunLogicer:
						case ItemType.MicroHID:
						case ItemType.GunCrossvec:
						case ItemType.GunFSP9:
						case ItemType.GunRevolver:
						case ItemType.GunShotgun:
						case ItemType.GrenadeHE:
						case ItemType.GrenadeFlash:
							itemsToRemove.Add(i);
							break;
					}
				}
				foreach (ItemBase i in itemsToRemove)
				{
					killer.RemoveItem(i);
				}

				BroadcastUtil.PlayerBroadcast(killer, this.GetTranslation("noguns_output"), 2);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckToSpectator")]
		//[PipeMethod]
		internal bool OnCheckToSpectator(Player killer)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IpAddress;
			if (Plugin.Instance.Config.ToSpec > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.ToSpec && !this.isImmune(killer))
			{
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} has been moved to spectator for teamkilling {this.Teamkillers[killerUserId].Teamkills.Count} times.");
				BroadcastUtil.PlayerBroadcast(killer, this.GetTranslation("tospec_output"), 5);
				killer.SetRole(RoleTypeId.Spectator, RoleChangeReason.RemoteAdmin);
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeMethod]
		internal bool OnCheckUndead(Player killer, Player victim)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IpAddress;
			string victimUserId = victim.UserId;
			string victimNickname = victim.Nickname;
			string victimIpAddress = victim.IpAddress;
			RoleTypeId victimRole = victim.Role;
			if (Plugin.Instance.Config.Undead > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.Undead && !this.isImmune(killer))
			{
				RoleTypeId oldRole = victimRole;
				//Vector oldPosition = victim.GetPosition();
				Log.Info($"Player {victimNickname} {victimUserId} {victimIpAddress} has been respawned as {oldRole} after {killerNickname} {killerUserId} {killerIpAddress} teamkilled " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				BroadcastUtil.PlayerBroadcast(killer, string.Format(this.GetTranslation("undead_killer_output"), victimNickname), 5);
				BroadcastUtil.PlayerBroadcast(victim, string.Format(this.GetTranslation("undead_victim_output"), killerNickname), 5);
				Timer t = new Timer
				{
					Interval = 3000,
					Enabled = true
				};
				t.Elapsed += delegate
				{
					Log.Info($"Respawning victim {victimNickname} {victimUserId} {victimIpAddress} as {victimRole}...");
					killer.SetRole(oldRole, RoleChangeReason.RemoteAdmin);
					//victim.Teleport(oldPosition);
					t.Dispose();
				};
				return true;
			}
			else
			{
				return false;
			}
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckKick")]
		//[PipeMethod]
		internal bool OnCheckKick(Player killer)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IpAddress;
			if (Plugin.Instance.Config.Kicker > 0 && this.Teamkillers[killerUserId].Teamkills.Count == Plugin.Instance.Config.Kicker && !this.isImmune(killer))
			{
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} has been kicked for teamkilling " + this.Teamkillers[killerUserId].Teamkills.Count + " times.");
				BroadcastUtil.PlayerBroadcast(killer, this.GetTranslation("kicker_output"), 5);
				killer.Kick(this.GetTranslation("kicker_output")); // "FriendlyFireAutoban"
				return true;
			}
			else
			{
				return false;
			}
		}

		internal Teamkiller AddAndGetTeamkiller(Player player)
		{
			if (player == null)
			{
				Log.Warning("[AddAndGetTeamkiller] Null player passed in.");
				return null;
			}

			int playerId = player.PlayerId;
			string playerNickname = player.Nickname;
			string playerUserId = player.UserId;
			string playerIpAddress = player.IpAddress;
			if (playerId == 0 || String.IsNullOrEmpty(playerNickname) || String.IsNullOrEmpty(playerUserId) || String.IsNullOrEmpty(playerIpAddress))
			{
				Log.Warning($"Adding Teamkiller entry failed for player #{playerId} {playerNickname} [{playerUserId}] [{playerIpAddress}]");
				return null;
			}

			if (!Plugin.Instance.Teamkillers.ContainsKey(playerUserId))
			{
				Log.Info($"Adding Teamkiller entry for player #{playerId} {playerNickname} [{playerUserId}] [{playerIpAddress}]");
				Plugin.Instance.Teamkillers[playerUserId] = new Teamkiller(playerId, playerNickname, playerUserId, playerIpAddress);
			}
			//else
			//{
			//	if (Plugin.Config.OutAll)
			//	{
			//		Log.Info("Fetching Teamkiller entry for player #" + playerId + " " + playerNickname + " [" + playerUserId + "] [" + playerIpAddress + "]");
			//	}
			//}
			return Plugin.Instance.Teamkillers[playerUserId];
		}

		//[PipeEvent("patpeter.friendly.fire.autoban.OnCheckVote")]
		//[PipeMethod]
		internal bool OnVoteTeamkill(Player killer)
		{
			string killerUserId = killer.UserId;
			string killerNickname = killer.Nickname;
			string killerIpAddress = killer.IpAddress;

			if (Plugin.Instance.Config.OutAll)
			{
				Log.Info("votetk > 0: " + Plugin.Instance.Config.VoteTk);
				Log.Info("Teamkiller count is greater than votetk? " + this.Teamkillers[killerUserId].Teamkills.Count);
				Log.Info("Teamkiller is immune? " + this.isImmune(killer));
			}
			if (Plugin.Instance.Config.VoteTk > 0 && this.Teamkillers[killerUserId].Teamkills.Count >= Plugin.Instance.Config.VoteTk && !this.isImmune(killer))
			{
				Log.Info($"Player {killerNickname} {killerUserId} {killerIpAddress} is being voted on a ban for teamkilling {this.Teamkillers[killerUserId].Teamkills.Count} times.");
				IDictionary<int, string> options = new Dictionary<int, string>();
				options[1] = "Yes";
				options[2] = "No";
				ISet<string> votes = new HashSet<string>();
				IDictionary<int, int> counter = new Dictionary<int, int>();

				/*if (Voting != null && StartVote != null && !Voting.Invoke())
				{
					//Plugin.Instance.InvokeEvent("OnStartVote", $"Ban {killerNickname}?", options, votes, counter);
					Log.Info($"Running vote:  Ban {killerNickname}?");
					this.StartVote.Invoke($"Ban {killerNickname}?", options, votes, counter);
					return true;
				}
				else
				{
					Log.Warn("patpeter.callvote Voting PipeLink is broken. Cannot start vote.");
					return false;
				}*/
				Log.Warning("patpeter.callvote Voting PipeLink is broken. Cannot start vote.");
				return false;
			}
			else
			{
				return false;
			}
		}
	}
}
