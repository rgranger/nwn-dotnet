﻿using System;
using System.Collections.Generic;
using NWN.Core;
using NWN.Core.NWNX;
using static NWN.Systems.PlayerSystem;

namespace NWN.Systems
{
  public static partial class DialogSystem
  {
    public static Dictionary<string, Func<uint, int>> Register = new Dictionary<string, Func<uint, int>>
        {
            { "diag_root", HandleDialogStart },
            { "intro_start", HandleIntroStart },
            { "intro_validation", HandleIntroValidation },
        };

    private static int HandleDialogStart(uint oidSelf)
    {
      uint oPC = NWScript.GetLastSpeaker();

      if(!Convert.ToBoolean(NWScript.GetIsObjectValid(oPC)))
        oPC = NWScript.GetLastUsedBy();

      Player player;
      if (Players.TryGetValue(oPC, out player))
      {
        uint shop; 
        string tag = NWScript.GetTag(oidSelf);

        switch (tag)
        {
          case "bank_npc":
            new Bank(player);
            break;
          case "bank_gold":
            new BankGold(player);
            break;
          case "storage_npc":
            new Storage(player);
            break;
          case "intro_mirror":
            new IntroMirror(player);
            break;
          case "refinery":
            new Refinery(player);
            break;
          case "decoupe":
            new Scierie(player);
            break;
          case "tannerie_peau":
            new Tannerie(player);
            break;
          case "bal_system":
            new Messenger(player);
            break;
          case "hventes":
            new HotelDesVentes(player);
            break;
          case "jukebox":
            new Jukebox(player, oidSelf);
            break;
          case "blacksmith":
            shop = NWScript.GetNearestObjectByTag("blacksmith_shop", oidSelf);

            if (!Convert.ToBoolean(NWScript.GetIsObjectValid(shop)))
            {
              shop = NWScript.CreateObject(NWScript.OBJECT_TYPE_STORE, "generic_shop_res", NWScript.GetLocation(oidSelf), 0, "blacksmith_shop");
              NWScript.SetLocalObject(shop, "_STORE_NPC", oidSelf);

              foreach (int baseItemType in Craft.Collect.System.forgeBasicBlueprints)
              {
                Craft.Blueprint blueprint = new Craft.Blueprint(baseItemType);

                if (!Craft.Collect.System.blueprintDictionnary.ContainsKey(baseItemType))
                  Craft.Collect.System.blueprintDictionnary.Add(baseItemType, blueprint);

                uint oBlueprint = NWScript.CreateItemOnObject("blueprintgeneric", shop, 1, "blueprint");
                NWScript.SetName(oBlueprint, $"Patron original : {blueprint.name}");
                NWScript.SetLocalInt(oBlueprint, "_BASE_ITEM_TYPE", baseItemType);
                ItemPlugin.SetBaseGoldPieceValue(oBlueprint, blueprint.goldCost * 10);
              }

              foreach (Feat feat in SkillSystem.forgeBasicSkillBooks)
              {
                uint skillBook = NWScript.CreateItemOnObject("skillbookgeneriq", shop, 1, "skillbook");
                ItemPlugin.SetItemAppearance(skillBook, NWScript.ITEM_APPR_TYPE_SIMPLE_MODEL, 2, Utils.random.Next(0, 50));
                NWScript.SetLocalInt(skillBook, "_SKILL_ID", (int)feat);

                int value;
                if (int.TryParse(NWScript.Get2DAString("feat", "FEAT", (int)feat), out value))
                  NWScript.SetName(skillBook, NWScript.GetStringByStrRef(value));

                if (int.TryParse(NWScript.Get2DAString("feat", "DESCRIPTION", (int)feat), out value))
                  NWScript.SetDescription(skillBook, NWScript.GetStringByStrRef(value));

                if (int.TryParse(NWScript.Get2DAString("feat", "CRValue", (int)feat), out value))
                  ItemPlugin.SetBaseGoldPieceValue(skillBook, value * 1000);
              }

              uint craftTool = NWScript.CreateItemOnObject("oreextractor", shop, 1, "oreextractor");
              ItemPlugin.SetBaseGoldPieceValue(craftTool, 50);
              NWScript.SetLocalInt(craftTool, "_DURABILITY", 10);

              craftTool = NWScript.CreateItemOnObject("forgehammer", shop, 1, "forgehammer");
              ItemPlugin.SetBaseGoldPieceValue(craftTool, 50);
              NWScript.SetLocalInt(craftTool, "_DURABILITY", 5);
            }

            NWScript.OpenStore(shop, player.oid);
            break;
          case "woodworker":
            shop = NWScript.GetNearestObjectByTag("woodworker_shop", oidSelf);

            if (!Convert.ToBoolean(NWScript.GetIsObjectValid(shop)))
            {
              shop = NWScript.CreateObject(NWScript.OBJECT_TYPE_STORE, "generic_shop_res", NWScript.GetLocation(oidSelf), 0, "woodworker_shop");
              NWScript.SetLocalObject(shop, "_STORE_NPC", oidSelf);

              foreach (int baseItemType in Craft.Collect.System.woodBasicBlueprints)
              {
                Craft.Blueprint blueprint = new Craft.Blueprint(baseItemType);

                if (!Craft.Collect.System.blueprintDictionnary.ContainsKey(baseItemType))
                  Craft.Collect.System.blueprintDictionnary.Add(baseItemType, blueprint);

                uint oBlueprint = NWScript.CreateItemOnObject("blueprintgeneric", shop, 1, "blueprint");
                NWScript.SetName(oBlueprint, $"Patron original : {blueprint.name}");
                NWScript.SetLocalInt(oBlueprint, "_BASE_ITEM_TYPE", baseItemType);
                ItemPlugin.SetBaseGoldPieceValue(oBlueprint, blueprint.goldCost * 10);
              }

              foreach (Feat feat in SkillSystem.woodBasicSkillBooks)
              {
                uint skillBook = NWScript.CreateItemOnObject("skillbookgeneriq", shop, 1, "skillbook");
                ItemPlugin.SetItemAppearance(skillBook, NWScript.ITEM_APPR_TYPE_SIMPLE_MODEL, 2, Utils.random.Next(0, 50));
                NWScript.SetLocalInt(skillBook, "_SKILL_ID", (int)feat);

                int value;
                if (int.TryParse(NWScript.Get2DAString("feat", "FEAT", (int)feat), out value))
                  NWScript.SetName(skillBook, NWScript.GetStringByStrRef(value));

                if (int.TryParse(NWScript.Get2DAString("feat", "DESCRIPTION", (int)feat), out value))
                  NWScript.SetDescription(skillBook, NWScript.GetStringByStrRef(value));

                if (int.TryParse(NWScript.Get2DAString("feat", "CRValue", (int)feat), out value))
                  ItemPlugin.SetBaseGoldPieceValue(skillBook, value * 1000);
              }

              uint craftTool = NWScript.CreateItemOnObject("oreextractor", shop, 1, "oreextractor");
              ItemPlugin.SetBaseGoldPieceValue(craftTool, 50);
              NWScript.SetLocalInt(craftTool, "_DURABILITY", 10);

              craftTool = NWScript.CreateItemOnObject("forgehammer", shop, 1, "forgehammer");
              ItemPlugin.SetBaseGoldPieceValue(craftTool, 50);
              NWScript.SetLocalInt(craftTool, "_DURABILITY", 5);
            }

            NWScript.OpenStore(shop, player.oid);
            break;
          case "tanneur":
            shop = NWScript.GetNearestObjectByTag("tannery_shop", oidSelf);

            if (!Convert.ToBoolean(NWScript.GetIsObjectValid(shop)))
            {
              shop = NWScript.CreateObject(NWScript.OBJECT_TYPE_STORE, "generic_shop_res", NWScript.GetLocation(oidSelf), 0, "tannery_shop");
              NWScript.SetLocalObject(shop, "_STORE_NPC", oidSelf);

              foreach (int baseItemType in Craft.Collect.System.leatherBasicBlueprints)
              {
                Craft.Blueprint blueprint = new Craft.Blueprint(baseItemType);

                if (!Craft.Collect.System.blueprintDictionnary.ContainsKey(baseItemType))
                  Craft.Collect.System.blueprintDictionnary.Add(baseItemType, blueprint);

                uint oBlueprint = NWScript.CreateItemOnObject("blueprintgeneric", shop, 1, "blueprint");
                NWScript.SetName(oBlueprint, $"Patron original : {blueprint.name}");
                NWScript.SetLocalInt(oBlueprint, "_BASE_ITEM_TYPE", baseItemType);
                ItemPlugin.SetBaseGoldPieceValue(oBlueprint, blueprint.goldCost * 10);
              }

              foreach (Feat feat in SkillSystem.leatherBasicSkillBooks)
              {
                uint skillBook = NWScript.CreateItemOnObject("skillbookgeneriq", shop, 1, "skillbook");
                ItemPlugin.SetItemAppearance(skillBook, NWScript.ITEM_APPR_TYPE_SIMPLE_MODEL, 2, Utils.random.Next(0, 50));
                NWScript.SetLocalInt(skillBook, "_SKILL_ID", (int)feat);

                int value;
                if (int.TryParse(NWScript.Get2DAString("feat", "FEAT", (int)feat), out value))
                  NWScript.SetName(skillBook, NWScript.GetStringByStrRef(value));

                if (int.TryParse(NWScript.Get2DAString("feat", "DESCRIPTION", (int)feat), out value))
                  NWScript.SetDescription(skillBook, NWScript.GetStringByStrRef(value));

                if (int.TryParse(NWScript.Get2DAString("feat", "CRValue", (int)feat), out value))
                  ItemPlugin.SetBaseGoldPieceValue(skillBook, value * 1000);
              }

              uint craftTool = NWScript.CreateItemOnObject("oreextractor", shop, 1, "oreextractor");
              ItemPlugin.SetBaseGoldPieceValue(craftTool, 50);
              NWScript.SetLocalInt(craftTool, "_DURABILITY", 10);

              craftTool = NWScript.CreateItemOnObject("forgehammer", shop, 1, "forgehammer");
              ItemPlugin.SetBaseGoldPieceValue(craftTool, 50);
              NWScript.SetLocalInt(craftTool, "_DURABILITY", 5);
            }

            NWScript.OpenStore(shop, player.oid);
            break;
          case "le_bibliothecaire":
            DateTime previousSpawnDate;
            if (!DateTime.TryParse(NWScript.GetLocalString(NWScript.GetArea(oidSelf), "_DATE_LAST_TRIGGERED"), out previousSpawnDate) || (DateTime.Now - previousSpawnDate).TotalHours > 4)
            {
              NWScript.SetLocalString(NWScript.GetArea(oidSelf), "_DATE_LAST_TRIGGERED", DateTime.Now.ToString());

              shop = NWScript.GetNearestObjectByTag("bibliothecaire_shop", oidSelf);

              if (!Convert.ToBoolean(NWScript.GetIsObjectValid(shop)))
              {
                shop = NWScript.CreateObject(NWScript.OBJECT_TYPE_STORE, "generic_shop_res", NWScript.GetLocation(oidSelf), 0, "bibliothecaire_shop");
                NWScript.SetLocalObject(shop, "_STORE_NPC", oidSelf);
              }
              if (Utils.random.Next(1, 101) < 21)
              {
                int feat = (int)SkillSystem.languageSkillBooks[Utils.random.Next(0, SkillSystem.languageSkillBooks.Length)];
                uint skillBook = NWScript.CreateItemOnObject("skillbookgeneriq", shop, 1, "skillbook");
                ItemPlugin.SetItemAppearance(skillBook, NWScript.ITEM_APPR_TYPE_SIMPLE_MODEL, 2, Utils.random.Next(0, 50));
                NWScript.SetLocalInt(skillBook, "_SKILL_ID", feat);

                int value;
                if (int.TryParse(NWScript.Get2DAString("feat", "FEAT", feat), out value))
                  NWScript.SetName(skillBook, NWScript.GetStringByStrRef(value));

                if (int.TryParse(NWScript.Get2DAString("feat", "DESCRIPTION", feat), out value))
                  NWScript.SetDescription(skillBook, NWScript.GetStringByStrRef(value));

                ItemPlugin.SetBaseGoldPieceValue(skillBook, 3000);
              }
              else
              {
                uint skillBook = NWScript.CreateItemOnObject("skillbookgeneriq", shop, 1, "ruined_book");
                ItemPlugin.SetItemAppearance(skillBook, NWScript.ITEM_APPR_TYPE_SIMPLE_MODEL, 2, Utils.random.Next(0, 50));
                NWScript.SetName(skillBook, "Ouvrage ruiné");
                NWScript.SetDescription(skillBook, "Cet ouvrage est abîmé au-delà de toute rédemption. Il est même trop humide pour faire du feu.");
                ItemPlugin.SetBaseGoldPieceValue(skillBook, 3000);
              }

              NWScript.OpenStore(shop, player.oid);
            } 
            break;
          case "tribunal_hotesse":
            shop = NWScript.GetNearestObjectByTag("magic_shop", oidSelf);

            if (!Convert.ToBoolean(NWScript.GetIsObjectValid(shop)))
            {
              shop = NWScript.CreateObject(NWScript.OBJECT_TYPE_STORE, "generic_shop_res", NWScript.GetLocation(oidSelf), 0, "magic_shop");
              NWScript.SetLocalObject(shop, "_STORE_NPC", oidSelf);
              foreach (int itemPropertyId in SkillSystem.shopBasicMagicScrolls)
              {
                uint oScroll = NWScript.CreateItemOnObject("spellscroll", shop, 1, "scroll");
                int spellId = int.Parse(NWScript.Get2DAString("iprp_spells", "SpellIndex", itemPropertyId));
                NWScript.SetName(oScroll, $"{NWScript.GetStringByStrRef(int.Parse(NWScript.Get2DAString("spells", "Name", spellId)))}");
                NWScript.SetDescription(oScroll, $"{NWScript.GetStringByStrRef(int.Parse(NWScript.Get2DAString("spells", "SpellDesc", spellId)))}");
                //ItemPlugin.SetBaseGoldPieceValue(oScroll, int.Parse(NWScript.Get2DAString("spells", "Innate", spellId)) * 300 + 100);

                NWScript.AddItemProperty(NWScript.DURATION_TYPE_PERMANENT, NWScript.ItemPropertyCastSpell(itemPropertyId, 1), oScroll);
              }
            }

            NWScript.OpenStore(shop, player.oid);
            ChatPlugin.SendMessage(ChatPlugin.NWNX_CHAT_CHANNEL_PLAYER_TALK, "Pour obtenir votre amulette de concentration de l'arcane, il vous faut vous enregistrer auprès du juge.", oidSelf, player.oid);
            break;
        }
      }
        
      return 0;
    }
    private static int HandleIntroStart(uint oidSelf)
    {
      Player player;
      if (Players.TryGetValue(NWScript.GetLastSpeaker(), out player))
      {
        Area area; 
        if (AreaSystem.areaDictionnary.TryGetValue(NWScript.GetObjectUUID(NWScript.GetArea(player.oid)), out area))
          AreaSystem.StartEntryScene(area, player);
      }

      return 0;
    }
    private static int HandleIntroValidation(uint oidSelf)
    {
      uint oPC = NWScript.GetLastSpeaker();

      if (ObjectPlugin.GetInt(oPC, "_STARTING_SKILL_POINTS") > 0)
      {
        NWScript.SendMessageToPC(oPC, $"Il vous reste encore {ObjectPlugin.GetInt(oPC, "_STARTING_SKILL_POINTS")} points de compétences à dépenser auprès du reflet avant de pouvoir débarquer !");
        return 0;
      }
      else
      {
        NWScript.SetLocalInt(oidSelf, "_GO", 1);
        return 1;
      }
    }
  }
}
