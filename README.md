# Cycletopia

Cycletopia is a GTA V Story Mode mod that replaces nearby ambient motor traffic with AI-controlled cyclists.

## Features

- Replaces ambient road vehicles with a varied selection of bicycles and riders
- Preserves the player's vehicle, bicycles, boats, aircraft, trains, mission entities, and persistent entities
- Limits replacements per update to avoid frame-time spikes
- Configurable replacement radius and cyclist speed
- Toggle on or off in-game with `F7`

## Requirements

- Grand Theft Auto V (Story Mode)
- ScriptHookV
- ScriptHookVDotNet 3
- .NET Framework 4.8 to build from source

## Installation

1. Build `src/Cycletopia.csproj`, or use a compiled `Cycletopia.dll`.
2. Copy `Cycletopia.dll` into the GTA V `Scripts` directory.
3. Copy `Cycletopia.ini` into the same `Scripts` directory.
4. Start GTA V in Story Mode.

Do not use ScriptHook mods in GTA Online.

## Configuration

Edit `Cycletopia.ini` to change the replacement radius, target cyclist speed, or maximum replacements per update.

## Controls

- `F7`: toggle Cycletopia on or off
