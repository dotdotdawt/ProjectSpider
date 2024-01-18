using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public new string name;
    public float cooldownTime;
    public float activeTime;
    public float manaToActivate;

    public Projectile projectilePrefab;
    public Unit summonedUnitPrefab;
    public GameObject VFXSelfBuffPrefab;
    public GameObject VFXSpecialEffect;

    public virtual void Activate() { }


}
