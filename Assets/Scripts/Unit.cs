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
    Shielding,
    Critical
}

public enum TargetType
{
    Self,
    Target,
    Ally,
    Allies,
    Area,
    Enemies
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
    public float wisdom; // NEW
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
    public float bonusWisdom;

    [Header("Constants AD, Armor and Agility Calculation")]
    public float baseAttackTime = 2f; // 2 seconds
    public float agilityAttackSpeedConstant = 2f; // 2%
    public float wisdomCastSpeedConstant = 2f;
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
    public int baseSizeMultiplier;
    //public int baseSizeMultiplier = 1;

    [Header("Static Object References")]
    public Rigidbody2D rb;
    public HealthBar healthBar;
    public GameObject floatingTextPrefab;
    public GameObject floatingTextMagicPrefab;
    public GameObject floatingTextCritPrefab;
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
        return baseAttackDamageConstant + GetMightStat(); // Check for alterations above
    }

    public int GetMightStat()
    {
        return might + bonusMight;
    }

    public int GetMindStat()
    {
        return mind + baseSpellPowerConstant + bonusMind;
    }

    public float GetWisdomStat()
    {
        return wisdom + bonusWisdom;
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

    public int GetArmorReducedDamage(float rawDamage) // m is decimal
    {
        return Mathf.RoundToInt(System.Convert.ToSingle(100m / (GetArmorStat() + 100m)) * rawDamage);
    }

    public int GetMagicReducedDamage(float rawDamage) // m is decimal
    {
        return Mathf.RoundToInt(System.Convert.ToSingle(100m / (GetMagicResistStat() + 100m)) * rawDamage);
    }

    public void HandleSpell(Ability ability)
    {
        Unit _tgt = target;
        int _dmg = 0;
        TextType _txt = TextType.Physical; // default

        // Target Types
        if (ability.spellType == Ability.SpellType.Target) // Single target ability magic
        {
            _tgt = target;
        }
        if (ability.spellType == Ability.SpellType.Self) // Self only target
        {
            _tgt = this;
        }

        // Damage Types
        if (ability.damageType == Ability.DamageType.Magical) // Magic
        {
            _dmg = Mathf.RoundToInt(GetMagicReducedDamage(ability.spellDamage + (ability.dmgBoostRatio * _tgt.GetMindStat())));
            _txt = TextType.Magical;
        }
        if (ability.damageType == Ability.DamageType.Physical) // Physical
        {
            _dmg = Mathf.RoundToInt(GetArmorReducedDamage(ability.spellDamage + (ability.dmgBoostRatio * _tgt.GetMightStat())));
            _txt = TextType.Physical;
        }

        // Deal it
        DealDamageToTarget(_dmg, _tgt, _txt);
        ApplySpellEffect(ability, _tgt);
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

    public void DealBasicAttackDamage(int damage=0)
    {
        int _rawDmg = GetArmorReducedDamage(GetBasicAttackDamage());
        if (damage != null)
        {
            if (damage != 0)
            {
                Debug.Log("Incoming damage " + _rawDmg);
                _rawDmg = GetArmorReducedDamage(damage);
            }
        }
        // Always default target currently
        DealDamageToTarget(_rawDmg, target, TextType.Physical);
    }

    public void DealDamageToTarget(int damage, Unit spTarget, TextType textType)
    {
        if (floatingTextPrefab)
        {
            ShowFloatingText(damage, textType);
        }
        spTarget.currentHealth -= damage;
        spTarget.healthBar.SetHealth(spTarget.currentHealth);
    }
    

    public void StatsUpdate()
    {
        rawMoveSpeed = GetRawMovementSpeed();
    }

    public void ShowFloatingText(int damage, TextType textType) // Lots of DOTween customization... if can be made shorter, do it.
    {
        GameObject _pfb = null;
        if (textType == TextType.Physical)
        {
            _pfb = floatingTextPrefab;
        }
        if (textType == TextType.Magical)
        {
            _pfb = floatingTextMagicPrefab;
        }
        if (textType == TextType.Critical)
        {
            _pfb = floatingTextCritPrefab;
        }

        if (_pfb != null)
        {
            Vector3 spawnLoc;
            double _rx = target.transform.position.x + (UnityEngine.Random.Range(-1, 1) * 0.1);
            double _ry = target.transform.position.y + (UnityEngine.Random.Range(-1, 1) * 0.1);
            spawnLoc.x = System.Convert.ToSingle(_rx);
            spawnLoc.y = System.Convert.ToSingle(_ry);
            spawnLoc.z = target.transform.position.z;
            var fText = Instantiate(_pfb, spawnLoc, Quaternion.identity, target.transform);
            fText.GetComponent<TextMeshPro>().text = damage.ToString();

            // moving
            fText.transform.DOMoveY((fText.transform.position.y + 1), 1f).SetEase(Ease.InSine);
            var sequence = DOTween.Sequence();
            double _x = fText.transform.localScale.x * 4;
            double _y = fText.transform.localScale.y * 4;
            sequence.Append(fText.transform.DOScaleX(Math.Abs(System.Convert.ToSingle(_x)), 0.8f).SetEase(Ease.InOutSine));
            sequence.Join(fText.transform.DOScaleY(Math.Abs(System.Convert.ToSingle(_y)), 0.8f).SetEase(Ease.InOutSine));
            sequence.Append(fText.transform.DOScaleX(0, 0.8f).SetEase(Ease.InOutSine));
            sequence.Join(fText.transform.DOScaleY(0, 0.8f).SetEase(Ease.InOutSine));
        }
        
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
        if (range >= 1.5f * baseSizeMultiplier) // is ranged
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

    public float GetCastTimer() // using WISDOM currently
    {
        int bonuses = 0;
        return baseCastingTime / ((wisdom * wisdomCastSpeedConstant + bonuses) / 100 + 1);
        //return baseCastingTime / ((agility * agilityAttackSpeedConstant + bonuses) / 100 + 1);
    }

    public void HandleBuffs()
    {
        if (!battleStarted) return;

        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i] == null) 
            {
                return;
            }
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
        //Debug.Log("Adding " + (buff.amountChange, buff.statUsed, name));
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
        if (buff.statUsed == Buff.Stat.Wisdom)
        {
            bonusWisdom += buff.amountChange;
        }
    }

    public void RemoveBuff(Buff buff)
    {
        //Debug.Log("Removing " + (buff.amountChange, buff.statUsed, name));
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
        if (buff.statUsed == Buff.Stat.Wisdom)
        {
            bonusWisdom -= buff.amountChange;
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
                buffs.Remove(buffs[i]);
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
            CreateBuff(30, 5f, Buff.Stat.Might, abilityInstance);
            CreateBuff(50, 5f, Buff.Stat.Speed, abilityInstance);
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

    public void ApplySpellEffect(Ability ability, Unit fxTarget)
    {
        Debug.Log("ApplyingSpellEffect from ability" + ability.name);
        //target.CreateBuff(-40, 5, Buff.Stat.Speed, fromAbility);
        //target.CreateBuff(-20, 5, Buff.Stat.Armor, fromAbility);
        int statAmt = GetMindStat();
        float bonusAmt = ability.impactRatio * statAmt;
        int _amt = Mathf.RoundToInt(ability.impactAmt + bonusAmt);
        //float _amt = ability.impactAmt + Mathf.RoundToInt((ability.impactRatio * GetMindStat())); // negative number

        float _dur = ability.duration;
        //float _dur = ability.duration + Mathf.RoundToInt((ability.bonusDurationRatio * GetMindStat()));

        Buff.Stat _stat = Buff.Stat.Fortitude;
        //Stat _stat = ability.impactStat;

        fxTarget.CreateBuff(_amt, _dur, _stat, ability);
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
  
    public void Hit()
    {
        HandleOnHit();
        DealBasicAttackDamage();
        //DealPhysicalDamage();
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
        if (range >= 1.5f * baseSizeMultiplier)
        {
            RangedAttack();
            PunchAnimation();
        }
        else
        {
            Hit();
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

    public int GetRoll()
    {
        return UnityEngine.Random.Range(1, 100);
    }

    public float GetChanceToHitBonus(float ratio)
    {
        return GetSenseStat() * ratio;
    }

    public void HandleOnHit()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] != null)
            {
                if (abilities[i].name == "Acid Fangs")
                {
                    float bonusChance = GetChanceToHitBonus(abilities[i].chanceToHitBonusRatio);
                    float hitChance = abilities[i].chanceToHit + bonusChance;
                    int rollResult = GetRoll();

                    if (rollResult > 100-hitChance)
                    {
                        Debug.Log("Acid Fangs triggered " + abilities[i].name);
                        int debuffFactor = -1; // Debuff and not buff

                        // dmg
                        int _dmgBonus = Mathf.RoundToInt(abilities[i].bonusDamageMightRatio * GetMightStat());
                        _dmgBonus += Mathf.RoundToInt(abilities[i].bonusDamageMindRatio * GetMindStat());
                        _dmgBonus += Mathf.RoundToInt(abilities[i].damageOnHit);
                        int _dmg = Mathf.RoundToInt(_dmgBonus);

                        // impact
                        int _imp = Mathf.RoundToInt(abilities[i].bonusImpactMindRatio * GetMindStat());
                        _imp += Mathf.RoundToInt(abilities[i].targetBuffAmount);

                        // duration
                        float _dur = Mathf.RoundToInt(abilities[i].targetBuffDuration);
                        _dur += Mathf.RoundToInt(abilities[i].bonusDurationRatio * GetMindStat());

                        // Create
                        target.CreateBuff(_imp * debuffFactor, _dur, Buff.Stat.Armor, abilities[i]);

                        // Handlers
                        DealBasicAttackDamage(_dmg);
                        HandleSpell(abilities[i]);

                        // x
                        Debug.Log("Acid Fangs hit for " + _dmg);
                        GameObject _anim = Instantiate(abilities[i].targetAnimationPrefab, target.transform.position, target.transform.rotation);
                        Destroy(_anim, 0.8f);

                    }
                }
                if (abilities[i].name == "Blinding Strikes")
                {
                    int levelIndex = abilities[i].level - 1;
                    float hitChance = abilities[i].chanceToHit + GetChanceToHitBonus(abilities[i].chanceToHitBonusRatio);
                    int rollResult = GetRoll();
                    if (rollResult > 100 - hitChance)
                    {
                        
                        int damageDealt = Mathf.RoundToInt((abilities[i].bonusDamageMightRatio * GetMightStat()) + abilities[i].damageOnHit);
                        int debuffImpact = Mathf.RoundToInt((abilities[i].bonusImpactMindRatio * GetMindStat()) + abilities[i].targetBuffAmount);
                        float duration = (abilities[i].bonusDurationRatio * GetMindStat()) + abilities[i].targetBuffDuration;

                        target.CreateBuff(debuffImpact * -1, duration, Buff.Stat.Sense, abilities[i]);
                        //DealPhysicalDamage(damageDealt);
                        DealBasicAttackDamage();
                        HandleSpell(abilities[i]);
                        Debug.Log("Blinding Strikes hit for " + damageDealt);

                        GameObject _anim = Instantiate(abilities[i].targetAnimationPrefab, target.transform.position, target.transform.rotation);
                        Destroy(_anim, 0.8f);
                    }

                }
            }
        }
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
