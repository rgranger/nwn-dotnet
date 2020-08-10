﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NWN.Systems
{
  public static partial class SkillSystem
  {
    public static Dictionary<int, Func<PlayerSystem.Player, int, int>> RegisterAddCustomFeatEffect = new Dictionary<int, Func<PlayerSystem.Player, int, int>>
    {
            { 1117, HandleAddStrengthMalusFeat },
    };

    public static Dictionary<int, Func<PlayerSystem.Player, int, int>> RegisterRemoveCustomFeatEffect = new Dictionary<int, Func<PlayerSystem.Player, int, int>>
    {
            { 1117, HandleRemoveStrengthMalusFeat },
    };

    private static int HandleAddStrengthMalusFeat(PlayerSystem.Player player, int idMalusFeat)
    {
      // TODO : gérer le cas où on choppe plusieurs fois le même malus
      player.removeableMalus.Add(idMalusFeat, new Skill(idMalusFeat, 0));
      NWNX.Creature.SetRawAbilityScore(player, Enums.Ability.Strength, NWNX.Creature.GetRawAbilityScore(player, Enums.Ability.Strength) - 2);

      return Entrypoints.SCRIPT_HANDLED;
    }

    private static int HandleRemoveStrengthMalusFeat(PlayerSystem.Player player, int idMalusFeat)
    {
      player.removeableMalus.Remove(idMalusFeat);
      NWNX.Creature.SetRawAbilityScore(player, Enums.Ability.Strength, NWNX.Creature.GetRawAbilityScore(player, Enums.Ability.Strength) + 2);

      return Entrypoints.SCRIPT_HANDLED;
    }
  }
}
