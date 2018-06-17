# Friendly Fire Autoban
This is a plugin for [Smod2](https://github.com/Grover-c13/Smod2) that automatically bans players after a specified number of teamkills, for a certain amount of time.

## Config Settings
These are the configuration settintgs for FFA:

Config Setting | Value Type | Default Value | Description
--- | :---: | :---: | ---
friendly_fire_autoban_enable: | boolean | true | enable or disable the plugin on each server
friendly_fire_autoban_amount: | integer | 5 | amount of teamkills that trigger a ban
friendly_fire_autoban_length: | integer | 3600 | time to ban the teamkiller for (in minutes)
friendly_fire_autoban_noguns: | integer | 0 | number of teamkills at which to remove a player's guns
friendly_fire_autoban_matrix: | string | (1:1,2:2,3:3,4:4,1:3,2:4,3:1,4:2) | Matrix of killer:victim tuples that the plugins considers teamkills
