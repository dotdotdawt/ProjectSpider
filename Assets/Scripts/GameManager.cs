using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : MonoBehaviour
{
    //public Unit[] units = null;
    public List<Unit> units = new List<Unit>();
    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void StartBattle()
    {
        SetupUnits();
    }

    void UnitDied(Unit unit)
    {
        units.Remove(unit);
        SetupUnits();
    }

    void SetupUnits()
    {
        foreach (Unit unit in units)
        {

            GetSetTarget(unit);
            unit.battleStarted = true;
            unit.OnDeath += () => UnitDied(unit);
        }
    }

    public void GetSetTarget(Unit unit)
    {
        //Debug.Log("Get target fired");
        float shortestDistance = 500f;
        foreach (Unit loopedUnit in units)
        {
            if (unit.team == Team.Player)
            {
                if (loopedUnit.team == Team.Enemy)
                {
                    float newDistance = Vector2.Distance(unit.transform.position, loopedUnit.transform.position);
                    if (newDistance < shortestDistance)
                    {
                        shortestDistance = newDistance;
                        unit.SetTarget(loopedUnit);
                    }
                }
            }

            if (unit.team == Team.Enemy)
            {
                if (loopedUnit.team == Team.Player)
                {
                    float newDistance = Vector2.Distance(unit.transform.position, loopedUnit.transform.position);
                    if (newDistance < shortestDistance)
                    {
                        shortestDistance = newDistance;
                        unit.SetTarget(loopedUnit);
                    }
                }
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
      
    }
}
