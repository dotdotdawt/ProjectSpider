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
    public enum AbilType
    {
        Passive,
        Entrance, 
        OnHit,
        Spell,
        Ultimate
    }
    public enum BuffTarget
    {
        Self,
        Target
    }
    [Header("Core")]
    public new string name;
    public SpellType spellType;
    public AbilType abilType;
    public float cooldownTime;
    public float activeTime;
    public float manaToActivate;

    public int chanceToHit;
    public int damageOnHit;

    [Header("Object References")]
    public Unit summonedUnitPrefab;

    [Header("Damage & Basic")]
    public DamageType damageType;
    public int spellDamage;
    public float duration;
    public float bonusDurationRatio; // Mind
    public float dmgBoostRatio;
    public float bonusDamageMightRatio;
    public float bonusDamageMindRatio;
    public float chanceToHitBonusRatio; // Sense

    [Header("Buff Impact")]
    public BuffTarget buffTarget;
    public Stat impactStat;
    public int impactAmt;
    public float impactRatio;
    public float impactDamage;
    public float bonusImpactMightRatio;
    public float bonusImpactMindRatio;
    // Deprecated? // need to redo this with clear head
    public int targetBuffAmount;
    public float targetBuffDuration;
    public bool isDebuff;

    [Header("Additional Buffs")]
    public BuffTarget buffTargetA;
    public Buff.Stat buffStatA;
    public int buffAmountA;
    public float buffDurationA;
    public bool isDebuffA;
    public BuffTarget buffTargetB;
    public Buff.Stat buffStatB;
    public int buffAmountB;
    public float buffDurationB;
    public Buff.Stat buffStatC;
    public int buffAmountC;
    public float buffDurationC;

    [Header("VFX & Animations")]
    public Projectile projectilePrefab;
    public GameObject VFXSelfBuffPrefab;
    public GameObject VFXSpecialEffect;
    public GameObject targetAnimationPrefab;

    public virtual void Activate() { }


}
