﻿using System;
using System.Collections.Generic;
using System.Linq;
using NWN.Core;
using NWN.Core.NWNX;
using NWN.ScriptHandlers;

namespace NWN.Systems
{
  public static partial class PlayerSystem
  {
    private static int HandleBeforePlayerSave(uint oidSelf)
    {
      /* Fix polymorph bug : Lorsqu'un PJ métamorphosé est sauvegardé, toutes ses buffs sont supprimées afin que les stats de 
       * la nouvelle forme ne remplace pas celles du PJ dans son fichier .bic. Après sauvegarde, les stats de la métamorphose 
       * sont réappliquées. 
       * Bug 1 : les PV temporaires de la forme se cumulent avec chaque sauvegarde, ce qui permet d'avoir PV infinis
       * BUG 2 : Les buffs ne faisant pas partie de la métamorphose (appliquées par sort par exemple), ne sont pas réappliquées
       * Ici, la correction consiste à ne pas sauvegarder le PJ s'il est métamorphosé, sauf s'il s'agit d'une déconnexion.
       * Mais il se peut que dans ce cas, ses buffs soient perdues à la reco. A vérifier. Si c'est le cas, une meilleure
       * correction pourrait être de parcourir tous ses buffs et de les réappliquer dans l'event AFTER de la sauvegarde*/

      Player player;
      if (Players.TryGetValue(oidSelf, out player))
      {
        if (player.isConnected)
        {
          if (Utils.HasAnyEffect(player.oid, NWScript.EFFECT_TYPE_POLYMORPH))
          {
            Effect eff = NWScript.GetFirstEffect(player.oid);

            while(Convert.ToBoolean(NWScript.GetIsEffectValid(eff)))
            {
              if(NWScript.GetEffectType(eff) != NWScript.EFFECT_TYPE_POLYMORPH)
                player.effectList.Add(eff);
              eff = NWScript.GetNextEffect(player.oid);
            }

            //EventsPlugin.SkipEvent();
            return 0;
          }
        }

        // TODO : probablement faire pour chaque joueur tous les check faim / soif / jobs etc ici

        // AFK detection
        if (player.location != NWScript.GetLocation(player.oid))
        {
          player.location = NWScript.GetLocation(player.oid);
          player.isAFK = false;
        }

        player.currentHP = NWScript.GetCurrentHitPoints(player.oid);

        if (NWScript.GetLocalInt(NWScript.GetArea(player.oid), "REST") != 0)
          player.CraftJobProgression();

        player.AcquireSkillPoints();

        player.dateLastSaved = DateTime.Now;
        player.isAFK = true;

        SavePlayerCharacterToDatabase(player);
      }

      return 0;
    }
    private static int HandleAfterPlayerSave(uint oidSelf)
    {
      /* Fix polymorph bug : Lorsqu'un PJ métamorphosé est sauvegardé, toutes ses buffs sont supprimées afin que les stats de 
       * la nouvelle forme ne remplace pas celles du PJ dans son fichier .bic. Après sauvegarde, les stats de la métamorphose 
       * sont réappliquées. 
       * Bug 1 : les PV temporaires de la forme se cumulent avec chaque sauvegarde, ce qui permet d'avoir PV infinis
       * BUG 2 : Les buffs ne faisant pas partie de la métamorphose (appliquées par sort par exemple), ne sont pas réappliquées
       * Ici, la correction consiste à ne pas sauvegarder le PJ s'il est métamorphosé, sauf s'il s'agit d'une déconnexion.
       * Mais il se peut que dans ce cas, ses buffs soient perdues à la reco. A vérifier. Si c'est le cas, une meilleure
       * correction pourrait être de parcourir tous ses buffs et de les réappliquer dans l'event AFTER de la sauvegarde*/

      Player player;
      if (Players.TryGetValue(oidSelf, out player))
      {
        if (player.isConnected)
        {
          if (Utils.HasAnyEffect(player.oid, NWScript.EFFECT_TYPE_POLYMORPH))
          {
            foreach (Effect eff in player.effectList)
            {
              float duration = EffectPlugin.UnpackEffect(eff).fDuration;
              if (duration > 0)
                NWScript.ApplyEffectToObject(NWScript.DURATION_TYPE_TEMPORARY, eff, player.oid, duration);
              else
                NWScript.ApplyEffectToObject(NWScript.DURATION_TYPE_PERMANENT, eff, player.oid);
            }
          }
        }
      }

      return 0;
    }
    private static void SavePlayerCharacterToDatabase(Player player)
    {
      var query = NWScript.SqlPrepareQueryCampaign(Scripts.database, $"UPDATE playerCharacters SET areaTag = @areaTag, position = @position, facing = @facing, currentHP = @currentHP, dateLastSaved = @dateLastSaved, currentSkillJob = @currentSkillJob, currentCraftJob = @currentCraftJob, currentCraftJobRemainingTime = @currentCraftJobRemainingTime, currentCraftJobMaterial = @currentCraftJobMaterial, frostAttackOn = @frostAttackOn where rowid = @characterId");
      NWScript.SqlBindInt(query, "@characterId", player.characterId);
      NWScript.SqlBindString(query, "@areaTag", NWScript.GetTag(NWScript.GetArea(player.oid)));
      NWScript.SqlBindVector(query, "@position", NWScript.GetPosition(player.oid));
      NWScript.SqlBindFloat(query, "@facing", NWScript.GetFacing(player.oid));
      NWScript.SqlBindInt(query, "@currentHP", player.currentHP);
      NWScript.SqlBindString(query, "@dateLastSaved", player.dateLastSaved.ToString());
      NWScript.SqlBindInt(query, "@currentSkillJob", player.currentSkillJob);
      NWScript.SqlBindString(query, "@currentCraftJob", player.currentCraftJob);
      NWScript.SqlBindFloat(query, "@currentCraftJobRemainingTime", player.currentCraftJobRemainingTime);
      NWScript.SqlBindString(query, "@currentCraftJobMaterial", player.currentCraftJobMaterial);
      NWScript.SqlBindInt(query, "@frostAttackOn", Convert.ToInt32(player.isFrostAttackOn));
      NWScript.SqlStep(query);
    }
    private static void SavePlayerLearnableSkillsToDatabase(Player player)
    {
      foreach(KeyValuePair<int, SkillSystem.Skill> skillListEntry in player.learnableSkills)
      {
        if(skillListEntry.Value.databaseSaved)
        {
          var query = NWScript.SqlPrepareQueryCampaign(Scripts.database, $"UPDATE playerLearnableSkills SET skillPoints = @skillPoints, trained = @trained  where characterId = @characterId and skillId = @skillId");
          NWScript.SqlBindInt(query, "@characterId", player.characterId);
          NWScript.SqlBindInt(query, "@skillId", skillListEntry.Key);
          NWScript.SqlBindFloat(query, "@skillPoints", skillListEntry.Value.acquiredPoints);
          NWScript.SqlBindInt(query, "@trained", Convert.ToInt32(skillListEntry.Value.trained));
          NWScript.SqlStep(query);
        }
        else
        {
          var query = NWScript.SqlPrepareQueryCampaign(Scripts.database, $"INSERT INTO playerLearnableSkills (characterId, skillId, skillPoints, trained) VALUES (@characterId, @skillId, @skillPoints, @trained)");
          NWScript.SqlBindInt(query, "@characterId", player.characterId);
          NWScript.SqlBindInt(query, "@skillId", skillListEntry.Key);
          NWScript.SqlBindFloat(query, "@skillPoints", skillListEntry.Value.acquiredPoints);
          NWScript.SqlBindInt(query, "@trained", Convert.ToInt32(skillListEntry.Value.trained));
          NWScript.SqlStep(query);
        }
      }

      // Ici on vire de la liste tout les skills trained et sauvegardés
      player.learnableSkills = player.learnableSkills.Where(kv => !kv.Value.trained).ToDictionary(kv => kv.Key, KeyValuePair => KeyValuePair.Value);
    }
  }
}