using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Stat
{
    None,
    Might,
    Mind,
    Agility,
    Fortitude,
    Spirit,
    Vitality,
    Sense,
    Speed,
    MovementSpeed,
    AttackDamage,
    Armor,
    MagicResist,
    AttackSpeed,
    Health,
    Mana,
}

[CreateAssetMenu]
public class Ability : ScriptableObject
{
    public enum SpellType
    {
        Target,
        AOE,
        Self,
        Allies,
        Enemies,
        Aura,
        Buff
    }
    public enum EffectType
    {
        None,
        Self,
        DOT
    }
    public enum DamageType
    {
        Magical,
        Physical,
        True
    }

    public new string name;
    public float cooldownTime;
    public float activeTime;
    public float manaToActivate;
    public int level;
    public int totalLevels = 3;

    public int chanceToHit;
    public int damageOnHit;

    public Projectile projectilePrefab;
    public Unit summonedUnitPrefab;
    public GameObject VFXSelfBuffPrefab;
    public GameObject VFXSpecialEffect;
    public GameObject targetAnimationPrefab;

    public DamageType damageType;
    public SpellType spellType;
    public float dmgBoostRatio;
    public int spellDamage;

    public int impactAmt;
    public float impactRatio;
    public float impactDamage;
    public Stat impactStat;
    public float duration;

    //public float[] chanceToHitBonusRatio; // Sense
    //public float[] bonusDamageMightRatio;
    //public float[] bonusDamageMindRatio;
    //public float[] bonusImpactMightRatio;
    //public float[] bonusImpactMindRatio;
    //public float[] bonusDurationRatio; // Mind

    public float chanceToHitBonusRatio; // Sense
    public float bonusDamageMightRatio;
    public float bonusDamageMindRatio;
    public float bonusImpactMightRatio;
    public float bonusImpactMindRatio;
    public float bonusDurationRatio; // Mind

    public Buff.Stat selfBuffStat;
    ///public int[] selfBuffAmount;
    //public float[] selfBuffDuration;
    public Buff.Stat targetBuffStat;
    //public int[] targetBuffAmount;
    //public float[] targetBuffDuration;
    public int selfBuffAmount;
    public float selfBuffDuration;
    public int targetBuffAmount;
    public float targetBuffDuration;

    public Buff.Stat selfBuffStatSecondary;
    public Buff.Stat selfBuffStatTertiary;

    //public int[] selfBuffAmountSecondary;
    //public int[] selfBuffAmountTertiary;

    //public float[] selfBuffDurationSecondary;
    //public float[] selfBuffDurationTertiary;

    public int selfBuffAmountSecondary;
    public int selfBuffAmountTertiary;

    public float selfBuffDurationSecondary;
    public float selfBuffDurationTertiary;

    public Buff.Stat targetBuffStatSecondary;
    public Buff.Stat targetBuffStatTertiary;
    
    public int targetBuffAmountSecondary;
    public int targetBuffAmountTertiary;
    
    public float targetBuffDurationSecondary;
    public float targetBuffDurationTertiary;

    public virtual void Activate() { }


}
