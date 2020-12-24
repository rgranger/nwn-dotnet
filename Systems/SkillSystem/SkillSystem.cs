﻿using System;
using System.Collections.Generic;
using NWN.Core;
using NWN.Core.NWNX;

namespace NWN.Systems
{
  public static partial class SkillSystem
  {
    public enum SkillType
    {
      Invalid = 0,
      Skill = 1,
      Spell = 2,
    }
    public static Dictionary<int, Func<PlayerSystem.Player, int, int>> RegisterAddCustomFeatEffect = new Dictionary<int, Func<PlayerSystem.Player, int, int>>
    {
            { 1285, HandleHealthPoints },
            { 1286, HandleHealthPoints },
            { 1287, HandleHealthPoints },
            { 1288, HandleHealthPoints },
            { 1289, HandleHealthPoints },
    };

    public static Dictionary<int, Func<PlayerSystem.Player, int, int>> RegisterRemoveCustomFeatEffect = new Dictionary<int, Func<PlayerSystem.Player, int, int>>
    {
            { 1130, HandleRemoveStrengthMalusFeat },
    };

    private static int HandleHealthPoints(PlayerSystem.Player player, int feat)
    {
      CreaturePlugin.SetMaxHitPointsByLevel(player.oid, 1, CreaturePlugin.GetMaxHitPointsByLevel(player.oid, 1) 
        + 1 + (3 * ((CreaturePlugin.GetRawAbilityScore(player.oid, NWScript.ABILITY_CONSTITUTION) - 10) / 2) 
        + CreaturePlugin.GetKnowsFeat(player.oid, (int)Feat.Toughness)));
      return 0;
    }

    private static int HandleRemoveStrengthMalusFeat(PlayerSystem.Player player, int idMalusFeat)
    {
      player.removeableMalus.Remove(idMalusFeat);
      CreaturePlugin.SetRawAbilityScore(player.oid, NWScript.ABILITY_STRENGTH, CreaturePlugin.GetRawAbilityScore(player.oid, NWScript.ABILITY_STRENGTH) + 2);

      return 0;
    }

    public static Feat[] forgeBasicSkillBooks = new Feat[] { Feat.CraftOreExtractor, Feat.CraftForgeHammer, Feat.Metallurgy, Feat.Research, Feat.Miner, Feat.Prospection, Feat.StripMiner, Feat.Reprocessing, Feat.Forge, Feat.CraftScaleMail, Feat.CraftDagger, Feat.CraftLightMace, Feat.CraftMorningStar, Feat.CraftSickle, Feat.CraftShortSpear };
    public static Feat[] craftSkillBooks = new Feat[] { Feat.Metallurgy, Feat.AdvancedCraft, Feat.Miner, Feat.Geology, Feat.Prospection, Feat.VeldsparReprocessing, Feat.ScorditeReprocessing, Feat.PyroxeresReprocessing, Feat.StripMiner, Feat.Reprocessing, Feat.ReprocessingEfficiency, Feat.Connections, Feat.Forge };
    public static Feat[] languageSkillBooks = new Feat[] { Feat.LanguageAbyssal, Feat.LanguageCelestial, Feat.LanguageDeep, Feat.LanguageDraconic, Feat.LanguageDruidic, Feat.LanguageDwarf, Feat.LanguageElf, Feat.LanguageGiant, Feat.LanguageGoblin, Feat.LanguageHalfling, Feat.LanguageInfernal, Feat.LanguageOrc, Feat.LanguagePrimodial, Feat.LanguageSylvan, Feat.LanguageThieves, Feat.LanguageGnome };
    
    public static Feat[] lowSkillBooks = new Feat[] { Feat.CraftOreExtractor, Feat.CraftForgeHammer, Feat.CraftLance, Feat.Forge, Feat.Reprocessing, Feat.BlueprintCopy, Feat.Research, Feat.Miner, Feat.Metallurgy, Feat.DeneirsEye, Feat.DirtyFighting, Feat.ResistDisease, Feat.Stealthy, Feat.SkillFocusAnimalEmpathy, Feat.SkillFocusBluff, Feat.SkillFocusConcentration, Feat.SkillFocusDisableTrap, Feat.SkillFocusDiscipline, Feat.SkillFocusHeal, Feat.SkillFocusHide, Feat.SkillFocusIntimidate, Feat.SkillFocusListen, Feat.SkillFocusLore, Feat.SkillFocusMoveSilently, Feat.SkillFocusOpenLock, Feat.SkillFocusParry, Feat.SkillFocusPerform, Feat.SkillFocusPickPocket, Feat.SkillFocusSearch, Feat.SkillFocusSetTrap, Feat.SkillFocusSpellcraft, Feat.SkillFocusSpot, Feat.SkillFocusTaunt, Feat.SkillFocusTumble, Feat.SkillFocusUseMagicDevice, Feat.PointBlankShot, Feat.IronWill, Feat.Alertness, Feat.CombatCasting, Feat.Dodge, Feat.ExtraTurning, Feat.GreatFortitude  };
    public static Feat[] mediumSkillBooks = new Feat[] { Feat.CraftTorch, Feat.CraftStuddedLeather, Feat.CraftSling, Feat.CraftSmallShield, Feat.CraftSickle, Feat.CraftShortSpear, Feat.CraftRing, Feat.CraftPaddedArmor , Feat.CraftPotion, Feat.CraftQuarterstaff, Feat.CraftMorningStar, Feat.CraftMagicWand, Feat.CraftLightMace, Feat.CraftLightHammer, Feat.CraftLightFlail, Feat.CraftLightCrossbow, Feat.CraftLeatherArmor, Feat.CraftBullets, Feat.CraftCloak, Feat.CraftClothing, Feat.CraftClub, Feat.CraftDagger, Feat.CraftDarts, Feat.CraftGloves, Feat.CraftHeavyCrossbow, Feat.CraftHelmet, Feat.CraftAmulet, Feat.CraftArrow, Feat.CraftBelt, Feat.CraftBolt, Feat.CraftBoots, Feat.CraftBracer,  Feat.ReprocessingEfficiency, Feat.StripMiner, Feat.VeldsparReprocessing, Feat.ScorditeReprocessing, Feat.PyroxeresReprocessing, Feat.PlagioclaseReprocessing, Feat.Geology, Feat.Prospection, Feat.TymorasSmile, Feat.LliirasHeart, Feat.RapidReload, Feat.Expertise, Feat.ImprovedInitiative, Feat.DefensiveRoll, Feat.SneakAttack, Feat.FlurryOfBlows, Feat.WeaponSpecializationHeavyCrossbow, Feat.WeaponSpecializationDagger, Feat.WeaponSpecializationDart, Feat.WeaponSpecializationClub, Feat.StillSpell, Feat.TwoWeaponFighting, Feat.RapidShot, Feat.SilenceSpell, Feat.PowerAttack, Feat.Knockdown, Feat.LightningReflexes, Feat.ImprovedUnarmedStrike, Feat.Ambidexterity, Feat.Cleave, Feat.CalledShot, Feat.DeflectArrows, Feat.WeaponSpecializationLightCrossbow, Feat.WeaponSpecializationLightFlail, Feat.WeaponSpecializationLightMace, Feat.Disarm, Feat.EmpowerSpell, Feat.WeaponSpecializationMorningStar, Feat.ExtendSpell, Feat.SpellFocusAbjuration, Feat.SpellFocusConjuration, Feat.SpellFocusDivination, Feat.SpellFocusEnchantment, Feat.WeaponSpecializationSickle, Feat.WeaponSpecializationSling, Feat.WeaponSpecializationSpear, Feat.WeaponSpecializationStaff, Feat.WeaponSpecializationThrowingAxe, Feat.WeaponSpecializationTrident, Feat.WeaponSpecializationUnarmedStrike, Feat.SpellFocusEvocation, Feat.SpellFocusIllusion, Feat.SpellFocusNecromancy, Feat.SpellFocusTransmutation, Feat.SpellPenetration };
    public static Feat[] highSkillBooks = new Feat[] { Feat.CraftWarHammer, Feat.CraftTrident, Feat.CraftThrowingAxe, Feat.CraftStaff, Feat.CraftSplintMail, Feat.CraftSpellScroll, Feat.CraftShortsword, Feat.CraftShortBow, Feat.CraftScimitar, Feat.CraftScaleMail, Feat.CraftRapier, Feat.CraftMagicRod, Feat.CraftLongsword, Feat.CraftLongBow, Feat.CraftLargeShield , Feat.CraftBattleAxe, Feat.OmberReprocessing, Feat.KerniteReprocessing, Feat.GneissReprocessing, Feat.CraftHalberd, Feat.JaspetReprocessing, Feat.CraftHeavyFlail, Feat.CraftHandAxe, Feat.HemorphiteReprocessing, Feat.CraftGreatAxe, Feat.CraftGreatSword, Feat.ArcaneDefenseAbjuration, Feat.ArcaneDefenseConjuration, Feat.ArcaneDefenseDivination, Feat.ArcaneDefenseEnchantment, Feat.ArcaneDefenseEvocation, Feat.ArcaneDefenseIllusion, Feat.ArcaneDefenseNecromancy, Feat.ArcaneDefenseTransmutation, Feat.BlindFight, Feat.SpringAttack, Feat.GreatCleave, Feat.ImprovedExpertise, Feat.SkillMastery, Feat.Opportunist, Feat.Evasion, Feat.WeaponSpecializationDireMace, Feat.WeaponSpecializationDoubleAxe, Feat.WeaponSpecializationDwaxe, Feat.WeaponSpecializationGreatAxe, Feat.WeaponSpecializationGreatSword, Feat.WeaponSpecializationHalberd, Feat.WeaponSpecializationHandAxe, Feat.WeaponSpecializationHeavyFlail, Feat.WeaponSpecializationKama, Feat.WeaponSpecializationKatana, Feat.WeaponSpecializationKukri,  Feat.WeaponSpecializationBastardSword, Feat.WeaponSpecializationLightHammer, Feat.WeaponSpecializationLongbow, Feat.WeaponSpecializationLongSword, Feat.WeaponSpecializationRapier, Feat.WeaponSpecializationScimitar, Feat.WeaponSpecializationScythe, Feat.WeaponSpecializationShortbow, Feat.WeaponSpecializationShortSword, Feat.WeaponSpecializationShuriken, Feat.WeaponSpecializationBattleAxe, Feat.QuickenSpell, Feat.MaximizeSpell, Feat.ImprovedTwoWeaponFighting, Feat.ImprovedPowerAttack, Feat.WeaponSpecializationTwoBladedSword, Feat.WeaponSpecializationWarHammer, Feat.WeaponSpecializationWhip, Feat.ImprovedDisarm, Feat.ImprovedKnockdown, Feat.ImprovedParry, Feat.ImprovedCriticalBastardSword, Feat.ImprovedCriticalBattleAxe, Feat.ImprovedCriticalClub, Feat.ImprovedCriticalDagger, Feat.ImprovedCriticalDart, Feat.ImprovedCriticalDireMace, Feat.ImprovedCriticalDoubleAxe, Feat.ImprovedCriticalDwaxe, Feat.ImprovedCriticalGreatAxe, Feat.ImprovedCriticalGreatSword, Feat.ImprovedCriticalHalberd, Feat.ImprovedCriticalHandAxe, Feat.ImprovedCriticalHeavyCrossbow, Feat.ImprovedCriticalHeavyFlail, Feat.ImprovedCriticalKama, Feat.ImprovedCriticalKatana, Feat.ImprovedCriticalKukri, Feat.ImprovedCriticalLightCrossbow, Feat.ImprovedCriticalLightFlail, Feat.ImprovedCriticalLightHammer, Feat.ImprovedCriticalLightMace, Feat.ImprovedCriticalLongbow, Feat.ImprovedCriticalLongSword, Feat.ImprovedCriticalMorningStar, Feat.ImprovedCriticalRapier, Feat.ImprovedCriticalScimitar, Feat.ImprovedCriticalScythe, Feat.ImprovedCriticalShortbow, Feat.ImprovedCriticalShortSword, Feat.ImprovedCriticalShuriken, Feat.ImprovedCriticalSickle, Feat.ImprovedCriticalSling, Feat.ImprovedCriticalSpear, Feat.ImprovedCriticalStaff, Feat.ImprovedCriticalThrowingAxe, Feat.ImprovedCriticalTrident, Feat.ImprovedCriticalTwoBladedSword, Feat.ImprovedCriticalUnarmedStrike, Feat.ImprovedCriticalWarHammer, Feat.ImprovedCriticalWhip };
    public static Feat[] epicSkillBooks = new Feat[] { Feat.CraftWhip, Feat.CraftTwoBladedSword, Feat.CraftTowerShield, Feat.CraftShuriken, Feat.CraftScythe, Feat.CraftKukri, Feat.CraftKatana, Feat.CraftBreastPlate, Feat.CraftDireMace, Feat.CraftDoubleAxe, Feat.CraftDwarvenWarAxe, Feat.CraftFullPlate, Feat.CraftHalfPlate, Feat.CraftBastardSword, Feat.CraftKama, Feat.DarkOchreReprocessing, Feat.CrokiteReprocessing, Feat.BistotReprocessing, Feat.ResistEnergyAcid, Feat.ResistEnergyCold, Feat.ResistEnergyElectrical, Feat.ResistEnergyFire, Feat.ResistEnergySonic, Feat.ZenArchery, Feat.CripplingStrike, Feat.SlipperyMind, Feat.GreaterSpellFocusAbjuration, Feat.GreaterSpellFocusConjuration, Feat.GreaterSpellFocusDivination, Feat.GreaterSpellFocusDiviniation, Feat.GreaterSpellFocusEnchantment, Feat.GreaterSpellFocusEvocation, Feat.GreaterSpellFocusIllusion, Feat.GreaterSpellFocusNecromancy, Feat.GreaterSpellFocusTransmutation, Feat.GreaterSpellPenetration };

    public static int[] shopBasicMagicScrolls = new int[] { NWScript.SPELL_ACID_SPLASH, NWScript.SPELL_DAZE, NWScript.SPELL_ELECTRIC_JOLT, NWScript.SPELL_FLARE, NWScript.SPELL_RAY_OF_FROST, NWScript.SPELL_RESISTANCE, NWScript.SPELL_BURNING_HANDS, NWScript.SPELL_CHARM_PERSON, NWScript.SPELL_COLOR_SPRAY, NWScript.SPELL_ENDURE_ELEMENTS, NWScript.SPELL_EXPEDITIOUS_RETREAT, NWScript.SPELL_GREASE, NWScript.SPELL_HORIZIKAULS_BOOM, NWScript.SPELL_ICE_DAGGER, NWScript.SPELL_IRONGUTS, NWScript.SPELL_MAGE_ARMOR, NWScript.SPELL_MAGIC_MISSILE, NWScript.SPELL_NEGATIVE_ENERGY_RAY, NWScript.SPELL_RAY_OF_ENFEEBLEMENT, NWScript.SPELL_SCARE, NWScript.SPELL_SHELGARNS_PERSISTENT_BLADE, NWScript.SPELL_SHIELD, NWScript.SPELL_SLEEP, NWScript.SPELL_SUMMON_CREATURE_I, NWScript.SPELL_TRUE_STRIKE, NWScript.SPELL_AMPLIFY, NWScript.SPELL_BALAGARNSIRONHORN, NWScript.SPELL_LESSER_DISPEL, NWScript.SPELL_CURE_MINOR_WOUNDS, NWScript.SPELL_INFLICT_MINOR_WOUNDS, NWScript.SPELL_VIRTUE, NWScript.SPELL_BANE, NWScript.SPELL_BLESS, NWScript.SPELL_CURE_LIGHT_WOUNDS, NWScript.SPELL_DIVINE_FAVOR, NWScript.SPELL_DOOM, NWScript.SPELL_ENTROPIC_SHIELD, NWScript.SPELL_INFLICT_LIGHT_WOUNDS, NWScript.SPELL_REMOVE_FEAR, NWScript.SPELL_SANCTUARY, NWScript.SPELL_SHIELD_OF_FAITH, NWScript.SPELL_CAMOFLAGE, NWScript.SPELL_ENTANGLE, NWScript.SPELL_MAGIC_FANG };
  }
}
