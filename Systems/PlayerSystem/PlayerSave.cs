﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NWN.API;
using NWN.Core;
using NWNX.API.Events;

namespace NWN.Systems
{
  public partial class PlayerSystem
  {
    public void HandleBeforePlayerSave(ServerVaultEvents.OnServerCharacterSaveBefore onSaveBefore)
    {
      /* Fix polymorph bug : Lorsqu'un PJ métamorphosé est sauvegardé, toutes ses buffs sont supprimées afin que les stats de 
       * la nouvelle forme ne remplace pas celles du PJ dans son fichier .bic. Après sauvegarde, les stats de la métamorphose 
       * sont réappliquées. 
       * Bug 1 : les PV temporaires de la forme se cumulent avec chaque sauvegarde, ce qui permet d'avoir PV infinis
       * BUG 2 : Les buffs ne faisant pas partie de la métamorphose (appliquées par sort par exemple), ne sont pas réappliquées
       * Ici, la correction consiste à parcourir tous ses buffs et à les réappliquer dans l'event AFTER de la sauvegarde*/

      if (onSaveBefore.Player == null)
        return;

      Log.Info($"Before saving {onSaveBefore.Player.Name}");

      if (onSaveBefore.Player.IsDM || onSaveBefore.Player.IsDMPossessed || onSaveBefore.Player.IsPlayerDM)
      {
        Log.Info("DM detected. Skipping save");
        onSaveBefore.Skip = true;
        return;
      }

      if (Players.TryGetValue(onSaveBefore.Player, out Player player))
      {
        if (onSaveBefore.Player.GetLocalVariable<int>("_DISCONNECTING").HasNothing)
        {
          if (onSaveBefore.Player.ActiveEffects.Any(e => e.EffectType == API.Constants.EffectType.Polymorph))
          {
            player.effectList = onSaveBefore.Player.ActiveEffects.ToList();
            Log.Info($"Polymorph detected, saving effect list");
          }
        }

        // TODO : probablement faire pour chaque joueur tous les check faim / soif / jobs etc ici

        // AFK detection
        if (player.location == player.oid?.Location)
        {
          player.isAFK = true;
          Log.Info("Player AFK");
        }
        else
          player.location = player.oid.Location;

        Log.Info("saved Location");

        player.currentHP = onSaveBefore.Player.HP;

        Log.Info("Saved HP");

        Log.Info($"player location : {player.location.Area}");

        if (player.location.Area?.GetLocalVariable<int>("_AREA_LEVEL").Value == 0)
        {
          Log.Info($"area level : {player.location.Area.GetLocalVariable<int>("_AREA_LEVEL").Value}");
          player.CraftJobProgression();
        }

        if (player.oid.Area.Tag == $"entrepotpersonnel_{player.oid.CDKey}")
          player.location = NwModule.FindObjectsWithTag<NwWaypoint>("wp_outentrepot").FirstOrDefault().Location;

        Log.Info("Craft job progression done");

        player.AcquireSkillPoints();
        Log.Info("Acquire skill points done");

        player.dateLastSaved = DateTime.Now;

        SavePlayerCharacterToDatabase(player);
        Log.Info("Save player to DB done");
        SavePlayerLearnableSkillsToDatabase(player);
        Log.Info("Saved skills to DB");
        SavePlayerLearnableSpellsToDatabase(player);
        Log.Info("Saved Spells to DB");
        SavePlayerStoredMaterialsToDatabase(player);
        Log.Info("Saved materials to DB");
        SavePlayerMapPinsToDatabase(player);
        Log.Info("Saved map pin to DB");
        SavePlayerAreaExplorationStateToDatabase(player);
        Log.Info("Saved area exploration state to DB");
        HandleExpiredContracts(player);
        Log.Info("Handled expired contracts");
        HandleExpiredBuyOrders(player);
        Log.Info("Handled expired buy orders");
        HandleExpiredSellOrders(player);
        Log.Info("Handled expired sell orders");
      }
    }
    public void HandleAfterPlayerSave(ServerVaultEvents.OnServerCharacterSaveAfter onSaveAfter)
    {
      /* Fix polymorph bug : Lorsqu'un PJ métamorphosé est sauvegardé, toutes ses buffs sont supprimées afin que les stats de 
       * la nouvelle forme ne remplace pas celles du PJ dans son fichier .bic. Après sauvegarde, les stats de la métamorphose 
       * sont réappliquées. 
       * Bug 1 : les PV temporaires de la forme se cumulent avec chaque sauvegarde, ce qui permet d'avoir PV infinis
       * BUG 2 : Les buffs ne faisant pas partie de la métamorphose (appliquées par sort par exemple), ne sont pas réappliquées
       * Ici, la correction consiste à ne pas sauvegarder le PJ s'il est métamorphosé, sauf s'il s'agit d'une déconnexion.
       * Mais il se peut que dans ce cas, ses buffs soient perdues à la reco. A vérifier. Si c'est le cas, une meilleure
       * correction pourrait être de parcourir tous ses buffs et de les réappliquer dans l'event AFTER de la sauvegarde*/

      if (onSaveAfter.Player == null)
        return;

      Log.Info($"After saving {onSaveAfter.Player.Name}");

      if (Players.TryGetValue(onSaveAfter.Player, out Player player))
      {
        if (onSaveAfter.Player.GetLocalVariable<int>("_DISCONNECTING").HasNothing)
        {
          if (onSaveAfter.Player.ActiveEffects.Any(e => e.EffectType == API.Constants.EffectType.Polymorph))
          {
            Log.Info("Polymorph detected. Reapplying effect list");
            foreach (API.Effect eff in player.effectList)
              onSaveAfter.Player.ApplyEffect(eff.DurationType, eff, TimeSpan.FromSeconds((double)eff.DurationRemaining));
            Log.Info("Reapplied effect list");
          }
        }
      }
    }
    private static void SavePlayerCharacterToDatabase(Player player)
    {
      Log.Info("Saving to database");

      var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"UPDATE playerCharacters SET areaTag = @areaTag, position = @position, facing = @facing, currentHP = @currentHP, bankGold = @bankGold, dateLastSaved = @dateLastSaved, currentSkillType = @currentSkillType, currentSkillJob = @currentSkillJob, currentCraftJob = @currentCraftJob, currentCraftObject = @currentCraftObject, currentCraftJobRemainingTime = @currentCraftJobRemainingTime, currentCraftJobMaterial = @currentCraftJobMaterial, menuOriginTop = @menuOriginTop, menuOriginLeft = @menuOriginLeft where rowid = @characterId");
      NWScript.SqlBindInt(query, "@characterId", player.characterId);

      //Log.Info($"location : {player.location.Area}");
      //Log.Info($"previous location : {player.previousLocation.Area}");

      if (player.location.Area != null)
      {
        NWScript.SqlBindString(query, "@areaTag", player.location.Area.Tag);
        NWScript.SqlBindVector(query, "@position", player.location.Position);
        NWScript.SqlBindFloat(query, "@facing", player.location.Rotation);
      }
      else
      {
        NWScript.SqlBindString(query, "@areaTag", player.previousLocation.Area.Tag);
        NWScript.SqlBindVector(query, "@position", player.previousLocation.Position);
        NWScript.SqlBindFloat(query, "@facing", player.previousLocation.Rotation);
      }

      NWScript.SqlBindInt(query, "@currentHP", player.currentHP);
      NWScript.SqlBindInt(query, "@bankGold", player.bankGold);
      NWScript.SqlBindString(query, "@dateLastSaved", player.dateLastSaved.ToString());
      NWScript.SqlBindInt(query, "@currentSkillType", (int)player.currentSkillType);
      NWScript.SqlBindInt(query, "@currentSkillJob", player.currentSkillJob);
      NWScript.SqlBindInt(query, "@currentCraftJob", player.craftJob.baseItemType);
      Log.Info($"saved currentCraftJob :{player.craftJob.baseItemType}");
      NWScript.SqlBindString(query, "@currentCraftObject", player.craftJob.craftedItem);
      NWScript.SqlBindFloat(query, "@currentCraftJobRemainingTime", player.craftJob.remainingTime);
      NWScript.SqlBindString(query, "@currentCraftJobMaterial", player.craftJob.material);
      NWScript.SqlBindInt(query, "@menuOriginTop", player.menu.originTop);
      NWScript.SqlBindInt(query, "@menuOriginLeft", player.menu.originLeft);
      NWScript.SqlStep(query);

      Log.Info($"{NWScript.GetName(player.oid)} saved location : {NWScript.GetTag(NWScript.GetAreaFromLocation(player.location))} - {NWScript.GetPositionFromLocation(player.location)} - {NWScript.GetFacingFromLocation(player.location)}");
    }
    private static void SavePlayerLearnableSkillsToDatabase(Player player)
    {
      foreach (KeyValuePair<int, SkillSystem.Skill> skillListEntry in player.learnableSkills)
      {
        var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"INSERT INTO playerLearnableSkills (characterId, skillId, skillPoints, trained) VALUES (@characterId, @skillId, @skillPoints, @trained)" +
        "ON CONFLICT (characterId, skillId) DO UPDATE SET skillPoints = @skillPoints, trained = @trained");
        NWScript.SqlBindInt(query, "@characterId", player.characterId);
        NWScript.SqlBindInt(query, "@skillId", skillListEntry.Key);
        NWScript.SqlBindFloat(query, "@skillPoints", Convert.ToInt32(skillListEntry.Value.acquiredPoints));
        NWScript.SqlBindInt(query, "@trained", Convert.ToInt32(skillListEntry.Value.trained));
        NWScript.SqlStep(query);
      }

      // Ici on vire de la liste tout les skills trained et sauvegardés
      player.learnableSkills = player.learnableSkills.Where(kv => !kv.Value.trained).ToDictionary(kv => kv.Key, KeyValuePair => KeyValuePair.Value);
    }
    private static void SavePlayerLearnableSpellsToDatabase(Player player)
    {
      foreach (KeyValuePair<int, SkillSystem.LearnableSpell> skillListEntry in player.learnableSpells)
      {
        var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"INSERT INTO playerLearnableSpells (characterId, skillId, skillPoints, trained) VALUES (@characterId, @skillId, @skillPoints, @trained)" +
        "ON CONFLICT (characterId, skillId) DO UPDATE SET skillPoints = @skillPoints, trained = @trained");
        NWScript.SqlBindInt(query, "@characterId", player.characterId);
        NWScript.SqlBindInt(query, "@skillId", skillListEntry.Key);
        NWScript.SqlBindFloat(query, "@skillPoints", Convert.ToInt32(skillListEntry.Value.acquiredPoints));
        NWScript.SqlBindInt(query, "@trained", Convert.ToInt32(skillListEntry.Value.trained));
        NWScript.SqlStep(query);
      }

      // Ici on vire de la liste tout les skills trained et sauvegardés
      player.learnableSpells = player.learnableSpells.Where(kv => !kv.Value.trained).ToDictionary(kv => kv.Key, KeyValuePair => KeyValuePair.Value);
    }
    private static void SavePlayerStoredMaterialsToDatabase(Player player)
    {
      if (player.materialStock.Count > 0)
      {
        foreach (string material in player.materialStock.Keys)  
        {
          var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"INSERT INTO playerMaterialStorage (characterId, materialName, materialStock) VALUES (@characterId, @materialName, @materialStock)" +
              $"ON CONFLICT (characterId, materialName) DO UPDATE SET materialStock = @materialStock where characterId = @characterId and materialName = @materialName");
          NWScript.SqlBindInt(query, "@characterId", player.characterId);
          NWScript.SqlBindString(query, "@materialName", material);
          NWScript.SqlBindInt(query, "@materialStock", player.materialStock[material]);
          NWScript.SqlStep(query);
        }
      }
    }
    private static void SavePlayerMapPinsToDatabase(Player player)
    {
      if (player.mapPinDictionnary.Count > 0)
      {
        string queryString = "INSERT INTO playerMapPins (characterId, mapPinId, areaTag, x, y, note) VALUES (@characterId, @mapPinId, @areaTag, @x, @y, @note)" +
          "ON CONFLICT (characterId, mapPinId) DO UPDATE SET x = @x, y = @y, note = @note";

        foreach (MapPin mapPin in player.mapPinDictionnary.Values)
        {
          var query = NWScript.SqlPrepareQueryCampaign(Config.database, queryString);
          NWScript.SqlBindInt(query, "@characterId", player.characterId);
          NWScript.SqlBindInt(query, "@mapPinId", mapPin.id);
          NWScript.SqlBindString(query, "@areaTag", mapPin.areaTag);
          NWScript.SqlBindFloat(query, "@x", mapPin.x);
          NWScript.SqlBindFloat(query, "@y", mapPin.y);
          NWScript.SqlBindString(query, "@note", mapPin.note);
          NWScript.SqlStep(query);
        }
      }
    }
    private static void SavePlayerAreaExplorationStateToDatabase(Player player)
    {
      if (player.areaExplorationStateDictionnary.Count > 0)
      {
        string queryString = "INSERT INTO playerAreaExplorationState (characterId, areaTag, explorationState) VALUES (@characterId, @areaTag, @explorationState)" +
          "ON CONFLICT (characterId, areaTag) DO UPDATE SET explorationState = @explorationState";

        foreach (KeyValuePair<string, string> explorationStateListEntry in player.areaExplorationStateDictionnary)
        {
          var query = NWScript.SqlPrepareQueryCampaign(Config.database, queryString);
          NWScript.SqlBindInt(query, "@characterId", player.characterId);
          NWScript.SqlBindString(query, "@areaTag", explorationStateListEntry.Key);
          NWScript.SqlBindString(query, "@explorationState", explorationStateListEntry.Value);
          NWScript.SqlStep(query);
        }
      }
    }
    private static void HandleExpiredContracts(Player player)
    {
      var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"SELECT expirationDate, rowid from playerPrivateContracts where characterId = @characterId");
      NWScript.SqlBindInt(query, "@characterId", player.characterId);

      while (NWScript.SqlStep(query) > 0)
      {
        int contractId = NWScript.SqlGetInt(query, 1);

        if ((DateTime.Parse(NWScript.SqlGetString(query, 0)) - DateTime.Now).TotalSeconds < 0)
        {
          Task contractExpiration = NwTask.Run(async () =>
          { 
            await NwTask.Delay(TimeSpan.FromSeconds(0.2));
            DeleteExpiredContract(player, contractId);
          });
        }
      }
    }
    private static void DeleteExpiredContract(Player player, int contractId)
    {
      var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"SELECT serializedContract from playerPrivateContracts where rowid = @rowid");
      NWScript.SqlBindInt(query, "@rowid", contractId);
      if (NWScript.SqlStep(query) > 0)
      {
        foreach (string materialString in NWScript.SqlGetString(query, 0).Split("|"))
        {
          string[] descriptionString = materialString.Split("$");
          if (descriptionString.Length == 3)
          {
            if (player.materialStock.ContainsKey(descriptionString[0]))
              player.materialStock[descriptionString[0]] += Int32.Parse(descriptionString[1]);
            else
              player.materialStock.Add(descriptionString[0], Int32.Parse(descriptionString[1]));

            player.oid.SendServerMessage($"Expiration du contrat {contractId} - {descriptionString[1]} unité(s) de {descriptionString[0]} ont été réintégrées à votre entrepôt.");
          }
        }

        var deletionQuery = NWScript.SqlPrepareQueryCampaign(Config.database, $"DELETE from playerPrivateContracts where rowid = @rowid");
        NWScript.SqlBindInt(deletionQuery, "@rowid", contractId);
        NWScript.SqlStep(deletionQuery);
      }
    }
    private static void HandleExpiredBuyOrders(Player player)
    {
      var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"SELECT expirationDate, rowid from playerBuyOrders where characterId = @characterId");
      NWScript.SqlBindInt(query, "@characterId", player.characterId);

      while (NWScript.SqlStep(query) > 0)
      {
        int contractId = NWScript.SqlGetInt(query, 1);

        if ((DateTime.Parse(NWScript.SqlGetString(query, 0)) - DateTime.Now).TotalSeconds < 0)
        {
          Task contractExpiration = NwTask.Run(async () =>
          {
            await NwTask.Delay(TimeSpan.FromSeconds(0.2));
            DeleteExpiredBuyOrder(player, contractId);
          });
        }
      }
    }
    private static void DeleteExpiredBuyOrder(Player player, int contractId)
    {
      var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"SELECT quantity, unitPrice from playerBuyOrders where rowid = @rowid");
      NWScript.SqlBindInt(query, "@rowid", contractId);
      NWScript.SqlStep(query);

      int gold = NWScript.SqlGetInt(query, 0) + NWScript.SqlGetInt(query, 1);
      player.bankGold += gold;
      player.oid.SendServerMessage($"Expiration de l'ordre d'achat {contractId} - {gold} pièce(s) d'or ont été reversées à votre banque.");

      var deletionQuery = NWScript.SqlPrepareQueryCampaign(Config.database, $"DELETE from playerBuyOrders where rowid = @rowid");
      NWScript.SqlBindInt(deletionQuery, "@rowid", contractId);
      NWScript.SqlStep(deletionQuery);
    }
    private static void HandleExpiredSellOrders(Player player)
    {
      var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"SELECT expirationDate, rowid from playerSellOrders where characterId = @characterId");
      NWScript.SqlBindInt(query, "@characterId", player.characterId);

      while (NWScript.SqlStep(query) > 0)
      {
        int contractId = NWScript.SqlGetInt(query, 1);

        if ((DateTime.Parse(NWScript.SqlGetString(query, 0)) - DateTime.Now).TotalSeconds < 0)
        {
          Task contractExpiration = NwTask.Run(async () =>
          {
            await NwTask.Delay(TimeSpan.FromSeconds(0.2));
            DeleteExpiredBuyOrder(player, contractId);
          });
        }
      }
    }
    private static void DeleteExpiredSellOrder(Player player, int contractId)
    {
      var query = NWScript.SqlPrepareQueryCampaign(Config.database, $"SELECT playerSellOrders, quantity from playerSellOrders where rowid = @rowid");
      NWScript.SqlBindInt(query, "@rowid", contractId);
      NWScript.SqlStep(query);

      string material = NWScript.SqlGetString(query, 0);
      int quantity = NWScript.SqlGetInt(query, 1);

      if (player.materialStock.ContainsKey(material))
        player.materialStock[material] += quantity;
      else
        player.materialStock.Add(material, quantity);

      player.oid.SendServerMessage($"Expiration de l'ordre de vente {contractId} - {quantity} unité(s) de {material} sont en cours de transfert vers votre entrepôt.");

      var deletionQuery = NWScript.SqlPrepareQueryCampaign(Config.database, $"DELETE from playerSellOrders where rowid = @rowid");
      NWScript.SqlBindInt(deletionQuery, "@rowid", contractId);
      NWScript.SqlStep(deletionQuery);
    }
  }
}
