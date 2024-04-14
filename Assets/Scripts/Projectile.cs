using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




//[CreateAssetMenu]
public class Projectile : MonoBehaviour
{
    public Unit target;
    public Unit attacker;
    public Rigidbody2D rb;
    public float projectileSpeed;
    public bool isHoming;
    public bool hitTarget = false;
    public bool isSpell = false;
    public Ability ability;

    public bool isSpinning;
    public float degreesPerSecond = 200f;
    public float zOffset = 0f;

    //public Collider2D collider;

    public void SetTarget(Unit newTarget)
    {
        //Debug.Log("Setting target: " + newTarget.name);
        target = newTarget;
    }

    void Awake()
    {
        Destroy(gameObject, 2f);
    }

    public void SetAttacker(Unit newAttacker)
    {
        attacker = newAttacker;
    }

    public void SetAbilityOrigin(Ability newAbility)
    {
        ability = newAbility;
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Unit unit = hitInfo.GetComponent<Unit>();
        if (unit == null) return;
        if (isSpell) // spell // 
        {
            if (unit == target)
            {
                //attacker.DealMagicDamage();
                attacker.HandleSpell(ability);
                //attacker.ApplySpellEffect(ability);
                hitTarget = true;
                Destroy(gameObject);
            }          
        }
        else // attack
        {
            if (unit == target)
            {
                attacker.Hit();
                hitTarget = true;
                Destroy(gameObject);
            }  
        }
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            //target = FindObjectOfType<GameManager>().GetClosestTarget(attacker);
            //target = attacker.target;
        }
        if (target != null)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, target.transform.position, projectileSpeed * Time.deltaTime);
        }
        if (isSpinning)
        {
            transform.Rotate(new Vector3(0, 0, degreesPerSecond) * Time.deltaTime);
        }
        if (!isSpinning)
        {
            Vector3 targ = target.transform.position;
            targ.z = 0f;

            Vector3 objectPos = transform.position;
            targ.x = targ.x - objectPos.x;
            targ.y = targ.y - objectPos.y;

            float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + zOffset));
        }

    }
}

