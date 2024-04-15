using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Buff : MonoBehaviour
{
    public Stat statUsed;
    public int amountChange;
    public float duration;
    public bool active = true;
    public bool started = false;
    public float secondsPassed = 0f;

   
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
        Wisdom
    }
    //public int statsToChange[];
    // Stat stat, float change, float percentValue = 1f, float flatValue = 0f, float length, string buffName

    public void StartBuff()
    {
        started = true;
    }


    public void SetStat(Stat stat)
    {
        statUsed = stat;
    }

    public void SetAmount(int amount)
    {
        amountChange = amount;
    }

    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    public void ResetAll()
    {

    }

    public void Deactivate()
    {
        if (duration < secondsPassed)
        {
            active = false;
        }
    }

    public void Update()
    {
        if (!started) return;
        Deactivate();
        if (started)
        {
            secondsPassed += Time.deltaTime;
        }
        
    }

}
