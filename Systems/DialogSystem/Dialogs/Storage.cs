﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using NWN.Core;
using NWN.Core.NWNX;
using static NWN.Systems.PlayerSystem;

namespace NWN.Systems
{
  class Storage
  {
    private Dictionary<uint, string> inventoryMaterials;
    public Storage(Player player)
    {
      this.inventoryMaterials = new Dictionary<uint, string>();
      this.DrawWelcomePage(player);
    }
    private void DrawWelcomePage(PlayerSystem.Player player)
    {
      player.menu.Clear();
      player.menu.title = $"Yop, tu veux déposer tes matières premières quelque part ? Vas-y file moi ça. Oublie pas qu'on prend 5 % pour le service.";
      player.menu.choices.Add(($"Tout déposer.", () => HandleDropAll(player)));
      player.menu.choices.Add(($"Déposer une matière en particulier.", () => HandleDropMaterialSelection(player)));
      player.menu.choices.Add(($"A vrai dire, je suis là pour un retrait.", () => HandleWithdrawMaterialSelection(player)));
      player.menu.choices.Add(("Quitter", () => player.menu.Close()));
      player.menu.Draw();
    }
    private void HandleDropAll(PlayerSystem.Player player)
    {
      player.menu.Clear();
      player.menu.title = $"Voilà qui est fait. Merci pour ta contribution à la cause !";

      var oItem = NWScript.GetFirstItemInInventory();

      while(Convert.ToBoolean(NWScript.GetIsObjectValid(oItem)))
      {
        string itemTag = NWScript.GetTag(oItem);
        if (CollectSystem.IsItemCraftMaterial(itemTag))
        {
          int addedOre = NWScript.GetItemStackSize(oItem) - NWScript.GetItemStackSize(oItem) * 5 / 100;

          if (player.materialStock.ContainsKey(itemTag))
            player.materialStock[itemTag] += addedOre;
          else
            player.materialStock.Add(itemTag, addedOre);

          NWScript.DestroyObject(oItem);
        }

        oItem = NWScript.GetNextItemInInventory();
      }

      player.menu.choices.Add(($"Retour.", () => DrawWelcomePage(player)));
      player.menu.choices.Add(("Quitter", () => player.menu.Close()));
      player.menu.Draw();
    }
    private void HandleDropMaterialSelection(PlayerSystem.Player player)
    {
      player.menu.Clear();
      player.menu.title = $"D'ac. Dépôt de quelle matière première ? (Utilisez !set X pour préciser la quantité avant de valider votre choix)";

      var oItem = NWScript.GetFirstItemInInventory();

      while (Convert.ToBoolean(NWScript.GetIsObjectValid(oItem)))
      {
        string itemTag = NWScript.GetTag(oItem);
        if (CollectSystem.IsItemCraftMaterial(itemTag))
        {
          inventoryMaterials.Add(oItem, itemTag);
        }

        oItem = NWScript.GetNextItemInInventory();
      }

      foreach (string value in inventoryMaterials.Values.Distinct())
        player.menu.choices.Add(($"{value}.", () => HandleValidateDropMaterial(player, value)));

      player.menu.choices.Add(($"Retour.", () => DrawWelcomePage(player)));
      player.menu.choices.Add(("Quitter", () => player.menu.Close()));
      player.menu.Draw();
    }
    private void HandleValidateDropMaterial(PlayerSystem.Player player, string material)
    {
      player.menu.Clear();

      if (player.setValue <= 0)
      {
        player.menu.title = $"Plait-il ? Je n'ai pas bien compris. (Utilisez la commande !set X avant de valider votre choix)";
        player.menu.choices.Add(($"Valider.", () => HandleValidateDropMaterial(player, material)));
      }
      else
      {
        int valueToStock = player.setValue;
        foreach (KeyValuePair<uint, string> materialEntry in inventoryMaterials.Where(v => v.Value == material))
        {
          if (Convert.ToBoolean(NWScript.GetIsObjectValid(materialEntry.Key)))
          {
            int stackSize = NWScript.GetItemStackSize(materialEntry.Key);
            if (stackSize >= valueToStock)
            {
              player.materialStock[material] += (valueToStock - valueToStock * 5 / 100);
              if (stackSize == valueToStock)
                NWScript.DestroyObject(materialEntry.Key);
              else
                NWScript.SetItemStackSize(materialEntry.Key, stackSize - valueToStock);

              break;
            }
            else
            {
              player.materialStock[material] += (stackSize - stackSize * 5 / 100);
              NWScript.DestroyObject(materialEntry.Key);
            }
          }
        }
        player.menu.title = $"Voilà qui est fait !";
      }

      player.setValue = 0;
      player.menu.choices.Add(($"Retour.", () => DrawWelcomePage(player)));
      player.menu.choices.Add(("Quitter", () => player.menu.Close()));
      player.menu.Draw();
    }
    private void HandleWithdrawMaterialSelection(PlayerSystem.Player player)
    {
      player.menu.Clear();
      player.menu.title = $"D'ac. Retrait de quelle matière première ? (Utilisez !set X pour préciser la quantité avant de valider votre choix)";

      foreach (KeyValuePair<string, int> stockEntry in player.materialStock.Where(v => v.Value > 0))
        player.menu.choices.Add(($"{stockEntry.Key} - {stockEntry.Value}.", () => HandleValidateWithdrawMaterial(player, stockEntry.Key)));

      player.menu.choices.Add(($"Retour.", () => DrawWelcomePage(player)));
      player.menu.choices.Add(("Quitter", () => player.menu.Close()));
      player.menu.Draw();
    }
    private void HandleValidateWithdrawMaterial(PlayerSystem.Player player, string material)
    {
      player.menu.Clear();

      if (player.setValue <= 0)
      {
        player.menu.title = $"Plait-il ? Je n'ai pas bien compris. (Utilisez la commande !set X avant de valider votre choix)";
        player.menu.choices.Add(($"Valider.", () => HandleValidateDropMaterial(player, material)));
      }
      else
      {
        string itemTemplate = CollectSystem.GetCraftMaterialItemTemplate(material);
        if (itemTemplate != "")
        {
          int remainingValue = 0;

          if (player.setValue >= player.materialStock[material])
          {
            player.menu.title = $"Ouais, j'te file tout en gros. D'ac, démerdes-toi avec ça.";
            remainingValue = player.materialStock[material];
            player.materialStock[material] = 0;
          }
          else
          {
            player.menu.title = $"{player.setValue} de {material} ? C'est parti !";
            remainingValue = player.setValue;
            player.materialStock[material] -= player.setValue;
          }

          while (remainingValue > 0)
          {
            if (remainingValue >= 50000)
            {
              NWScript.SetName(NWScript.CreateItemOnObject(itemTemplate, player.oid, 50000, material), material);
              remainingValue -= 50000;
            }
            else
            {
              NWScript.SetName(NWScript.CreateItemOnObject(itemTemplate, player.oid, remainingValue, material), material);
              break;
            }
          }
        }
      }

      player.setValue = 0;
      player.menu.choices.Add(($"Retour.", () => DrawWelcomePage(player)));
      player.menu.choices.Add(("Quitter", () => player.menu.Close()));
      player.menu.Draw();
    }
  }
}