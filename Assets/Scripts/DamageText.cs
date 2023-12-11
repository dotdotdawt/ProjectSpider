using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] TMP_Text damageText = null;

    public void DestroyText()
    {
        Destroy(gameObject);
    }

    public void SetValue(float amount)
    {
        damageText.text = System.String.Format("{0:0}", amount);
    }
}

//public class DamageTextSpawner : MonoBehaviour
//{
//    [SerializeField] DamageText damageTextPrefab = null;
//
//    public void Spawn(float damageAmount)
//    {
//        DamageText instance = Instantiate<DamageText>(damageTextPrefab, transform);
//        instance.SetValue(damageAmount);
//    }
//
//}
//public class Destroyer : MonoBehaviour
//{
//    [SerializeField] GameObject targetToDestroy = null;
//
//    public void DestroyTarget()
//    {
//        Destroy(targetToDestroy);
//    }
//}
//
//public class DamageText : MonoBehaviour
//{
//    [SerializeField] TMP_Text damageText = null;
//
//    public void DestroyText()
//    {
//        Destroy(gameObject);
//    }
//
//    public void SetValue(float amount)
//    {
//        damageText.text = String.Format("{0:0}", amount);
//    }
//}
