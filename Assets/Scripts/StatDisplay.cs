
using System.Collections;
using UnityEngine;
using TMPro;
using System.Globalization;

public class StatDisplay : MonoBehaviour
{
    public enum Stat 
    {
        AttackDamage,
        Mind,
        Armor,
        MagicResist,
        AttackSpeed,
        Sense,
        MovementSpeed,
        Health,
        Mana
    }
    public Unit unit;
    public Stat statUsed;

    // Start is called before the first frame update
    void Start()
    {
        if (unit == null)
        {
            return;
        }
        SetupStat();
    }

    void SetupStat()
    {
        if (statUsed == Stat.AttackDamage)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetBasicAttackDamage();
        }
        if (statUsed == Stat.Mind)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetMindStat();
        }
        if (statUsed == Stat.Armor)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetArmorStat();
        }
        if (statUsed == Stat.MagicResist)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetMagicResistStat();
        }
        if (statUsed == Stat.AttackSpeed)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetAttackSpeedStat();
        }
        if (statUsed == Stat.MovementSpeed)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetRawMovementSpeed(); // instead of stat to show raw data
        }
        if (statUsed == Stat.Health)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetCurrentHealth() + "/" + "" + unit.GetMaxHealth();
        }
        if (statUsed == Stat.Mana)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetCurrentMana();
        }
        if (statUsed == Stat.Sense)
        {
            this.GetComponent<TextMeshProUGUI>().text = "" + unit.GetSenseStat();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (unit == null)
        {
            return;
        }
        SetupStat();
    }
}
