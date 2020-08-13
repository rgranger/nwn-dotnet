﻿using System;
using System.Collections.Generic;
using NWN.Enums;
using NWN.NWNX;

namespace NWN.Systems
{
  public static partial class CommandSystem
  {
    private static void ExecuteTestCommand(ChatSystem.Context ctx, Options.Result options)
    {
      PlayerSystem.Player player;
      if (PlayerSystem.Players.TryGetValue(ctx.oSender, out player))
      {
        foreach (KeyValuePair<int, SkillSystem.Skill> SkillListEntry in player.learnableSkills)
        {
          player.SendMessage($"feat : {SkillListEntry.Key}");
        }
      }
    }
  }
}