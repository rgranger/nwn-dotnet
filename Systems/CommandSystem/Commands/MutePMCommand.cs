﻿using NWN.NWNX;

namespace NWN.Systems
{
  public static partial class CommandSystem
  {
    private static void ExecuteMutePMCommand(ChatSystem.Context ctx, Options.Result options)
    {
      if (NWScript.GetIsObjectValid(ctx.oTarget) == 1)
      {
        if (ObjectPlugin.GetInt(ctx.oSender, "__BLOCK_" + ctx.NWScript.GetName(oTarget.oid) + "_MP") == 0)
        {
          ObjectPlugin.SetInt(ctx.oSender, "__BLOCK_" + ctx.NWScript.GetName(oTarget.oid) + "_MP", 1, true);
          NWScript.SendMessageToPC(ctx.oSender, "Vous bloquez désormais tous les mps de " + ctx.NWScript.GetName(oTarget.oid) + ". Cette commande ne fonctionne pas sur les Dms.");
        }
        else
        {
          ObjectPlugin.DeleteInt(ctx.oSender, "__BLOCK_" + ctx.NWScript.GetName(oTarget.oid) + "_MP");
          NWScript.SendMessageToPC(ctx.oSender, "Vous ne bloquez plus les mps de " + ctx.NWScript.GetName(oTarget.oid));
        }
      }
      else
      {
        if (ObjectPlugin.GetInt(ctx.oSender, "__BLOCK_ALL_MP") == 0)
        {
          ObjectPlugin.SetInt(ctx.oSender, "__BLOCK_ALL_MP", 1, true);
          NWScript.SendMessageToPC(ctx.oSender, "Vous bloquez désormais l'affichage global des mps. Vous recevrez cependant toujours ceux des DMs.");
        }
        else
        {
          ObjectPlugin.DeleteInt(ctx.oSender, "__BLOCK_ALL_MP");
          NWScript.SendMessageToPC(ctx.oSender, "Vous réactivez désormais l'affichage global des mps. Vous ne recevrez cependant pas ceux que vous bloqué individuellement.");
        }
      }
    }
  }
}
