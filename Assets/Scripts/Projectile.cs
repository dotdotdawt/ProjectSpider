using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Unit target;
    public Unit attacker;
    public Rigidbody2D rb;
    public float projectileSpeed;
    public bool isHoming;
    public float lifespan;
    public bool hitTarget = false;

    public bool isSpinning;
    public float degreesPerSecond = 200f;

    //public Collider2D collider;

    public void SetTarget(Unit newTarget)
    {
        //Debug.Log("Setting target: " + newTarget.name);
        target = newTarget;
    }

    void OnAwake()
    {
        StartCoroutine(DestroySelfAfterSeconds(lifespan));
    }

    IEnumerator DestroySelfAfterSeconds(float destroyTime)
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }

    public void SetAttacker(Unit newAttacker)
    {
        attacker = newAttacker;
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Unit unit = hitInfo.GetComponent<Unit>();
        if (unit == target)
        {
            attacker.DealDamage(attacker.ad);
            hitTarget = true;
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {

        if (target != null)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, target.transform.position, projectileSpeed * Time.deltaTime);
            //rb.AddForce(target.transform.position - transform.position * projectileSpeed, ForceMode2D.Impulse);
        }

        if (isSpinning)
        {
            transform.Rotate(new Vector3(0, 0, degreesPerSecond) * Time.deltaTime);
        }

        if (isHoming)
        {
            //rb.AddForce(target.transform.position - transform.position * projectileSpeed);
        }

    }
}

