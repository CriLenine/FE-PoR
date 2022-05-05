# FE-PoR
Little solo demo project highly based on Fire Emblem : Path of Radiance

I worked alone on this project for 2 weeks (aside my usual schedule) during november, it was the first "long-term" Unity project i had to make this year, so it is only a very basic prototype.

## Features

The game starts with a simple menu (a button to play and a button to quit).  
When the level starts, some characters are spawned (2 allies and an enemy), they each have specific data (move range, damage, weapons, etc.).  
The player first plays the allied characters, then the enemy, and so on.  
Each character can move around itself within its move range, and an A* pathfinding algorithm is used to avoid eventual obstacles (here there are trees).
Once it has moved the player can, if the character's weapons ranges allow it, attack an enemy.

#### Fight between two characters

Each attack has a chance to miss based on the dodge chance of the enemy. If the enemy survives to the first potential hit, and if the enemy's equipped weapon can be used at the appropriate range, he will hit back the attacker.

The game ends when all the characters of a team are killed.
