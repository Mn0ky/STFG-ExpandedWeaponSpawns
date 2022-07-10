# ExpandedWeaponSpawns

<p align="center">
  <a href="https://forthebadge.com">
    <img src="https://forthebadge.com/images/badges/made-with-c-sharp.svg">
  </a>
</p>
<p align="center">
  <a href="https://github.com/Mn0ky/STFG-ExpandedWeaponSpawns/releases/latest">
    <img src="https://img.shields.io/github/downloads/Mn0ky/STFG-ExpandedWeaponSpawns/total?label=Github%20downloads&logo=github">
  </a>
  <a href="https://opensource.org/licenses/MIT">
    <img src="https://img.shields.io/badge/MIT-blue.svg">
  </a>
</p>

<p align="center">
  <a href="">
    <img src="https://user-images.githubusercontent.com/67206766/172464881-d1125d2a-1698-4b67-84c2-bd0992eb92e5.gif">
  </a>
</p>


## Description

This mod gives the player the ability to have the special/unused/dev weapons (such as the holy sword or mini minigun) to naturally spawn in along with the normal weapons. One can edit the rarities (spawn chances) of said weapons to decrease/increase the chance that they'll spawn. All changes to a weapon's active state and rarity are automatically saved to disk, which means changes are saved and loaded even after closing then reopening the game. 

This mod originally evolved from an experiment which fixed the bow, said fixes now allow the bow to be properly drawn and fired— with the networking to go with it. By default, the bow is able to be drawn back and charged with <kbd>LeftShift</kbd> but this is customizable in the config which can be found at:<br/> ``BepInEx\config\monky.plugins.ExpandedWeaponSpawns.cfg``. The arrows have had collision enabled allowing for strategic makeshift platforms to be created and built upon. 

Some weapons (the lava whip and holy minigun in particular) do not have a weapon drop, this is circumvented in the mod by having them come down as a sword through a present. Once picked up, they'll give their respective weapons. If thrown, they'll come out as a normal minigun (for the holy mini gun) or a normal sword (for the lava whip). 

While some of the special weapons are still pretty nonfunctional, the plan is to ultimately rework them through successive updates. 
<ins>**The mod is unable to be used in public lobbies; one must directly join another mod user**</ins>.

## Installation

To install the mod, one can follow the [same instructions](https://github.com/Mn0ky/QOL-Mod/#installation) as found on the QOL-Mod page and apply them here. 
**In addition to putting the ``ExpandedWeaponSpawns.dll`` into the ``BepInEx\Plugins`` folder, you must [download](https://github.com/Mn0ky/SFTG-SimpleAntiCheat/releases/latest/download/SimpleAntiCheat.dll) and put the ``SimpleAntiCheat.dll`` into the folder as well, 
<ins>or the mod will not run</ins>**.
## Using the Menus

To access the weapon selector menu, which one can use to disable or enable weapons for spawning (like in the normal ingame options), simply use the default keybind of: <kbd>LeftShift</kbd> + <kbd>F3</kbd> (changeable in config).
![weapon_selector](https://user-images.githubusercontent.com/67206766/172457637-4535192d-c545-45ab-b213-8e97f9d89106.png)<br/>
Located at the top of the menu, the ``Edit Rarities`` button opens the rarity editor menu, which, as stated above, allows for decreasing/increasing the spawn chances of special weapons.
![rarity_editor](https://user-images.githubusercontent.com/67206766/172458665-ddc7d95a-de7e-439b-ba90-261c8a1ca638.png)
