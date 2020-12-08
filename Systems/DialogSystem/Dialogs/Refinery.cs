﻿using System;
using System.Collections.Generic;
using NWN.Core;
using NWN.Core.NWNX;
using static NWN.Systems.CollectSystem;
using static NWN.Systems.PlayerSystem;

namespace NWN.Systems
{
  class Refinery
  {
    public Refinery(Player player)
    {
      this.DrawWelcomePage(player);
    }
    private void DrawWelcomePage(PlayerSystem.Player player)
    {
      player.menu.Clear();
      player.menu.title = $"Fonderie - Le minerai brut est acheminé de votre entrepôt. Efficacité : -35 %. Que souhaitez-vous fondre ? (Utilisez la commande !set X avant de valider votre choix)";

      foreach (KeyValuePair<string, int> materialEntry in player.materialStock)
      {
        if(materialEntry.Value > 0 && GetOreTypeFromName(materialEntry.Key) != OreType.Invalid)
          player.menu.choices.Add(($"{materialEntry.Key} - {materialEntry.Value} unité(s).", () => HandleRefineOre(player, materialEntry.Key)));
      }

      player.menu.choices.Add(("Quitter", () => player.menu.Close()));
      player.menu.Draw();
    }
    private void HandleRefineOre(PlayerSystem.Player player, string oreName)
    {
      player.menu.Clear();

      if (player.setValue < 100)
      {
        player.menu.title = $"Les ouvriers chargés du transfert ne se dérangeant pas pour moins de 100 unités. (Utilisez la commande !set X avant de valider votre choix)";
        player.menu.choices.Add(("Valider.", () => HandleRefineOre(player, oreName)));
      }
      else
      {
        if (player.setValue > player.materialStock[oreName])
          player.setValue = player.materialStock[oreName];

        player.materialStock[oreName] -= player.setValue;

        float reprocessingEfficiency = 0.3f;

        float value;
        if (float.TryParse(NWScript.Get2DAString("feat", "GAINMULTIPLE", CreaturePlugin.GetHighestLevelOfFeat(player.oid, (int)Feat.Reprocessing)), out value))
          reprocessingEfficiency += reprocessingEfficiency + 3 * value / 100;

        if (float.TryParse(NWScript.Get2DAString("feat", "GAINMULTIPLE", CreaturePlugin.GetHighestLevelOfFeat(player.oid, (int)Feat.ReprocessingEfficiency)), out value))
          reprocessingEfficiency += reprocessingEfficiency + 2 * value / 100;

        if (float.TryParse(NWScript.Get2DAString("feat", "GAINMULTIPLE", CreaturePlugin.GetHighestLevelOfFeat(player.oid, (int)Feat.Connections)), out value))
          reprocessingEfficiency += reprocessingEfficiency + 1 * value / 100;

        CollectSystem.Ore processedOre;
        if (CollectSystem.oresDictionnary.TryGetValue(CollectSystem.GetOreTypeFromName(oreName), out processedOre))
        {
          if (float.TryParse(NWScript.Get2DAString("feat", "GAINMULTIPLE", CreaturePlugin.GetHighestLevelOfFeat(player.oid, (int)processedOre.feat)), out value))
            reprocessingEfficiency += reprocessingEfficiency + 2 * value / 100;

          foreach (KeyValuePair<CollectSystem.MineralType, float> mineralKeyValuePair in processedOre.mineralsDictionnary)
          {
            int refinedMinerals = Convert.ToInt32(player.setValue * mineralKeyValuePair.Value * reprocessingEfficiency);
            string mineralName = CollectSystem.GetNameFromMineralType(mineralKeyValuePair.Key);
            player.materialStock[mineralName] += refinedMinerals;
            NWScript.SendMessageToPC(player.oid, $"Vous venez de raffiner {refinedMinerals} unités de {mineralName}. Les lingots sont en cours d'acheminage vers votre entrepôt.");
          }

          player.menu.title = $"Voilà qui est fait !";
        }
        else
        {
          player.menu.title = $"HRP - Erreur, votre minerai brut n'a pas correctement été reconnu. Le staff a été informé du problème.";
          Utils.LogMessageToDMs($"REFINERY - Could not recognize ore type : {oreName} - Used by : {NWScript.GetName(player.oid)}");
        }
      }

      player.setValue = 0;
      player.menu.choices.Add(("Retour.", () => DrawWelcomePage(player)));
      player.menu.choices.Add(("Quitter", () => player.menu.Close()));
      player.menu.Draw();
    }
  }
}