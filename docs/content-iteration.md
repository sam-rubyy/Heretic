# Content Iteration Guide

How to add and wire up new gameplay content (rooms, enemies, abilities, loot, weapons, bosses) using the existing systems.

## Generation Flow (what happens at runtime)
- `RoomManager.BuildFloor` uses a `FloorConfig` asset to build a grid and seed RNG. It calls `FloorLayoutGenerator.Generate`, which lays out a main path plus optional branches and assigns a `RoomTemplate` to each grid cell based on required doors and depth.
- Each `RoomTemplate` points at a Room prefab; `RoomManager` instantiates those prefabs, calls `Room.Configure`, and links neighboring `RoomDoor`s. If a neighbor or matching opposite door is missing, the door is locked.
- When a room activates, it locks doors (optional) and spawns an encounter via its `EnemySpawner`s. Enemy selection and counts come from the active `FloorConfig`.
- Enemies die -> `EnemyLootDropper` rolls loot pools -> `DroppedItem` spawns -> player collides -> `ItemManager` grants the `ItemBase`.

## Rooms and Floor Configs
- **Create a Room prefab**: add the `Room` component, child `RoomDoor`s (set `Direction` and add `entryPoint`), and `EnemySpawner`s. Spawners should have spawn points and optionally a default enemy prefab for test scenes.
- **Make a RoomTemplate asset** (`Create > Heretic/Rooms/Room Template`):
  - Assign the Room prefab.
  - Set `DoorLayout` booleans to match the doors present.
  - Choose `RoomType` (Start, Normal, Treasure, Shop, Boss).
  - Set weight (selection bias), difficulty rating, and allowed depth range (`MinDepth`/`MaxDepth` along the main path).
- **Register in a FloorConfig** (`Create > Heretic/Rooms/Floor Config`):
  - Assign `StartRoom` and `BossRoom` templates.
  - Add templates to `NormalRooms` and/or `SpecialRooms`. Special rooms are picked only if no normal template fits the required door layout at that cell.
  - Tune layout knobs: `MainPathLength`, `MaxBranches`, `BranchLength`.
  - Enemy knobs: `BaseEnemyCountRange`, `AdditionalEnemiesPerDepth`, `BossEnemyCount`, `BaseDifficulty`, `DifficultyPerDepth`.

## Enemies
- **Prefab requirements**:
  - Derive behavior from `EnemyBase` (or a subclass). Attach `EnemyHealth` and optionally `EnemyLootDropper`.
  - Add movement/AI scripts as needed.
  - Add an `AbilityController` if the enemy uses abilities (set `autoUseAbilities` and optionally a `target`; it can auto-find the player).
- **Link to rooms**: Place the enemy prefab on an `EnemySpawner` in your Room prefab, or let runtime spawning pick it via pools (see below). Spawners are auto-owned by the room in `Room.Configure`.
- **Enemy pool entry** (for procedural floors):
  - Open your `FloorConfig`, add an entry to `EnemyPool`: prefab, weight, `MinDifficulty`/`MaxDifficulty`.
  - Difficulty used = `BaseDifficulty + depth * DifficultyPerDepth` (+ template difficulty tweak). Matching pool entries can be picked multiple times per room spawn.
- **Bosses**:
  - Use `BossEnemy` to layer boss loot on top of base drops. Assign `bossLootPool`, `bossLootRolls`, `guaranteedBossDrops`, and whether to include the base loot pool.
  - Put the boss prefab in a boss `RoomTemplate` and set that template on `BossRoom` in the `FloorConfig`.

## Abilities
- **Create an ability**: derive from `Ability` (e.g., `ProjectileAbility`, `AreaDamageAbility`) via `Create > Abilities/...`. Set `AbilityId`, cooldown, weights, and parameters.
- **Add to an enemy**: put an `AbilityController` on the enemy prefab and add ability slots in the inspector. It will auto-use ready abilities on a cadence.
- **Add to the player**:
  - `PlayerAbilityController` holds slots and input bindings (Q/E/Space by default).
  - Grant via an item (`AbilityItem` gives an ability on collect, removes on drop) or by adding the ability to the controller in the inspector for starting loadouts.

## Items and Loot
- **Item shape**: derive from `ItemBase` (`Create > Items/...`). Implement `OnCollected`/`OnRemoved` for bespoke effects. To affect stats/shooting, implement:
  - `IShotModifier` to change `ShotParams` (fire rate, spread, projectiles, range).
  - `IBulletModifier` to change `BulletParams` (damage, speed, lifetime, knockback, on-hit/travel effects).
  - `IPlayerStatModifier` to touch `PlayerStats`.
  - `IItemModifierPriority` to order modifiers (higher priority runs later).
- **Granting items**: `DroppedItem` spawns in the world; on player trigger it calls `ItemManager.AddItem`, which applies modifiers and events.
- **Loot pools**:
  - Create an `EnemyLootPool` (`Create > Enemies/Loot Pool`) and add `LootEntry` items with weights.
  - On enemies, set `EnemyLootDropper` with a pool, drop chance, roll count, and guaranteed drops. Use `preventDuplicateDrops` if you want unique rolls per kill.
  - Bosses can use a separate pool via `BossEnemy` while optionally including the base pool.
- **Make enemies drop new stuff**: add your new item to an `EnemyLootPool` and assign that pool to enemy prefabs (or to variants used on later floors). For floor-specific loot, swap pools when configuring that floor’s prefabs.

## Weapons
- **Data**: make a `WeaponData` asset (`Create > Weapons/Weapon Data`) and set `ShotParams` (rate, burst, spread, range) plus `BulletParams` (damage, speed, lifetime/“range”, knockback, effects).
- **Prefab**: set up a `WeaponBase` with a `firePoint`, `bulletPrefab`, `WeaponData`, and references to `ItemManager`/`PlayerStats`. `WeaponBase` already applies item and stat modifiers to shots/bullets.
- **Equipping**: `PlayerAttack` holds the current weapon. Swap via `PlayerAttack.SetWeapon(...)` from an item you create (e.g., a weapon pickup item that instantiates/assigns the weapon prefab or swaps `WeaponData`).

## Special / Utility Rooms
- Use `RoomType` to mark Treasure/Shop/etc. Build their prefabs with appropriate content (e.g., free chests, vendors) and set `DoorLayout` correctly.
- Add them to `SpecialRooms` in a `FloorConfig`; they’re chosen when a normal template does not match the required doors at a cell. Weight/min/max depth still apply.

## Quick Checklists
- **New enemy type**
  - Build prefab with `EnemyBase` + `EnemyHealth` + (optional) `EnemyLootDropper` + (optional) `AbilityController`.
  - Wire visuals/AI and test standalone.
  - Add to `FloorConfig.enemyPool` with difficulty range & weight.
  - For handcrafted rooms, drop it into `EnemySpawner`s on the Room prefab.
- **New ability**
  - Create `Ability` asset; set cooldown/weights/params.
  - Add to enemy `AbilityController` slots or make an `AbilityItem` to grant it to the player.
- **New item/loot**
  - Create `ItemBase` (or reuse `AbilityItem`, `DamageBoostItem`, etc.).
  - If it modifies combat, implement the appropriate modifier interfaces.
  - Add to an `EnemyLootPool`, set pools on `EnemyLootDropper`s, and ensure `DroppedItem` prefab has a suitable icon.
- **New room/boss**
  - Build Room prefab with doors/spawners.
  - Create `RoomTemplate` with door layout, type, weights, depth range.
  - Assign start/boss templates in `FloorConfig`; add room templates to normal/special lists.
  - For bosses, use `BossEnemy` and set boss loot/abilities.
- **New weapon**
  - Create `WeaponData` + bullet prefab.
  - Configure `WeaponBase` prefab; hook it to `PlayerAttack` or a weapon item pickup.

## Testing Tips
- In the scene with `RoomManager`, enable `GenerateOnStart` and set a `GenerationSeed` for reproducible layouts.
- Use a tiny `MainPathLength`/`MaxBranches` while iterating on rooms to see your new templates quickly.
- Temporarily boost `BaseEnemyCountRange` or loot `rollCount` to stress-test encounters and drops.
- Verify door links: if a door locks immediately, check that both adjacent rooms have opposing doors set in their `RoomTemplate` door layouts.
