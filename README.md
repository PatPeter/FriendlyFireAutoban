# Friendly Fire Autoban
This is a plugin for [Smod2](https://github.com/Grover-c13/Smod2) that automatically bans players after a specified number of teamkills, for a certain amount of time.

## Configuration Settings
Key | Value Type | Default Value | Description
--- | --- | --- | ---
friendly_fire_autoban_enable | boolean | true | `Enable` or `Disable` the plugin on each server
friendly_fire_autoban_outall | boolean | false | Print debugging statements to see if your configuration is working correctly
friendly_fire_autoban_system | integer | 1 | Change system for processing teamkills:<br>(1) basic counter that will ban the player instantly upon reaching a threshold,<br>(2) timer-based counter that will ban a player after reaching the threshold but will forgive 1 teamkill every `friendly_fire_autoban_expire` seconds, or<br>(3) allow users to teamkill as much as possible and ban them after they have gone `friendly_fire_autoban_expire` seconds without teamkilling (will ban on round end and player disconnect).
friendly_fire_autoban_matrix | list | 1:1,2:2,3:3,4:4,1:3,2:4,3:1,4:2 | Matrix of `killer:victim` team tuples that the plugins considers teamkills
friendly_fire_autoban_amount | integer | 5 | Amount of teamkills before a ban will be issued.
friendly_fire_autoban_length | integer | 3600 | Length of ban in minutes.
friendly_fire_autoban_expire | integer | 60 | For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.
friendly_fire_autoban_scaled | dictionary |  | For ban system #3, dictionary of amount of teamkills:length of ban that will be processed at the end of the round.
friendly_fire_autoban_noguns | integer | 0 | Number of kills to remove the player's guns as a warning for teamkilling, and will remove guns every time the player picks them up or spawns with them. In ban system #1, this will remove the player's guns for the rest of the round.
friendly_fire_autoban_tospec | integer | 0 | Number of kills at which to put a player into spectator as a warning for teamkilling.
friendly_fire_autoban_kicker | integer | 0 | Number of kills at which to kick the player as a warning for teamkilling.
friendly_fire_autoban_bomber | integer | 0 | Whether to delay grenade damage of thrower by one second [experimental] (2), make player immune to grenade damage (1), or keep disabled (0).
friendly_fire_autoban_disarm | boolean | false | Whether disarmed players should be considered members of the opposite team and role.
friendly_fire_autoban_rolewl | integer |  | Matrix of `killer:victim` role tuples that the plugin will NOT consider teamkills.<br><br>If you want NTF to be able to teamkill based on the chain of command, use this value (on one line): <br>12:11,12:4,12:13,12:15,<br>4:11,4:13,4:15,<br>11:13,11:15,13:15
friendly_fire_autoban_mirror | integer | 0 | Whether damage should be mirrored back to a teamkiller, with values greater than (1) being considered a multiplier.
friendly_fire_autoban_warntk | integer | -1 | How many teamkills before a ban should a teamkiller be warned (>=1), give a generic warning (0), or give no warning (-1).
friendly_fire_autoban_votetk | integer | 0 | [not implemented yet] The number of teamkills at which to call a vote via the callvote plugin to ban a user by the ban amount.
friendly_fire_autoban_immune | string | owner,admin,moderator | Groups that are immune to being autobanned.

## Example Configuration
Here is the default configuration for Friendly Fire Autoban that you can copy directly into your config_gameplay.txt:

~~~~
friendly_fire_autoban_enable: true
friendly_fire_autoban_outall: false
friendly_fire_autoban_system: 3
friendly_fire_autoban_matrix: 1:1,2:2,3:3,4:4,1:3,2:4,3:1,4:2
friendly_fire_autoban_amount: 4
friendly_fire_autoban_length: 1440
friendly_fire_autoban_expire: 60
friendly_fire_autoban_scaled: 4:1440,5:4320,6:4320,7:10080,8:10080,9:43800,10:43800,11:129600,12:129600,13:525600
friendly_fire_autoban_noguns: 0
friendly_fire_autoban_tospec: 0
friendly_fire_autoban_kicker: 0
friendly_fire_autoban_bomber: 0
friendly_fire_autoban_disarm: false
friendly_fire_autoban_rolewl: 12:11,12:4,12:13,12:15,4:11,4:13,4:15,11:13,11:15,13:15
friendly_fire_autoban_mirror: 0
friendly_fire_autoban_warntk: 0
friendly_fire_autoban_votetk: 0
friendly_fire_autoban_immune: owner,admin,moderator
~~~~

## Commands
Key | Aliases | Parameters | Description
--- | --- | --- | ---
friendly_fire_autoban_toggle | ffa_toggle |  | Toggles friendly fire autoban on or off.
