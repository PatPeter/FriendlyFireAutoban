# Friendly Fire Autoban
This is a plugin for [Smod2](https://github.com/Grover-c13/Smod2) and [EXILED](https://github.com/Exiled-Team/EXILED) that automatically bans players after a specified number of teamkills, for a certain amount of time.

## Configuration Settings
Key | Value Type | Default Value | Description
--- | --- | --- | ---
is_enabled |    boolean |                            true | Enable or disable the plugin. Defaults to true.
out_all    |    boolean |                           false | Print debugging statements to see if your configuration is working correctly
system     |    integer |                               1 | Change system for processing teamkills:<br>(1) basic counter that will ban the player instantly upon reaching a threshold,<br>(2) timer-based counter that will ban a player after reaching the threshold but will forgive 1 teamkill every `friendly_fire_autoban_expire` seconds, or<br>(3) allow users to teamkill as much as possible and ban them after they have gone `friendly_fire_autoban_expire` seconds without teamkilling (will ban on round end and player disconnect).
matrix     |       list | 1:1,2:2,3:3,4:4,1:3,2:4,3:1,4:2 | Matrix of `killer:victim` team tuples that the plugin considers teamkills
amount     |    integer |                               5 | Amount of teamkills before a ban will be issued.
length     |    integer |                            3600 | Length of ban in minutes.
expire     |    integer |                              60 | For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.
scaled     | dictionary |                                 | For ban system #3, dictionary of amount of teamkills:length of ban that will be processed at the end of the round.
no_guns    |    integer |                               0 | Number of kills to remove the player's guns as a warning for teamkilling, and will remove guns every time the player picks them up or spawns with them. In ban system #1, this will remove the player's guns for the rest of the round.
to_spec    |    integer |                               0 | Number of kills at which to put a player into spectator as a warning for teamkilling.
kicker     |    integer |                               0 | Number of kills at which to kick the player as a warning for teamkilling.
immune     |     string |           owner,admin,moderator | Groups that are immune to being autobanned.
bomber     |    integer |                               0 | Whether to delay grenade damage of thrower by one second [experimental] (2), make player immune to grenade damage (1), or keep disabled (0).
disarm     |    boolean |                           false | Whether disarmed players should be considered members of the opposite team and role.
rolewl     |       list |                                 | Matrix of `killer:victim` role tuples that the plugin will NOT consider teamkills.<br><br>If you want NTF to be able to teamkill based on the chain of command, use this value (on one line): <br>12:11,12:4,12:13,12:15,<br>4:11,4:13,4:15,<br>11:13,11:15,13:15
invert     |    integer |                               0 | Reverse Friendly Fire. If greater than 0, value of mirror will only apply after this many teamkills.
mirror     |      float |                               0 | Whether damage should be mirrored back to a teamkiller, with values greater than (1) being considered a multiplier.
undead     |    integer |                               0 | Respawns teamkilled players after this many teamkills.
warn_tk    |    integer |                              -1 | How many teamkills before a ban should a teamkiller be warned (>=1), give a generic warning (0), or give no warning (-1).
vote_tk    |    integer |                               0 | [not implemented yet] The number of teamkills at which to call a vote via the callvote plugin to ban a user by the ban amount.
kd_safe    |    integer |                               0 | The K/D ratio at which players will be immune from pre-ban and ban punishments. Takes effect when kills are greater than kdsafe, i.e. set to 2 requires a minimum of 4:2 (not 2:1), set to 3 requires a minimum of 6:2 (not 3:1), etc.

## Example Configuration
Here is the default configuration for Friendly Fire Autoban that you can copy directly into your config_gameplay.txt:

~~~~
friendly_fire_autoban:
  is_enabled: true
  out_all: false
  system: 3
  matrix: 1:1,2:2,3:3,4:4,1:3,2:4,3:1,4:2
  amount: 4
  length: 1440
  expire: 60
  scaled: 4:1440,5:4320,6:4320,7:10080,8:10080,9:43800,10:43800,11:129600,12:129600,13:525600
  noguns: 0
  tospec: 0
  kicker: 0
  bomber: 0
  disarm: false
  rolewl: 12:11,12:4,12:13,12:15,4:11,4:13,4:15,11:13,11:15,13:15
  mirror: 0
  warntk: 0
  votetk: 0
  immune: owner,admin,moderator
~~~~

## Commands
Key | Aliases | Parameters | Description
--- | --- | --- | ---
ffa_toggle | ffa_toggle |  | Toggles friendly fire autoban on or off.
ffa_whitelist | ffa_whitelist |  | Toggles a player by name or Steam ID as whitelisted from all FFA punishments.
