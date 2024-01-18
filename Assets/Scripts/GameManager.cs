using System;
using System.CodeDom;
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

    void UnitDied(Unit unit)
    {
        units.Remove(unit);
        SetupAllTargets();
        //SetupUnits();
    }

    void UnitCreated(Unit unit)
    {
        //units.Append(unit);
        //units.Add(unit);
        GetClosestTarget(unit);
    }

    public void AddUnit(Unit unit)
    {
        if (unit.enabled)
        {
            units.Add(unit);
            GetClosestTarget(unit);
            unit.OnDeath += () => UnitDied(unit);
            unit.OnCreate += () => UnitCreated(unit);
        }
        
    }

    public void StartBattle()
    {
        foreach (Unit unit in units)
        {
            GetClosestTarget(unit);
            unit.battleStarted = true;
        }
    }

    void SetupUnits()
    {
        // the bug is right here
        foreach (Unit unit in units)
        {
            AddUnit(unit);
        }
    }

    void SetupAllTargets()
    {
        foreach (Unit unit in units)
        {
            GetClosestTarget(unit);
        }
    }

    public void GetClosestTarget(Unit unit)
    {
        //Debug.Log("Get target fired");
        float shortestDistance = 500f;
        foreach (Unit loopedUnit in units)
        {
            if (unit.team == Team.Player)
            {
                if (loopedUnit.team == Team.Enemy && !loopedUnit.untargetable)
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
                if (loopedUnit.team == Team.Player && !loopedUnit.untargetable)
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


    public void GetFurthestTarget(Unit unit)
    {
        float longestDistance = 0f;
        foreach (Unit loopedUnit in units)
        {
            if (unit.team == Team.Player)
            {
                if (loopedUnit.team == Team.Enemy)
                {
                    float newDistance = Vector2.Distance(unit.transform.position, loopedUnit.transform.position);
                    if (newDistance > longestDistance)
                    {
                        longestDistance = newDistance;
                        unit.SetTarget(loopedUnit);
                    }
                }
            }

            if (unit.team == Team.Enemy)
            {
                if (loopedUnit.team == Team.Player)
                {
                    float newDistance = Vector2.Distance(unit.transform.position, loopedUnit.transform.position);
                    if (newDistance > longestDistance)
                    {
                        longestDistance = newDistance;
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
