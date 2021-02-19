﻿using NWN.API;
using NWN.API.Events;
using NWN.Services;
using System;
using NWN.API.Constants;
using NWN.Core;
using System.Threading.Tasks;

namespace NWN.Systems
{
  [ServiceBinding(typeof(AreaSystem))]
  partial class AreaSystem
  {
    private static int[] plageCrittersAppearances = new int[] { 1984, 1985, 1986, 3160, 3155, 3156, 1956, 1957, 1958, 1959, 1960, 1961, 1962, 291, 292, 1964, 4310, 1428, 1430, 1980, 1981, 3261, 3262, 3263 };
    private static int[] caveCrittersAppearances = new int[] { 3197, 3198, 3199, 3200, 3202, 3204, 3205, 3206, 3207, 3208, 3209, 3210, 3999, 6425, 6426, 6427, 6428, 6429, 6430, 6431, 6432, 6433, 6434, 6435, 6436, 3397, 3398, 3400, 3434 };
    private static int[] cityCrittersAppearances = new int[] { 1983, 1390, 1391, 1392, 1393, 1394, 1395, 1396, 1397, 1398, 1399, 1400, 1401, 1402, 1403, 1404, 1405, 1406, 1407, 1408, 1409, 1410, 1411, 4385, 4408, 4112, 4113, 2505 };
    private static int[] civilianAppearances = new int[] { 220, 221, 222, 224, 225, 226, 227, 228, 229, 231, 4357, 4358, 238, 239, 240, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269, 270, 271, 272, 273, 278, 279, 280, 281, 282, 283, 284, 212, 4379 };
    private static int[] genericCrittersAppearances = new int[] { 3259, 1181, 1182, 1183, 1184, 4370, 4371, 1794, 1988, 3213, 3214, 3215, 3216, 3222, 3223, 1339, 3445, 3520, 4338, 1335, 1336, 1966, 1967, 1968, 1969, 1970, 1971, 1972, 4221, 1025, 1026, 1797, 3237, 3238, 3239, 3240, 3241, 1328, 1941, 1330, 1438, 496, 509, 522, 535, 1784, 1785, 1787, 1788, 1789, 1791, 1855, 1856, 1857, 1858, 1859, 1860, 2589, 1334, 1973, 1974, 4309, 4310, 4320, 4321, 4322, 34, 142, 1796, 1340, 3192, 3193, 3194, 3195, 3196, 1341, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1013, 1014, 1015, 1016, 1017, 1019, 1020, 1255, 3152, 3153, 3157, 3158, 3159, 3161, 3162, 3163, 3164, 3165, 3166, 3167, 3168, 3154, 3148, 3149, 1975, 1976, 1977, 1978, 1979, 2506, 3043, 3044, 3045, 3046, 3047, 1275, 1947, 1949, 1950, 1951, 1952, 6365, 6408, 31, 145, 144, 3305, 4364, 1982, 1749, 1750, 1751, 1332, 1333, 1987, 1863, 1337, 1295, 1329, 3310, 3311, 1802, 1803, 1804, 1805, 8, 35, 37, 4115, 4116, 4117, 4118, 4119, 4120, 4121, 4122, 4123, 3138, 1338 };

    private static void HandleSpawnWaypoint(NwWaypoint spawnPoint)
    {
      Task generateLoot = NwTask.Run(async () =>
      {
        await NwTask.Delay(TimeSpan.FromSeconds(0.1));

        switch (spawnPoint.GetLocalVariable<string>("_SPAWN_TYPE").Value)
        {
          case "npc":
            SpawnNPCFromSpawnPoint(spawnPoint);
            break;
          case "civilian":
            SetRandomAppearanceAndNameFrom2da(SpawnNPCFromSpawnPoint(spawnPoint), civilianAppearances);
            break;
          case "critter":
            SetRandomAppearanceAndNameFrom2da(SpawnNPCFromSpawnPoint(spawnPoint), genericCrittersAppearances, true);
            break;
          case "critter_plage":
            SetRandomAppearanceAndNameFrom2da(SpawnNPCFromSpawnPoint(spawnPoint), plageCrittersAppearances, true);
            break;
          case "critter_cave":
            SetRandomAppearanceAndNameFrom2da(SpawnNPCFromSpawnPoint(spawnPoint), caveCrittersAppearances, true);
            break;
          case "critter_city":
            SetRandomAppearanceAndNameFrom2da(SpawnNPCFromSpawnPoint(spawnPoint), cityCrittersAppearances, true);
            break;
          default:
            SpawnCreatureFromSpawnPoint(spawnPoint);
            break;
        }
        return true;
      });
    }
    private static uint SpawnCreatureFromSpawnPoint(NwWaypoint spawnPoint)
    {
      NwCreature creature = NwCreature.Create(spawnPoint.GetLocalVariable<string>("_CREATURE_TEMPLATE").Value, spawnPoint.Location);

      switch (creature.Tag)
      {
        case "ratgarou":
        case "wereboar":
        case "sim_wraith":
          break;
        default:
          creature.AiLevel = AiLevel.Low;
          nativeEventService.Subscribe<NwCreature, CreatureEvents.OnDeath>(creature, LootSystem.HandleLoot);
          break;
      }

      return creature;
    }
    private static NwCreature SpawnNPCFromSpawnPoint(NwWaypoint spawnPoint)
    {
      NwCreature creature = NwCreature.Create(spawnPoint.GetLocalVariable<string>("_CREATURE_TEMPLATE").Value, spawnPoint.Location);
      nativeEventService.Subscribe<NwCreature, CreatureEvents.OnDeath>(creature, OnDeathSpawnNPCWaypoint);
      creature.GetLocalVariable<string>("_WAYPOINT_TEMPLATE").Value = spawnPoint.ResRef;
      creature.GetLocalVariable<API.Location>("_SPAWN_LOCATION").Value = spawnPoint.Location;
      creature.AiLevel = AiLevel.Low;
      spawnPoint.Destroy();

      //Log.Info($"SPAWN - From {spawnPoint.GetLocalVariable<string>("_CREATURE_TEMPLATE").Value} -  {creature.Name} in {creature.GetLocalVariable<API.Location>("_SPAWN_LOCATION").Value.Area.Name}");

      return creature;
    }
    private static void SetRandomAppearanceAndNameFrom2da(NwCreature creature, int[] appearanceArray, bool setName = false)
    {
      int appearance = appearanceArray[NWN.Utils.random.Next(0, appearanceArray.Length)];
      creature.CreatureAppearanceType = (AppearanceType)appearance;

      creature.PortraitResRef = NWScript.Get2DAString("appearance", "PORTRAIT", appearance);
      creature.ActionRandomWalk();

      if (!setName)
        return;

      if (Int32.TryParse(NWScript.Get2DAString("appearance", "STRING_REF", appearance), out int value))
        creature.Name = NWScript.GetStringByStrRef(value);
      else
        Utils.LogMessageToDMs($"Apparence {appearance} - Nom non défini.");


    }
    private static void OnDeathSpawnNPCWaypoint(CreatureEvents.OnDeath onDeath)
    {
      NwWaypoint waypoint = NwWaypoint.Create(onDeath.KilledCreature.GetLocalVariable<string>("_WAYPOINT_TEMPLATE"), onDeath.KilledCreature.GetLocalVariable<API.Location>("_SPAWN_LOCATION").Value);
      waypoint.GetLocalVariable<string>("_CREATURE_TEMPLATE").Value = onDeath.KilledCreature.ResRef;
    }
  }
}