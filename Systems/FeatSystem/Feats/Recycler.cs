﻿using NWN.API;
using NWN.API.Constants;
using NWN.Core;

namespace NWN.Systems
{
  class Recycler
  {
    public Recycler(NwPlayer oPC, NwGameObject oTarget)
    {
      if (!PlayerSystem.Players.TryGetValue(oPC, out PlayerSystem.Player player))
        return;

      if (!(oTarget is NwItem))
      {
        oPC.SendServerMessage($"{oTarget.Name.ColorString(Color.WHITE)} n'est pas un objet et ne peut donc pas être recyclé.", Color.RED);
        return;
      }

      NwItem item = (NwItem)oTarget;
      string material;
      
      switch (item.BaseItemType)
      {
        case BaseItemType.Armor:
        case BaseItemType.Helmet:
        case BaseItemType.TowerShield:
        case BaseItemType.LargeShield:
        case BaseItemType.Doubleaxe:
        case BaseItemType.Greataxe:
        case BaseItemType.Greatsword:
        case BaseItemType.Halberd:
        case BaseItemType.Handaxe:
        case BaseItemType.Scythe:
        case BaseItemType.TwoBladedSword:
        case BaseItemType.DireMace:
        case BaseItemType.Trident:
        case BaseItemType.ShortSpear:
        case BaseItemType.Bastardsword:
        case BaseItemType.Longsword:
        case BaseItemType.Battleaxe:
        case BaseItemType.Dagger:
        case BaseItemType.DwarvenWaraxe:
        case BaseItemType.Kama:
        case BaseItemType.Katana:
        case BaseItemType.Kukri:
        case BaseItemType.HeavyFlail:
        case BaseItemType.LightHammer:
        case BaseItemType.LightMace:
        case BaseItemType.Morningstar:
        case BaseItemType.Rapier:
        case BaseItemType.Shortsword:
        case BaseItemType.Scimitar:
        case BaseItemType.Sickle:
        case BaseItemType.Warhammer:
        case (BaseItemType)114:
        case (BaseItemType)115:

          if (item.GetLocalVariable<string>("_ITEM_MATERIAL").HasValue)
            material = item.GetLocalVariable<string>("_ITEM_MATERIAL").Value;
          else
            material = "Tritanium";

          player.craftJob.Start(Craft.Job.JobType.Recycling, null, player, NWScript.OBJECT_INVALID, item, material);
          break;
        
        case BaseItemType.HeavyCrossbow:
        case BaseItemType.LightCrossbow:
        case BaseItemType.Shortbow:
        case BaseItemType.Longbow:
        case BaseItemType.Dart:
        case BaseItemType.Sling:
        case BaseItemType.ThrowingAxe:
        case BaseItemType.Arrow:
        case BaseItemType.Bolt:
        case BaseItemType.Bullet:
        case BaseItemType.Quarterstaff:
        case BaseItemType.SmallShield:
        case BaseItemType.Club:

          if (item.GetLocalVariable<string>("_ITEM_MATERIAL").HasValue)
            material = item.GetLocalVariable<string>("_ITEM_MATERIAL").Value;
          else
            material = "Laurelinade";

          player.craftJob.Start(Craft.Job.JobType.Recycling, null, player, NWScript.OBJECT_INVALID, item, material);
          break;

        case BaseItemType.Belt:
        case BaseItemType.Boots:
        case BaseItemType.Bracer:
        case BaseItemType.Cloak:
        case BaseItemType.Gloves:
        case BaseItemType.Whip:

          if (item.GetLocalVariable<string>("_ITEM_MATERIAL").HasValue)
            material = item.GetLocalVariable<string>("_ITEM_MATERIAL").Value;
          else
            material = "MauvaisCuir";

          player.craftJob.Start(Craft.Job.JobType.Recycling, null, player, NWScript.OBJECT_INVALID, item, material);
          break;

        default:
          oPC.SendServerMessage($"{oTarget.Name.ColorString(Color.WHITE)} n'appartient pas à une catégorie d'objet qui puisse être recyclé.", Color.ORANGE);
          break;
      }
    }
  }
}
