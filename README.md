# Friendly Fire Autoban
This is a plugin for [Smod2](https://github.com/Grover-c13/Smod2) that automatically bans players after a specified number of teamkills, for a certain amount of time.

## FFA Config Settings
Config Setting | Value Type | Default Value | Description
--- | :---: | :---: | ---
friendly_fire_autoban_enable | boolean | true | `Enable` or `Disable` the plugin on each server
friendly_fire_autoban_system | integer | 1 | Change system for processing teamkills: basic counter (1), timer-based counter (2), or end-of-round counter (3).
friendly_fire_autoban_matrix | dictionary | 1:1,2:2,3:3,4:4,1:3,2:4,3:1,4:2 | Matrix of `killer:victim` tuples that the plugins considers teamkills
friendly_fire_autoban_amount | integer | 5 | Amount of teamkills before a ban will be issued.
friendly_fire_autoban_length | integer | 3600 | Length of ban in minutes.
friendly_fire_autoban_expire | integer | 60 | For ban system #2, Time it takes in seconds for teamkill to degrade and not count towards ban.
friendly_fire_autoban_scaled | dictionary | 1:0,2:1,3:5,4:15,5:30,6:60,7:180,8:300,9:480,10:720,11:1440,12:4320,13:10080,14:20160,15:43200,16:43200,17:14400,18:525600,19:2628000,20:26280000 | For ban system #3, dictionary of amount of teamkills:length of ban that will be processed at the end of the round. The default list is an *example* with every ban quantity in the original release of SCP:SL. **USE YOUR OWN VALUES**
friendly_fire_autoban_noguns | integer | 0 | Number of kills to remove the player's guns as a warning for teamkilling.
friendly_fire_autoban_tospec | integer | 0 | Number of kills at which to put a player into spectator as a warning for teamkilling.
