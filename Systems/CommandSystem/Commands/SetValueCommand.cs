﻿using System;

namespace NWN.Systems
{
  public static partial class CommandSystem
  {
    private static void ExecuteSetValueCommand(ChatSystem.Context ctx, Options.Result options)
    {
      PlayerSystem.Player player;
      if (PlayerSystem.Players.TryGetValue(ctx.oSender, out player))
      {
        if (((string)options.positional[0]).Length != 0)
        {
          if (Int32.TryParse((string)options.positional[0], out int value))
          {
            player.setValue = value;
            return;
          }   
        }

        player.setValue = Config.invalidInput;
      }
    }
  }
}
