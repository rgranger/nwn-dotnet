﻿using System;
using NWN.Core;
using System.Numerics;

namespace NWN.Systems
{
  public static partial class CommandSystem
  {
    private static void ExecuteRenameCommand(ChatSystem.Context ctx, Options.Result options)
    {
      PlayerSystem.Player player;
      if (PlayerSystem.Players.TryGetValue(ctx.oSender, out player))
      {
        if (Convert.ToBoolean(NWScript.GetIsDM(player.oid)))
        {
          string newName = (string)options.positional[0];

          Action<uint, Vector3> callback = (uint oTarget, Vector3 position) =>
          {
            NWScript.SetName(oTarget, newName);
          };

          player.targetEvent = TargetEvent.LootSaverTarget;
          player.SelectTarget(callback);
        }
        else
          NWScript.SendMessageToPC(player.oid, "Il s'agit d'une commande DM, vous ne pouvez pas en faire usage en PJ.");
      }
    }
  }
}
