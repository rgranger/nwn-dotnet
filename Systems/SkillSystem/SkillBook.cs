﻿using System;
using NWN.Enums;

namespace NWN.Systems
{
  static public class SkillBook
  {
    public class Context
    {
      public NWItem oItem { get; }
      public PlayerSystem.Player oActivator { get; }
      public int skillId { get; }

      public Context(NWItem oItem, PlayerSystem.Player oActivator, int SkillId)
      {
        this.oItem = oItem;
        this.oActivator = oActivator;
        this.skillId = SkillId;
      }
    }

    public static Pipeline<Context> pipeline = new Pipeline<Context>(
      new Action<Context, Action>[]
      {
        CheckRequiredStatsMiddleware,
        CheckRequiredFeatsMiddleware,
        CheckRequiredSkillsMiddleware,
        ValidationMiddleware,
      }
    );

    private static void CheckRequiredStatsMiddleware(Context ctx, Action next)
    {
      if (!CheckPlayerRequiredStat("MINATTACKBONUS", ctx.skillId, ctx.oActivator))
      {
        ctx.oActivator.SendMessage("Vous n'êtes pas assez expérimenté en maniement des armes pour retirer quoique ce soit de cet ouvrage");
        return;
      }

      if (!CheckPlayerRequiredStat("MINSTR", ctx.skillId, ctx.oActivator))
      {
        ctx.oActivator.SendMessage("Vous n'avez pas la force nécessaire pour retirer quoique ce soit de cet ouvrage");
        return;
      }

      if (!CheckPlayerRequiredStat("MINDEX", ctx.skillId, ctx.oActivator))
      {
        ctx.oActivator.SendMessage("Vous n'avez pas la dextérité nécessaire pour retirer quoique ce soit de cet ouvrage");
        return;
      }

      if (!CheckPlayerRequiredStat("MINCON", ctx.skillId, ctx.oActivator))
      {
        ctx.oActivator.SendMessage("Vous n'avez pas la constitution nécessaire pour retirer quoique ce soit de cet ouvrage");
        return;
      }

      if (!CheckPlayerRequiredStat("MININT", ctx.skillId, ctx.oActivator))
      {
        ctx.oActivator.SendMessage("Vous n'avez pas l'intelligence nécessaire pour retirer quoique ce soit de cet ouvrage");
        return;
      }

      if (!CheckPlayerRequiredStat("MINWIS", ctx.skillId, ctx.oActivator))
      {
        ctx.oActivator.SendMessage("Vous n'avez pas la sagesse nécessaire pour retirer quoique ce soit de cet ouvrage");
        return;
      }

      if (!CheckPlayerRequiredStat("MINCHA", ctx.skillId, ctx.oActivator))
      {
        ctx.oActivator.SendMessage("Vous n'avez pas le charisme nécessaire pour retirer quoique ce soit de cet ouvrage");
        return;
      }

      next();
    }

    private static void CheckRequiredFeatsMiddleware(Context ctx, Action next)
    {
      int result = CheckPlayerRequiredFeat("PREREQFEAT1", ctx.skillId, ctx.oActivator);
      if (result > -1)
      {
        ctx.oActivator.SendMessage($"Le don {NWScript.GetStringByStrRef(int.Parse(NWScript.Get2DAString("feat", "FEAT", result)))} est nécessaire avant de pouvoir retirer quoique ce soit de cet ouvrage");
        return;
      }

      result = CheckPlayerRequiredFeat("PREREQFEAT2", ctx.skillId, ctx.oActivator);
      if (result > -1)
      {
        ctx.oActivator.SendMessage($"Le don {NWScript.GetStringByStrRef(int.Parse(NWScript.Get2DAString("feat", "FEAT", result)))} est nécessaire avant de pouvoir retirer quoique ce soit de cet ouvrage");
        return;
      }

      if (CheckPlayerRequiredFeat("OrReqFeat0", ctx.skillId, ctx.oActivator) > -1 &&
          CheckPlayerRequiredFeat("OrReqFeat1", ctx.skillId, ctx.oActivator) > -1 &&
          CheckPlayerRequiredFeat("OrReqFeat2", ctx.skillId, ctx.oActivator) > -1 &&
          CheckPlayerRequiredFeat("OrReqFeat3", ctx.skillId, ctx.oActivator) > -1 &&
          CheckPlayerRequiredFeat("OrReqFeat4", ctx.skillId, ctx.oActivator) > -1)
      {

        ctx.oActivator.SendMessage($"Il vous manque un don avant de pouvoir retirer un réel savoir de cet ouvrage");
        return;
      }

      next();
    }
    private static void CheckRequiredSkillsMiddleware(Context ctx, Action next)
    {
      int result = CheckPlayerRequiredSkill("REQSKILL", "ReqSkillMinRanks", ctx.skillId, ctx.oActivator);
      if (result > -1)
      {
        ctx.oActivator.SendMessage($"Une maîtrise plus avancée de la compétence {NWScript.GetStringByStrRef(int.Parse(NWScript.Get2DAString("skills", "Name", result)))} est nécessaire avant de pouvoir retirer quoique ce soit de cet ouvrage");
        return;
      }

      result = CheckPlayerRequiredSkill("REQSKILL2", "ReqSkillMinRanks2", ctx.skillId, ctx.oActivator);
      if (result > -1)
      {
        ctx.oActivator.SendMessage($"Une maîtrise plus avancée de la compétence {NWScript.GetStringByStrRef(int.Parse(NWScript.Get2DAString("skills", "Name", result)))} est nécessaire avant de pouvoir retirer quoique ce soit de cet ouvrage");
        return;
      }

      if (int.TryParse(NWScript.Get2DAString("feat", "MinFortSave", ctx.skillId), out result))
      {
        if (NWScript.GetFortitudeSavingThrow(ctx.oActivator) < result)
        {
          ctx.oActivator.SendMessage($"Une vigueur minimale de {result} est nécessaire pour pouvoir retirer quoique ce soit de cet ouvrage");
          return;
        }
      }

      next();
    }

    private static void ValidationMiddleware(Context ctx, Action next)
    {
      ctx.oActivator.LearnableSkills.Add(ctx.skillId, new SkillSystem.Skill(ctx.skillId, 0));
      ctx.oItem.Destroy();

      next();
    }

    private static Boolean CheckPlayerRequiredStat(string Stat, int SkillId, PlayerSystem.Player player)
    {
      int value;
      if (int.TryParse(NWScript.Get2DAString("feat", Stat, SkillId), out value))
        if (value < NWScript.GetBaseAttackBonus(player))
          return true;

      return false;
    }

    private static int CheckPlayerRequiredFeat(string Feat, int SkillId, PlayerSystem.Player player)
    {
      int value;
      if (int.TryParse(NWScript.Get2DAString("feat", Feat, SkillId), out value))
        if (player.HasFeat((Feat)value))
          return -1;

      return value;
    }

    private static int CheckPlayerRequiredSkill(string Skill, string SkillRank, int SkillId, PlayerSystem.Player player)
    {
      int value;
      int SkillValueRequirement;
      if (int.TryParse(NWScript.Get2DAString("feat", Skill, SkillId), out value))
        if (int.TryParse(NWScript.Get2DAString("feat", SkillRank, SkillId), out SkillValueRequirement))
          if (SkillValueRequirement < player.GetSkillRank((Skill)value, true))
            return value;

      return -1;
    }
  }
}
