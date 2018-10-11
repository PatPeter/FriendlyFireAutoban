using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Timers;

namespace FriendlyFireAutoban.EventHandlers
{
    class RoundStartHandler : IEventHandlerRoundStart
    {
        private FriendlyFireAutobanPlugin plugin;

        public RoundStartHandler(Plugin plugin)
        {
            this.plugin = (FriendlyFireAutobanPlugin)plugin;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            this.plugin.Debug("friendly_fire_autoban_enable value: " + this.plugin.GetConfigBool("friendly_fire_autoban_enable"));
            this.plugin.Debug("friendly_fire_autoban_amount value: " + this.plugin.GetConfigInt("friendly_fire_autoban_amount"));
            this.plugin.Debug("friendly_fire_autoban_length value: " + this.plugin.GetConfigInt("friendly_fire_autoban_length"));
            this.plugin.Debug("friendly_fire_autoban_noguns value: " + this.plugin.GetConfigInt("friendly_fire_autoban_noguns"));
            this.plugin.Debug("friendly_fire_autoban_tospec value: " + this.plugin.GetConfigInt("friendly_fire_autoban_tospec"));
            this.plugin.duringRound = true;
            this.plugin.teamkillCounter = new Dictionary<string, int>();
            this.plugin.teamkillMatrix = new List<TeamkillTuple>();
            string[] teamkillMatrix = this.plugin.GetConfigList("friendly_fire_autoban_matrix");
            foreach (string pair in teamkillMatrix)
            {
                string[] tuple = pair.Split(':');
                if (tuple.Length != 2)
                {
                    plugin.Debug("Tuple " + pair + " does not have a single : in it.");
                    continue;
                }
                int tuple0 = -1, tuple1 = -1;
                if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
                {
                    plugin.Debug("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
                    continue;
                }

                this.plugin.teamkillMatrix.Add(new TeamkillTuple(tuple0, tuple1));
            }

            this.plugin.teamkillScaled = new Dictionary<int, int>();
            string[] teamkillScaled = this.plugin.GetConfigList("friendly_fire_autoban_scaled");
            foreach (string pair in teamkillScaled)
            {
                string[] tuple = pair.Split(':');
                if (tuple.Length != 2)
                {
                    plugin.Debug("Tuple " + pair + " does not have a single : in it.");
                    continue;
                }
                int tuple0 = -1, tuple1 = -1;
                if (!int.TryParse(tuple[0], out tuple0) || !int.TryParse(tuple[1], out tuple1))
                {
                    plugin.Debug("Either " + tuple[0] + " or " + tuple[1] + " could not be parsed as an int.");
                    continue;
                }

                if (!this.plugin.teamkillScaled.ContainsKey(tuple0))
                {
                    this.plugin.teamkillScaled[tuple0] = tuple1;
                }
            }

            //for friendly_fire__<class name>__<damage type>
            this.plugin.weaponKillCounter = new Dictionary<string, Dictionary<string, int>>();
        }
    }

    class RoundEndHandler : IEventHandlerRoundEnd
    {
        private FriendlyFireAutobanPlugin plugin;

        public RoundEndHandler(Plugin plugin)
        {
            this.plugin = (FriendlyFireAutobanPlugin)plugin;
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            if (ev.Round.Duration >= 3)
            {
                this.plugin.duringRound = false;
            }

            if (this.plugin.GetConfigInt("friendly_fire_autoban_system") == 2)
            {
                foreach (Timer timer in this.plugin.teamkillTimers.Values)
                {
                    timer.Enabled = false;
                }
            }
            else if (this.plugin.GetConfigInt("friendly_fire_autoban_system") == 3)
            {
                foreach (Player player in ev.Server.GetPlayers())
                {
                    if (this.plugin.teamkillCounter.ContainsKey(player.SteamId))
                    {
                        int teamkills = this.plugin.teamkillCounter[player.SteamId];
                        this.plugin.Debug("Player " + player.ToString() + " has committed " + teamkills + " teamkills.");
                        if (this.plugin.teamkillScaled.ContainsKey(teamkills))
                        {
                            int banLength = this.plugin.teamkillScaled[teamkills];
                            this.plugin.Ban(player, player.Name, banLength, teamkills);
                        }
                        else
                        {
                            this.plugin.Debug(teamkills + " teamkills is not bannable.");
                        }
                    }
                    else
                    {
                        this.plugin.Debug("Player " + player.ToString() + " has committed no teamkills.");
                    }
                }
            }

            this.plugin.teamkillCounter = new Dictionary<string, int>();
            this.plugin.weaponKillCounter = new Dictionary<string, Dictionary<string, int>>();
        }
    }

    class PlayerDieHandler : IEventHandlerPlayerDie
    {
        private FriendlyFireAutobanPlugin plugin;

        public PlayerDieHandler(Plugin plugin)
        {
            this.plugin = (FriendlyFireAutobanPlugin)plugin;
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            if (!this.plugin.duringRound)
            {
                this.plugin.Debug("Skipping OnPlayerDie for being outside of a round.");
                return;
            }
            string[] killerNameParts = Regex.Split(ev.Killer.ToString(), @"::");
            if (killerNameParts.Length >= 4)
            {
                killerNameParts = new string[] { killerNameParts[0], "::" + killerNameParts[2], killerNameParts[3] };
            }
            string[] victimNameParts = Regex.Split(ev.Player.ToString(), @"::");
            if (victimNameParts.Length >= 4)
            {
                victimNameParts = new string[] { victimNameParts[0], "::" + victimNameParts[2], victimNameParts[3] };
            }

            if (isTeamkill(ev.Killer, ev.Player))
            {
                if (this.plugin.GetConfigBool("friendly_fire_autoban_enable"))
                {
                    string[] TeamWeapon = new string[] { TeamResolver(ev.Killer.TeamRole.Team), ev.DamageTypeVar.ToString().ToLower() };
                    string confVar = "friendly_fire__" + String.Join("__", TeamWeapon);
                    if (this.plugin.teamkillCounter.ContainsKey(ev.Killer.SteamId))
                    {
                        if (this.plugin.GetConfigInt(confVar) > 0 && TeamWeapon[0] != "none")
                        {
                            if (this.plugin.weaponKillCounter.ContainsKey(ev.Killer.SteamId))
                            {
                                if (this.plugin.weaponKillCounter[ev.Killer.SteamId].ContainsKey(TeamWeapon[1]))
                                {
                                    this.plugin.weaponKillCounter[ev.Killer.SteamId][TeamWeapon[1]]++;
                                }
                                else
                                {
                                    this.plugin.weaponKillCounter[ev.Killer.SteamId][TeamWeapon[1]] = 1;
                                }
                            }
                            else
                            {
                                this.plugin.weaponKillCounter[ev.Killer.SteamId] = new Dictionary<string, int>();
                                this.plugin.weaponKillCounter[ev.Killer.SteamId][TeamWeapon[1]] = 1;
                            }

                        }
                        else
                        {
                            this.plugin.teamkillCounter[ev.Killer.SteamId]++;
                        }
                        plugin.Info("Player " + String.Join(" ", killerNameParts) + " " + ev.Killer.TeamRole.Team.ToString() + " teamkilled " +
                            String.Join(" ", victimNameParts) + " " + ev.Player.TeamRole.Team.ToString() + ", for a total of " + this.plugin.teamkillCounter[ev.Killer.SteamId] + " teamkills." +
                            "DMGTYPE: " + ev.DamageTypeVar.ToString().ToLower());
                    }
                    else
                    {
                        this.plugin.teamkillCounter[ev.Killer.SteamId] = 0;
                        if (this.plugin.GetConfigInt(confVar) > 0 && TeamWeapon[0] != "none")
                        {
                            this.plugin.weaponKillCounter[ev.Killer.SteamId] = new Dictionary<string, int>();
                            plugin.Info("WEAPONKILLCNT: " + this.plugin.weaponKillCounter[ev.Killer.SteamId]);
                            this.plugin.weaponKillCounter[ev.Killer.SteamId][TeamWeapon[1]] = 1;
                        }
                        else
                        {
                            this.plugin.teamkillCounter[ev.Killer.SteamId]++;
                        }
                        plugin.Info("Player " + String.Join(" ", killerNameParts) + " " + ev.Killer.TeamRole.Team.ToString() + " teamkilled " +
                            String.Join(" ", victimNameParts) + " " + ev.Player.TeamRole.Team.ToString() + ", for a total of 1 teamkill. " +
                            "DMGTYPE: " + ev.DamageTypeVar.ToString().ToLower());
                    }

                    if (this.plugin.GetConfigInt("friendly_fire_autoban_noguns") > 0 && this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_noguns"))
                    {
                        this.plugin.Info("Player " + String.Join(" ", killerNameParts) + " has had his/her guns removed for teamkilling.");
                        List<Item> inv = ev.Killer.GetInventory();
                        for (int i = 0; i < inv.Count; i++)
                        {
                            switch (inv[i].ItemType)
                            {
                                case ItemType.COM15:
                                case ItemType.E11_STANDARD_RIFLE:
                                case ItemType.LOGICER:
                                case ItemType.MICROHID:
                                case ItemType.MP4:
                                case ItemType.P90:
                                case ItemType.FRAG_GRENADE:
                                case ItemType.FLASHBANG:
                                    inv[i].Remove();
                                    break;
                            }
                        }
                    }

                    if (this.plugin.GetConfigInt("friendly_fire_autoban_tospec") > 0 && this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_tospec"))
                    {
                        this.plugin.Info("Player " + String.Join(" ", killerNameParts) + " has been moved to spectator for teamkilling " + this.plugin.teamkillCounter[ev.Killer.SteamId] + " times.");
                        ev.Killer.ChangeRole(Role.SPECTATOR);
                    }

                    if (this.plugin.GetConfigInt("friendly_fire_autoban_kicker") > 0 && this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_kicker"))
                    {
                        this.plugin.Info("Player " + String.Join(" ", killerNameParts) + " has been kicked for teamkilling " + this.plugin.teamkillCounter[ev.Killer.SteamId] + " times.");
                        ev.Killer.Ban(0);
                    }

                    if (this.plugin.GetConfigInt("friendly_fire_autoban_system") == 1)
                    {
                        if (this.plugin.GetConfigInt(confVar) > 0 && TeamWeapon[0] != "none" && this.plugin.weaponKillCounter[ev.Killer.SteamId].ContainsKey(TeamWeapon[1]) &&
                            this.plugin.weaponKillCounter[ev.Killer.SteamId][TeamWeapon[1]] >= this.plugin.GetConfigInt(confVar))
                        {
                            this.plugin.Ban(ev.Killer, String.Join(" ", killerNameParts), this.plugin.GetConfigInt("friendly_fire_autoban_length"), this.plugin.teamkillCounter[ev.Killer.SteamId]);
                        }
                        else if (this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_amount"))
                        {
                            this.plugin.Ban(ev.Killer, String.Join(" ", killerNameParts), this.plugin.GetConfigInt("friendly_fire_autoban_length"), this.plugin.teamkillCounter[ev.Killer.SteamId]);
                        }
                    }

                    if (this.plugin.GetConfigInt("friendly_fire_autoban_system") == 2)
                    {
                        Timer t;
                        if (this.plugin.teamkillTimers.ContainsKey(ev.Killer.SteamId))
                        {
                            t = this.plugin.teamkillTimers[ev.Killer.SteamId];
                            t.Stop();
                            t.Interval = this.plugin.GetConfigInt("friendly_fire_autoban_expire") * 1000;
                            t.Start();
                        }
                        else
                        {
                            t = new Timer
                            {
                                Interval = this.plugin.GetConfigInt("friendly_fire_autoban_expire") * 1000,
                                AutoReset = true,
                                Enabled = true
                            };
                            t.Elapsed += delegate
                            {
                                if (this.plugin.teamkillCounter[ev.Killer.SteamId] > 0)
                                {
                                    this.plugin.teamkillCounter[ev.Killer.SteamId]--;
                                    this.plugin.Info("Player " + String.Join(" ", killerNameParts) + " " + ev.Killer.TeamRole.Team.ToString() + " teamkill expired, counter now at " + this.plugin.teamkillCounter[ev.Killer.SteamId] + ".");
                                }
                                else
                                {
                                    t.Enabled = false;
                                }
                            };
                            this.plugin.teamkillTimers[ev.Killer.SteamId] = t;
                        }

                        if (this.plugin.teamkillCounter[ev.Killer.SteamId] >= this.plugin.GetConfigInt("friendly_fire_autoban_amount"))
                        {
                            t.Stop();
                            this.plugin.Ban(ev.Killer, String.Join(" ", killerNameParts), this.plugin.GetConfigInt("friendly_fire_autoban_length"), this.plugin.teamkillCounter[ev.Killer.SteamId]);
                        }
                    }
                }
                else
                {
                    plugin.Info("Player " + String.Join(" ", killerNameParts) + " " + ev.Killer.TeamRole.Team.ToString() + " teamkilled " +
                        String.Join(" ", victimNameParts) + " " + ev.Player.TeamRole.Team.ToString() + ".");
                }
            }
            else
            {
                this.plugin.Debug("Player " + String.Join(" ", killerNameParts) + " " + ev.Killer.TeamRole.Team.ToString() + " killed " +
                    String.Join(" ", victimNameParts) + " " + ev.Player.TeamRole.Team.ToString() + " and it was not detected as a teamkill.");
            }
        }

        public bool isTeamkill(Player killer, Player victim)
        {
            int killerTeam = (int)killer.TeamRole.Team;
            int victimTeam = (int)victim.TeamRole.Team;

            if (killer.SteamId == victim.SteamId)
            {
                return false;
            }

            bool isTeamkill = false;
            foreach (TeamkillTuple teamkill in this.plugin.teamkillMatrix)
            {
                if (killerTeam == teamkill.killerRole && victimTeam == teamkill.victimRole)
                {
                    isTeamkill = true;
                }
            }

            return isTeamkill;
        }
        //for friendly_fire__<class name>__<damage type>
        public static string TeamResolver(Team team)
        {
            Dictionary<Team, string> teams = new Dictionary<Team, string>();
            teams[Team.CHAOS_INSURGENCY] = "ci";
            teams[Team.CLASSD] = "classd";
            teams[Team.NINETAILFOX] = "mtf";
            teams[Team.SCIENTISTS] = "sci";
            if (teams.ContainsKey(team))
            {
                return teams[team];
            }
            else
            {
                return "none";
            }
        }
    }
}
