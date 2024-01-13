using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

using DG.Tweening;
using TMPro;

public enum Team
{
    Player,
    Enemy
}

public class Unit : MonoBehaviour
{
    // Public Actions
    public event Action OnDeath;
    public event Action OnCreate;

    [Header("Unit Stats")]
    public int level;
    public int maxHealth;
    public int currentHealth;
    public int might;
    public int mind;
    public int agility;
    public int vitality;
    public int fortitude;
    public int spirit;
    public int sense;
    public float speed = 5f; // ACTUAL move speed
    public int speedStat; // Need to write logic to clamp speed between 2f-8f
    public float range = 5f;
    public float castRange = 5f;
    public float mana = 0f;
    public int armor;
    public int magicResist;

    [Header("Constants AD, Armor and Agility Calculation")]
    public float baseAttackTime = 2f; // 2 seconds
    public float agilityAttackSpeedConstant = 2f; // 2%
    public float timeSinceLastAttack = Mathf.Infinity;
    public float timeSinceLastCast = Mathf.Infinity;
    public float attackSpeed = 0.9f; // deprecated
    public int baseAttackDamageConstant = 24; // base + might
    public int baseHealthConstant = 225;
    public int baseVitalityConstant = 25;
    public int baseArmorConstant = 5;
    public int baseMagicResistConstant = 5;
    public int baseSpellPowerConstant = 10;
    public float baseCastingTime = 1f;
    // Summoned units should have base health 50 and base damage 10

    [Header("Static Object References")]
    public Rigidbody2D rb;
    public HealthBar healthBar;
    public GameObject floatingTextPrefab;

    [Header("Object References")]
    public Unit target;
    public Team team;
    public Unit summonedUnitPrefab;
    public Projectile projectilePrefab;
    public Ability[] abilities;

    [Header("Boolean Toggles")]
    public bool initAbility;
    public bool battleStarted = false;
    public bool alive = false;
    public bool canAttack = true;
    public bool untargetable = false;

    [Header("DOTween Punch Settings")]
    public int vibrato;
    public float elasticity;
    public bool snapping;
    public float tamperAttackVector;
    public float tamperAS;
    public float rangedTamper;

    public void Awake()
    {
        FindObjectOfType<GameManager>().AddUnit(this);
    }

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // necessary?
        maxHealth = GetMaxHealth();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        this.alive = true;
    }

    public int GetMaxHealth()
    {
        return vitality * baseVitalityConstant + baseHealthConstant;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetCurrentMana()
    {
        return mana;
    }

    public void DealDamage()
    {
        int _rawDmg = GetArmorReducedDamage(GetBasicAttackDamage());
        if (floatingTextPrefab)
        {
            ShowFloatingText(_rawDmg);
        }
        target.currentHealth -= _rawDmg;
        target.healthBar.SetHealth(target.currentHealth);
    }

    public int GetBasicAttackDamage()
    {
        return baseAttackDamageConstant + might; // Check for alterations above
    }

    public int GetArmorReducedDamage(int rawDamage) // m is decimal
    {
        return Mathf.RoundToInt(System.Convert.ToSingle(100m / (GetArmorStat() + 100m)) * rawDamage);
    }

    public int GetMagicReducedDamage(int rawDamage) // m is decimal
    {
        return Mathf.RoundToInt(System.Convert.ToSingle(100m / (GetMagicResistStat() + 100m)) * rawDamage);
    }

    public int GetMindStat()
    {
        return mind + baseSpellPowerConstant;
    }

    public int GetArmorStat()
    {
        return fortitude + baseArmorConstant;
    }

    public int GetMagicResistStat()
    {
        return spirit + baseMagicResistConstant;
    }

    public float GetMovementSpeedStat() // needs expansion
    {
        return speed;
    }

    public float GetAttackSpeedStat()
    {
        return 1f / GetAttackSpeedTimer();
    }

    public void ShowFloatingText(int damage) // Lots of DOTween customization... if can be made shorter, do it.
    {
        Vector3 spawnLoc;
        double _rx = target.transform.position.x + (UnityEngine.Random.Range(-1, 1)*0.1);
        double _ry = target.transform.position.y + (UnityEngine.Random.Range(-1, 1)*0.1);
        spawnLoc.x = System.Convert.ToSingle(_rx);
        spawnLoc.y = System.Convert.ToSingle(_ry);
        spawnLoc.z = target.transform.position.z;
        var fText = Instantiate(floatingTextPrefab, spawnLoc, Quaternion.identity, target.transform);

        fText.GetComponent<TextMeshPro>().text = damage.ToString();
        fText.transform.DOMoveY((fText.transform.position.y+1), 1f).SetEase(Ease.InSine);

        var sequence = DOTween.Sequence();
        double _x = fText.transform.localScale.x * 2.5;
        double _y = fText.transform.localScale.y * 2.5;
        sequence.Append(fText.transform.DOScaleX(Math.Abs(System.Convert.ToSingle(_x)), 0.5f).SetEase(Ease.InOutSine));
        sequence.Join(fText.transform.DOScaleY(Math.Abs(System.Convert.ToSingle(_y)), 0.5f).SetEase(Ease.InOutSine));
        sequence.Append(fText.transform.DOScaleX(0, 0.5f).SetEase(Ease.InOutSine));
        sequence.Join(fText.transform.DOScaleY(0, 0.5f).SetEase(Ease.InOutSine));

    }

    private bool InAttackRangeOfTarget()
    {
        if (target == null)
        {
            return false;
        }
        return Vector2.Distance(transform.position, target.transform.position) <= Math.Abs(range);  
    }

    private bool InCastRangeOfTarget() // just defaulted to a value needs to get from ability
    {
        if (target == null)
        {
            return false;
        }
        return Vector2.Distance(transform.position, target.transform.position) <= Math.Abs(castRange);
    }

    private void PunchAnimation()
    {
        if (range >= 1.8f) // is ranged
        {
            transform.DOPunchPosition(target.transform.position * tamperAttackVector - transform.position * tamperAttackVector * rangedTamper, 
                attackSpeed * tamperAS * rangedTamper, vibrato, elasticity * rangedTamper, snapping);
        }
        else // is not ranged
        {
            transform.DOPunchPosition(target.transform.position * tamperAttackVector - transform.position * tamperAttackVector, 
                attackSpeed * tamperAS, vibrato, elasticity, snapping);
        }
    }

    public float GetAttackSpeedTimer()
    {
        int bonuses = 0;
        float AS = baseAttackTime / ((agility * agilityAttackSpeedConstant + bonuses) / 100 + 1);
        return AS;
    }

    public float GetCastTimer() // using agility currently
    {
        int bonuses = 0;
        return baseCastingTime / ((agility * agilityAttackSpeedConstant + bonuses) / 100 + 1);
    }

    IEnumerator WebUp() // Ability
    {
        if (target != null)
        {
            // Start the animations
            Debug.Log("WebUp started.");
            canAttack = false;
            untargetable = true;
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMoveY(7, 0.6f));
            sequence.Append(transform.DOMoveX(target.transform.position.x+2, 0.1f));
            sequence.Append(transform.DOMoveY(target.transform.position.y, 0.6f));

            // Waiting
            yield return new WaitForSeconds(1.1f);
            FlipSprite();
            untargetable = false;
            canAttack = true;
        }
    }

    public void LaunchAbilityProjectile(Projectile projectilePrefab) // Just makes the projectile and sends it
    {
        if (target != null)
        {
            Projectile projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
            projectile.transform.position = transform.position;
            projectile.SetTarget(target);
            projectile.SetAttacker(this);
            timeSinceLastAttack = 0f;
            mana = 0;
        }
    }

    public void Summon(Unit summonedUnitPrefab) // Ability
    {
        if (target != null)
        {
            mana = 0;
            //Unit summonedUnit = Instantiate(unitPrefab, transform.position, transform.rotation);
            Unit summonedUnit = Instantiate(summonedUnitPrefab, transform.position, transform.rotation);
            summonedUnit.transform.position = transform.position;
            summonedUnit.team = team;
            summonedUnit.battleStarted = true;
            timeSinceLastAttack = 0f;
        }
    }

    public void FlipSprite() // Flips it from its current facing
    {
        GetComponentInChildren<SpriteRenderer>().flipX = !GetComponentInChildren<SpriteRenderer>().flipX;
    }
  
    void AttackBehavior()
    {
        if (!canAttack) { return; }
        if (target.team == this.team) return;
        if (!target.alive)
        {
            target = null;
            return;
        }
        mana += 1;
        timeSinceLastAttack = 0f;
        if (range >= 1.5f)
        {
            RangedAttack();
            PunchAnimation();
        }
        else
        {
            DealDamage();
            PunchAnimation();
        }
    }

    void RangedAttack() // Just makes the projectile and sends it
    {
        if (target != null)
        {
            timeSinceLastAttack = 0f;
            Projectile projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
            projectile.transform.position = transform.position;
            projectile.SetTarget(target);
            projectile.SetAttacker(this);
        }
    }

    public void FindNearestTarget()
    {
        FindObjectOfType<GameManager>().GetClosestTarget(this);
    }

    public void SetTarget(Unit newTarget)
    {
        target = newTarget;
    }

    public void HandleAbilities() // Handles middle of battle abilities based on mana
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] != null)
            {
                if (mana >= abilities[i].manaToActivate)
                {
                    // Filter through abilities here 
                    if (abilities[i].name == "SummonEnt")
                    {
                        timeSinceLastAttack = 0f;
                        Summon(abilities[i].summonedUnitPrefab);
                    }
                    if (abilities[i].name == "Fireball")
                    {
                        timeSinceLastAttack = 0f;
                        LaunchAbilityProjectile(abilities[i].projectilePrefab);
                    }
                }
            }
        }
    }

    public void HandleStartAbilities()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] != null)
            {
                if (abilities[i].name == "WebUp")
                {
                    FindObjectOfType<GameManager>().GetFurthestTarget(this);
                    initAbility = false;
                    StartCoroutine(WebUp());
                }
            }
        }
    }

    public void CheckDeath()
    {
        if (currentHealth <= 0)
        {
            if (OnDeath != null)
            {
                OnDeath();
            }
            this.alive = false;
            gameObject.SetActive(false);
        }
    }
    
    public void Update()
    {
        if (!battleStarted) return; // nothing happens before battle
        if (!alive) Destroy(gameObject); // destroy game object if it was missed
        CheckDeath(); // OnDeath event
        if (initAbility) HandleStartAbilities(); // engage abilities
        if (InCastRangeOfTarget() && timeSinceLastAttack >= GetCastTimer()) // casting time attack time figure this out
        {
            HandleAbilities(); // mana abilities
        }
        if (InAttackRangeOfTarget() && timeSinceLastAttack >= GetAttackSpeedTimer()) // basic attacks and attack speed
        {
            AttackBehavior();
        }
        timeSinceLastAttack += Time.deltaTime;
    }

    public void FixedUpdate()
    {
        if (!battleStarted) return;
        if (!this.alive) return;
        if (target != null)
        {
            if (target.untargetable)
            {
                FindNearestTarget();
            }
            Vector2 direction = target.transform.position - transform.position;
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (!InAttackRangeOfTarget()) transform.position = Vector2.MoveTowards(this.transform.position, target.transform.position, speed * Time.deltaTime);
        }
    }
}
