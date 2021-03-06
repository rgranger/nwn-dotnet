﻿using NWN.API;
using NWN.API.Constants;
using NWN.Core.NWNX;
using NWN.Services;

namespace NWN.Systems
{
  [ServiceBinding(typeof(AttackSystem))]
  public class AttackSystem
  {
    [ScriptHandler("on_attack")]
    private void HandleAttackEvent(CallInfo callInfo)
    {
      AttackEventData attackData = DamagePlugin.GetAttackEventData();

      if (PlayerSystem.Players.TryGetValue(attackData.oTarget, out PlayerSystem.Player player))
      {
        // La cible de l'attaque est un joueur. Si l'attaque échoue, c'est qu'un objet d'armure a été utilisé et on fait diminuer la durabilité
        NwItem item;

        switch (attackData.iAttackResult)
        {
          // Attaque parée, c'est donc l'arme, ou le gantelet qui prend
          case 2:
            item = player.oid.GetItemInSlot(InventorySlot.RightHand);
            if (item == null)
              item = player.oid.GetItemInSlot(InventorySlot.LeftHand);
            if(item == null)
              item = player.oid.GetItemInSlot(InventorySlot.Arms);
            if (item == null)
              return;

           if(NwRandom.Roll(Utils.random, 100, 1) < 40) // diminuer le pourcentage en fonction des compétences
            {
              item.GetLocalVariable<int>("_DURABILITY").Value -= 1;
              if (item.GetLocalVariable<int>("_DURABILITY").Value <= 0)
              {
                item.Destroy();
                player.oid.SendServerMessage($"Il ne reste plus que des ruines irrécuprables de votre {item.Name.ColorString(Color.WHITE)}.", Color.RED);
              }
            }
            
            break;
          // Attaque résistée, c'est donc l'armure, le casque ou le bouclier qui prennent
          case 5:

            InventorySlot[] localizedDamaged = new InventorySlot[] { InventorySlot.Head, InventorySlot.Chest, InventorySlot.LeftHand };

            item = player.oid.GetItemInSlot(localizedDamaged[NwRandom.Roll(Utils.random, 3, 1)]);
            if (item == null)
              item = player.oid.GetItemInSlot(localizedDamaged[NwRandom.Roll(Utils.random, 3, 1)]);
            if (item == null)
              item = player.oid.GetItemInSlot(localizedDamaged[NwRandom.Roll(Utils.random, 3, 1)]);
            if (item == null)
              return;

            if (NwRandom.Roll(Utils.random, 100, 1) < 40) // diminuer le pourcentage en fonction des compétences
            {
              item.GetLocalVariable<int>("_DURABILITY").Value -= 1;
              if (item.GetLocalVariable<int>("_DURABILITY").Value <= 0)
              {
                item.Destroy();
                player.oid.SendServerMessage($"Il ne reste plus que des ruines irrécuprables de votre {item.Name.ColorString(Color.WHITE)}.", Color.RED);
              }
            }

            break;
          // Attaque échouée classique, donc on détermine aléatoirement la cible qui perd de la durabilité
          // La CA d'esquive (dextérité) permet de diminuer le risque d'usure
          case 4:

            int random = NwRandom.Roll(Utils.random, 11, 1) - 1;
            int loop = -1;
            item = player.oid.GetItemInSlot((InventorySlot)random);

            while (item == null || loop != random)
            {
              loop++;

              if (loop > 10)
                loop = 0;
              
              item = player.oid.GetItemInSlot((InventorySlot)loop);
            }

            if (item == null || item.Tag == "amulettorillink")
              return;

            if (NwRandom.Roll(Utils.random, 100, 1) < 40 - player.oid.GetAbilityModifier(Ability.Dexterity)) // diminuer le pourcentage en fonction des compétences
            {
              item.GetLocalVariable<int>("_DURABILITY").Value -= 1;
              if (item.GetLocalVariable<int>("_DURABILITY").Value <= 0)
              {
                item.Destroy();
                player.oid.SendServerMessage($"Il ne reste plus que des ruines irrécuprables de votre {item.Name.ColorString(Color.WHITE)}.", Color.RED);
              }
            }

            break;
        }

        if (PlayerSystem.Players.TryGetValue(callInfo.ObjectSelf, out PlayerSystem.Player attacker))
        {
          // L'attaquant est un joueur. On diminue la durabilité de son arme

          NwItem weapon;

          switch(attackData.iAttackType)
          {
            case 1:
              weapon = attacker.oid.GetItemInSlot(InventorySlot.RightHand);
              break;
            case 2:
              weapon = attacker.oid.GetItemInSlot(InventorySlot.LeftHand);
              break;
              default:
              return;
          }

          if (weapon == null)
            return;

          int durabilityChance;

          switch (attackData.iAttackResult)
          {
            case 1:
            case 2:
            case 5:
              durabilityChance = 60;
              break;
            case 4:
              durabilityChance = 40;
              break;
            default:
              return;
          }

          if (NwRandom.Roll(Utils.random, 100, 1) < durabilityChance) // diminuer le pourcentage en fonction des compétences
          {
            weapon.GetLocalVariable<int>("_DURABILITY").Value -= 1;
            if (weapon.GetLocalVariable<int>("_DURABILITY").Value <= 0)
            {
              weapon.Destroy();
              player.oid.SendServerMessage($"Il ne reste plus que des ruines irrécupérables de votre {weapon.Name.ColorString(Color.WHITE)}.", Color.RED);
            }
          }
        }
      }
    }
  }
}
