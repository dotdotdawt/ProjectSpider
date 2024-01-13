using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Ability : ScriptableObject
{
    public new string name;
    public float cooldownTime;
    public float activeTime;
    public float manaToActivate;

    public Projectile projectilePrefab;
    public Unit summonedUnitPrefab;

    public virtual void Activate() { }
}
