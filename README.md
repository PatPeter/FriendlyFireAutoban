# Friendly Fire Autoban
This is a plugin for [Smod2](https://github.com/Grover-c13/Smod2) that automatically bans players after a specified number of teamkills, for a certain amount of time.

## FFA Config Settings
Config Setting | Value Type | Default Value | Description
--- | --- | --- | ---
friendly_fire_autoban_enable | boolean | true | `Enable` or `Disable` the plugin on each server
friendly_fire_autoban_outall | boolean | false | Print debugging statements to see if your configuration is working correctly
friendly_fire_autoban_system | integer | 1 | Change system for processing teamkills:
(1) basic counter that will ban the player instantly upon reaching a threshold,
(2) timer-based counter that will ban a player after reaching the threshold but will forgive 1 teamkill every `friendly_fire_autoban_expire` seconds, or
(3) allow users to teamkill as much as possible and ban them after they have gone `friendly_fire_autoban_expire` seconds without teamkilling (will ban on round end and player disconnect).
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
friendly_fire_autoban_rolewl | integer | <empty> | Matrix of `killer:victim` role tuples that the plugin will NOT consider teamkills.
friendly_fire_autoban_mirror | integer | 0 | Whether damage should be mirrored back to a teamkiller, with values greater than (1) being considered a multiplier.
friendly_fire_autoban_warntk | integer | -1 | How many teamkills before a ban should a teamkiller be warned (>=1), give a generic warning (0), or give no warning (-1).
friendly_fire_autoban_votetk | integer | 0 | [not implemented yet] The number of teamkills at which to call a vote via the callvote plugin to ban a user by the ban amount.
friendly_fire_autoban_immune | string | owner,admin,moderator | Groups that are immune to being autobanned.
