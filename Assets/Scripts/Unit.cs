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

public enum TextType
{
    Physical,
    Magical,
    Healing,
    Shielding
}

public class Unit : MonoBehaviour
{
    // Public Actions
    public event Action OnDeath;
    public event Action OnCreate;

    public string unitName;

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
    public float rawMoveSpeed = 3f;
    public int speedStat; // Need to write logic to clamp speed between 2f-8f
    public float range = 5f;
    public float castRange = 5f;
    public float mana = 0f;
    public int armor;
    public int magicResist;

    public int bonusMight;
    public int bonusMind;
    public int bonusFortitude;
    public int bonusAgility;
    public int bonusVitality;
    public int bonusSpirit;
    public int bonusSense;
    public int bonusSpeed;

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
    public float minimumMoveSpeedConstant = 2f;
    public float maximumMoveSpeedConstant = 8f;
    public float speedPerPointConstant = 1f;
    public float baseMoveSpeedConstant = 3f;

    [Header("Static Object References")]
    public Rigidbody2D rb;
    public HealthBar healthBar;
    public GameObject floatingTextPrefab;
    public Buff buffPrefab;

    [Header("Object References")]
    public Unit target;
    public Team team;
    public Unit summonedUnitPrefab;
    public Projectile projectilePrefab;
    public Ability[] abilities;
    public List<Buff> buffs = new List<Buff>();

    public Color physicalDamageColor;
    public Color magicalDamageColor;

    [Header("Boolean Toggles")]
    public bool initAbility;
    public bool battleStarted = false;
    public bool alive = false;
    public bool canAttack = true;
    public bool untargetable = false;


    private int vibrato = 7;
    private float elasticity = 0.81f;
    private bool snapping = false;
    private float tamperAttackVector = 0.21f;
    private float tamperAS = 0.3f;
    private float rangedTamper = 0.18f;

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
        return ((vitality + bonusVitality) * baseVitalityConstant) + baseHealthConstant;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetCurrentMana()
    {
        return mana;
    }

    public int GetBasicAttackDamage()
    {
        return baseAttackDamageConstant + might + bonusMight; // Check for alterations above
    }

    public int GetMindStat()
    {
        return mind + baseSpellPowerConstant + bonusMind;
    }

    public int GetArmorStat()
    {
        return fortitude + baseArmorConstant + bonusFortitude;
    }

    public int GetSenseStat()
    {
        return sense + bonusSense;
    }

    public int GetMagicResistStat()
    {
        return spirit + baseMagicResistConstant + bonusSpirit;
    }

    public float GetMovementSpeedStat()
    {
        return speedStat + bonusSpeed;
    }

    public float GetAttackSpeedStat()
    {
        return 1f / GetAttackSpeedTimer();
    }

    public int GetArmorReducedDamage(int rawDamage) // m is decimal
    {
        return Mathf.RoundToInt(System.Convert.ToSingle(100m / (GetArmorStat() + 100m)) * rawDamage);
    }

    public int GetMagicReducedDamage(int rawDamage) // m is decimal
    {
        return Mathf.RoundToInt(System.Convert.ToSingle(100m / (GetMagicResistStat() + 100m)) * rawDamage);
    }

    public float GetRawMovementSpeed()
    {
        float rawMS = baseMoveSpeedConstant * ((100 + GetMovementSpeedStat()) / 100);
        if (rawMS > maximumMoveSpeedConstant)
        {
            return maximumMoveSpeedConstant;
        }
        if (rawMS < minimumMoveSpeedConstant)
        {
            return minimumMoveSpeedConstant;
        }
        else
        {
            return rawMS;
        }
    }

    public void DealPhysicalDamage()
    {
        int _rawDmg = GetArmorReducedDamage(GetBasicAttackDamage());
        if (floatingTextPrefab)
        {
            ShowFloatingText(_rawDmg, TextType.Physical);
        }
        target.currentHealth -= _rawDmg;
        target.healthBar.SetHealth(target.currentHealth);
    }

    public void DealMagicDamage()
    {
        int _rawDmg = GetMagicReducedDamage(GetBasicAttackDamage()+25);
        
        if (floatingTextPrefab)
        {
            ShowFloatingText(_rawDmg, TextType.Magical);
        }
        target.currentHealth -= _rawDmg;
        target.healthBar.SetHealth(target.currentHealth);
    }
    

    public void StatsUpdate()
    {
        rawMoveSpeed = GetRawMovementSpeed();
    }

    public void ShowFloatingText(int damage, TextType textType) // Lots of DOTween customization... if can be made shorter, do it.
    {
        Vector3 spawnLoc;
        double _rx = target.transform.position.x + (UnityEngine.Random.Range(-1, 1)*0.1);
        double _ry = target.transform.position.y + (UnityEngine.Random.Range(-1, 1)*0.1);
        spawnLoc.x = System.Convert.ToSingle(_rx);
        spawnLoc.y = System.Convert.ToSingle(_ry);
        spawnLoc.z = target.transform.position.z;
        var fText = Instantiate(floatingTextPrefab, spawnLoc, Quaternion.identity, target.transform);
        fText.GetComponent<TextMeshPro>().text = damage.ToString();

        // moving
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

    public void HandleBuffs()
    {
        if (!battleStarted) return;

        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i] == null) return;

            if (buffs[i] != null)
            {
                if (!buffs[i].active)
                {
                    RemoveBuff(buffs[i]);
                    buffs.Remove(buffs[i]);
                }
            }
        }
    }

    public void CreateBuff(int amount, float duration, Buff.Stat stat, Ability fromAbility)
    {
        Buff newBuff = Instantiate(buffPrefab, transform.position, transform.rotation);
        newBuff.SetAmount(amount);
        newBuff.SetDuration(duration);
        newBuff.SetStat(stat);
        buffs.Add(newBuff);
        AddBuff(newBuff);
        newBuff.StartBuff(); // not great but redo later

        // Make and destroy buff VFX
        if (fromAbility && fromAbility.VFXSelfBuffPrefab != null)
        {
            GameObject _vfxWebUp = Instantiate(fromAbility.VFXSelfBuffPrefab, transform.position, transform.rotation);
            _vfxWebUp.transform.parent = transform;
            Destroy(_vfxWebUp, duration);
        }
        
    }

    public void AddBuff(Buff buff)
    {
        Debug.Log("Adding " + (buff.amountChange, buff.statUsed, name));
        if (buff.statUsed == Buff.Stat.Might)
        {
            bonusMight += buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Mind)
        {
            bonusMind += buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Agility)
        {
            bonusAgility += buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Fortitude)
        {
            bonusFortitude += buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Spirit)
        {
            bonusSpirit += buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Vitality)
        {
            bonusVitality += buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Sense)
        {
            bonusSense += buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Speed)
        {
            bonusSpeed += buff.amountChange;
        }
    }

    public void RemoveBuff(Buff buff)
    {
        Debug.Log("Removing " + (buff.amountChange, buff.statUsed, name));
        if (buff.statUsed == Buff.Stat.Might)
        {
            bonusMight -= buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Mind)
        {
            bonusMind -= buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Agility)
        {
            bonusAgility -= buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Fortitude)
        {
            bonusFortitude -= buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Spirit)
        {
            bonusSpirit -= buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Vitality)
        {
            bonusVitality -= buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Sense)
        {
            bonusSense -= buff.amountChange;
        }
        if (buff.statUsed == Buff.Stat.Speed)
        {
            bonusSpeed -= buff.amountChange;
        }
        var parentObject = buff.GetComponentInParent<Transform>();
        Destroy(parentObject.gameObject);
        Destroy(buff);
    }

    public void RemoveAllBuffs()
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i] != null)
            {
                RemoveBuff(buffs[i]);
            }
        }
    }



    IEnumerator WebUp(Ability abilityInstance) // Ability
    {
        if (target != null)
        {
            // Start the animations
            Debug.Log("WebUp started.");
            canAttack = false;
            untargetable = true;

            // strand
            //GameObject webUpStrand = GameObject.FindWithTag("AbilityCosmetic");
            GameObject webUpStrand = Instantiate(abilityInstance.VFXSpecialEffect, transform.position, transform.rotation);
            webUpStrand.transform.parent = transform;
            webUpStrand.SetActive(true);
            webUpStrand.transform.position = new Vector3(webUpStrand.transform.position.x, webUpStrand.transform.position.y + 8, webUpStrand.transform.position.z);
            Destroy(webUpStrand, 5);

            // buff
            CreateBuff(30, 2f, Buff.Stat.Might, abilityInstance);
            CreateBuff(50, 2f, Buff.Stat.Speed, abilityInstance);
            StatsUpdate();

            // animation
            float xOffset = 1.5f;
            int newX = Mathf.RoundToInt(xOffset + target.transform.position.x);
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMoveY(7, 0.6f));
            sequence.Append(transform.DOMoveX(newX, 0.1f));
            sequence.Append(transform.DOMoveY(target.transform.position.y, 0.6f));

            // Waiting and return to normal state
            yield return new WaitForSeconds(1.1f);
            webUpStrand.transform.DOMoveY(20, 3f);
            FlipSprite();
            untargetable = false;
            canAttack = true;
        }
    }

    public void ApplySpellEffect(Ability fromAbility)
    {
        target.CreateBuff(-40, 5, Buff.Stat.Speed, fromAbility);
    }

    public void LaunchAbilityProjectile(Projectile projectilePrefab, Ability fromAbility) // Just makes the projectile and sends it
    {
        if (target != null)
        {
            Projectile projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
            projectile.transform.position = transform.position;
            projectile.SetTarget(target);
            projectile.SetAttacker(this);
            projectile.isSpell = true;
            if (fromAbility != null)
            {
                projectile.SetAbilityOrigin(fromAbility);
            }
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
            DealPhysicalDamage();
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
                if (mana >= abilities[i].manaToActivate && abilities[i].cooldownTime <= timeSinceLastCast)
                {
                    // Filter through abilities here 
                    if (abilities[i].name == "SummonEnt")
                    {
                        timeSinceLastAttack = 0f;
                        timeSinceLastCast = 0f;
                        Summon(abilities[i].summonedUnitPrefab);
                    }
                    if (abilities[i].name == "Fireball")
                    {
                        timeSinceLastAttack = 0f;
                        timeSinceLastCast = 0f;
                        LaunchAbilityProjectile(abilities[i].projectilePrefab, abilities[i]);
                    }
                    if (abilities[i].name == "Ice Web")
                    {
                        timeSinceLastAttack = 0f;
                        timeSinceLastCast = 0f;
                        LaunchAbilityProjectile(abilities[i].projectilePrefab, abilities[i]);
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
                    StartCoroutine(WebUp(abilities[i]));
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
                RemoveAllBuffs();
                Projectile lastProj = FindObjectOfType<Projectile>();
                if (lastProj != null)
                {
                    Destroy(lastProj);
                }
                OnDeath();
            }
            this.alive = false;
            gameObject.SetActive(false);
        }
    }
    
    public void Update()
    {
        HandleBuffs();
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
            if (!InAttackRangeOfTarget()) transform.position = Vector2.MoveTowards(this.transform.position, target.transform.position, rawMoveSpeed * Time.deltaTime);
        }
    }
}
