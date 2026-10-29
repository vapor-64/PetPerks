# PetPerks

A [Stardew Valley](https://www.stardewvalley.net/) mod. Each pet gets a perk: cats scare crows, dogs herd farm animals in and out, and turtles boost fish pond roe.

## Requirements

- Stardew Valley 1.6
- [SMAPI](https://smapi.io/) 4.0.0 or later
- [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098), optional, for in-game settings

## Installation

1. Install SMAPI.
2. Unzip this mod into your `Mods` folder.
3. Run the game through SMAPI.

## Features

- **Cats** scare away crows, so you no longer need scarecrows to protect your crops.
- **Dogs** open the barn and coop doors at 6:10 AM and let the animals out, then herd them back in from 6:00 PM, bring in stragglers and close the doors at 7:00 PM. Doors stay shut on rainy days, in winter and on festival days.
- **Turtles** make fish ponds produce 1.5x more roe.

Each perk works while at least one pet of that kind lives on your farm. In multiplayer only the host herds the animals.

## Configuration

Edit `config.json` (created after the first launch) or use Generic Mod Config Menu.

| Setting | Default | Description |
|---|---|---|
| `CatsScareCrows` | `true` | Cat perk on/off. |
| `DogsHerdAnimals` | `true` | Dog perk on/off. |
| `HerdOutTime` | `610` | When the dog lets the animals out (24h, 610 = 6:10 AM). |
| `HerdInTime` | `1800` | When the dog starts herding the animals in. |
| `CloseDoorsTime` | `1900` | When stragglers are brought in and the doors close. |
| `TurtlesBoostRoe` | `true` | Turtle perk on/off. |
| `TurtleRoeMultiplier` | `1.5` | Roe multiplier (fractions round randomly). |

## Console commands

- `petperks_herd out|in|close` runs the dog's herding immediately, for testing.

## Building from source

`dotnet build` (game path is auto-detected by ModBuildConfig; the mod is deployed to `Mods/PetPerks`).