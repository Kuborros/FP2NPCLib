# FP2NPCLib

# DEPRECATED in favor of [FP2Lib](https://github.com/Kuborros/FP2Lib)

A library for easy creation of NPC mods. It handles hooking into Savefile data, as well as instancing your NPC in desired level.

## Usage:

An NPC can be registered by calling:
```c#
registerNPC(string uID, string Name, string Scene, GameObject Prefab, int Species, int Home, int DialogueTopics)
```
or:
```c#
registerNPC(string uID, string Name, string Scene, GameObject Prefab, FPCharacterSpecies Species, FPCharacterHome Home, int DialogueTopics)
```

- uID - Your unique identifier for character. It's used internally to properly match character data between restarts. Set it to something unique.
- Name - Character name as seen in-game.
- Scene - Name of the map your character should be loaded on. To load them in multiple areas, register them multiple times with different uIDs.
- Prefab - GameObject containing the NPC itself, can be loaded from AssetBundle, or constructed within your mod itself, as long as it is valid.
- (Optional) Species - Species ID of your character.
- (Optional) Home - ID of location listed as 'Home' of your character.
- (Very, very recommended) DialogueTopics - Amount of dialogue topics your character has. Needs to be equal or higher to amount set in the Prefab. Fallback default will set a single dialogue if not defined, as it is required for minimal function.

To ensure your mod doesnt load without the lib you can add a bepinex dependency:
```c#
    [BepInDependency("000.kuborro.plugins.fp2.npclib")]
``` 

## Internal ID's

For both ID's convenient enums (``FPCharacterSpecies`` and ``FPCharacterHome``) are provided. You can use numeric values if you prefer so, listed below:

Species ID's are as follows:
```
0 (default/fallback): "Unknown"
1: "Cat"
2: "Bird"
3: "Bear"
4: "Panda"
5: "Red Panda"
6: "Monkey"
7: "Dog"
8: "Deer"
9: "Fox"
10: "Rabbit"
11: "Dragon"
12: "Bat"
13: "Boar"
14: "Otter"
15: "Raccoon";
16: "Goat"
17: "Hedgehog"
18: "Reptilian"
20: "Mouse"
21: "Ferret"
22: "Plant"
23: "Robot"
24: "Pangolin"
```
Number 19 is missing within the game's code, and is therefore an invalid ID.

Home ID's are:
```
0 (default/fallback): "Unknown"
1: "Shang Tu"
2: "Shang Mu"
3: "Shuigang"
4: "Parusa"
```

## Setting up required prefab

While creation of an assetbundle and npc prefab is outside the scope of this Readme, below are some very simplified steps:

- If not yet done, decompile the game with [AssetRipper](https://github.com/AssetRipper/AssetRipper) - If you want to run the game in the editor, decompile versions older than 1.0.4. Otherwise set AssetRipper to only generate dummy classes as versions with Rewired do not decompile properly.
- Create new working directory within Assets folder in the editor
- Copy an existing NPC or create new object with FPHubNPC script, SpriteRenderer, and Animator
- Edit the fields as needed, replace Animation Controller, Sprites, Dialogue, etc.  **More detailed guide will be created soon.**
- Export assets and prefab into a new AssetBundle
- Within your mod's code load the bundle with your preferred sync/async approach (for example. ``AssetBundle.LoadFromFile()``)
- Register the NPC
