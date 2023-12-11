using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using UnityEngine.Events;

using DG.Tweening;
using System.Security.Cryptography;

public enum Team
{
    Player,
    Enemy
}

public class Unit : MonoBehaviour
{
    public event Action OnDeath;

    // Basic unit stats
    public float speed = 5f;
    public int max_hp = 400;
    public int current_hp = 0;
    public int ad = 25;
    public float range = 5f;
    public float attackSpeed = 0.9f;
    public float timeSinceLastAttack = Mathf.Infinity;

    // references
    public Rigidbody2D rb;
    public Unit target;
    public Team team;
    public HealthBar healthBar;
    public DamageTextSpawner damageTextSpawner;
    public Ability[] abilities;
    public bool initAbility;
    public bool battleStarted = false;

    // ?
    public bool hasOrders = false;
    public bool alive = false;
    public bool canAttack = true;

    // Projectile
    //public Rigidbody2D projectilePrefab;
    public Projectile projectilePrefab;
    //public List<Unit> units = new List<Unit>();

    // DOPunch stuff
    //public float punchDuration = 1f;
    public int vibrato;
    public float elasticity;
    public bool snapping;
    public float tamperAttackVector;
    public float tamperAS;
    public float rangedTamper = 0.35f;

    // Start is called before the first frame update
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageTextSpawner = GetComponent<DamageTextSpawner>();
        current_hp = max_hp;
        healthBar.SetMaxHealth(max_hp);
        this.alive = true;
        
    }

    public void DealDamage(int damage)
    {
        target.current_hp -= damage;
        target.healthBar.SetHealth(target.current_hp);
        if (damageTextSpawner != null)
        {
            damageTextSpawner.Spawn(damage);
        }
        
    }

    private bool InAttackRangeOfTarget()
    {
        if (target != null)
        {
            return Vector2.Distance(transform.position, target.transform.position) <= Math.Abs(range);
        } else
        {
            return false;
        }
        
    }

    private void PunchAnimation(bool isRanged)
    {
        if (isRanged) // setup so we can do a different tween for ranged
        {
            transform.DOPunchPosition(target.transform.position * tamperAttackVector - transform.position * tamperAttackVector * rangedTamper, 
                attackSpeed * tamperAS * rangedTamper, vibrato, elasticity * rangedTamper, snapping);
        }
        else
        {
            transform.DOPunchPosition(target.transform.position * tamperAttackVector - transform.position * tamperAttackVector, 
                attackSpeed * tamperAS, vibrato, elasticity, snapping);
        }
    }

    IEnumerator WebUp()
    {
        if (target != null)
        {
            canAttack = false;
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMoveY(7, 0.6f));
            sequence.Append(transform.DOMoveX(target.transform.position.x+2, 0.1f));
            sequence.Append(transform.DOMoveY(target.transform.position.y, 0.6f));
            yield return new WaitForSeconds(1.1f);
            GetComponentInChildren<SpriteRenderer>().flipX = !GetComponentInChildren<SpriteRenderer>().flipX;

            canAttack = true;
        }
    }
  
    void AttackBehavior()
    {
        if (canAttack)
        {
            if (!target.alive)
            {
                target = null;
                return;
            }
            if (target.team == this.team) return;

            if (range <= 1.5f)
            {
                DealDamage(ad);
                PunchAnimation(false);
            }
            if (range >= 1.6f)
            {
                RangedAttack();
                PunchAnimation(true);
            }

            timeSinceLastAttack = 0f;
        }
        
    }

    void RangedAttack()
    {
        if (target != null)
        {
            Projectile projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
            projectile.transform.position = transform.position;
            projectile.SetTarget(target);
            projectile.SetAttacker(this);
            timeSinceLastAttack = 0f;
        }
        
    }

    public void SetTarget(Unit newTarget)
    {
        hasOrders = true;
        target = newTarget;
    }

    public void Update()
    {
        if (battleStarted == true)
        {
            if (initAbility)
            {
                if (abilities[0] != null)
                {
                    if (abilities[0].name == "WebUp")
                    {
                        initAbility = false;
                        StartCoroutine(WebUp());
                    }
                }
            }
            if (alive == false)
            {
                Destroy(gameObject);
            }
            if (current_hp <= 0)
            {
                if (OnDeath != null)
                {
                    OnDeath();

                }
                gameObject.SetActive(false);
                this.alive = false;
            }

            timeSinceLastAttack += Time.deltaTime;

            if (InAttackRangeOfTarget() && timeSinceLastAttack >= attackSpeed)
            {
                AttackBehavior();
            }
        }
    }
    public void FixedUpdate()
    {
        if (!this.alive) return;
        if (target != null)
        {
            Vector2 direction = target.transform.position - transform.position;
            float distance = Vector2.Distance(transform.position, target.transform.position);

            if (!InAttackRangeOfTarget()) transform.position = Vector2.MoveTowards(this.transform.position, target.transform.position, speed * Time.deltaTime);
        }
        

    }
}
