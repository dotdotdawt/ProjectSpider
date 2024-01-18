using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitInventory : MonoBehaviour
{
    [SerializeField] Transform slots;
    [SerializeField] Text inventoryText;
    void Start()
    {
        HasChanged ();
    }

    public void HasChanged () 
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(" - ");
        foreach (Transform slotTransform in slots)
        {
            GameObject unitDragged = slotTransform.GetComponent<UnitSlot>().unitRef;
            if (unitDragged == null) return;
            if (unitDragged)
            {
                builder.Append (unitDragged.name);
                builder.Append (" - ");

            }
        }
        inventoryText.text = builder.ToString();
    }

}

namespace UnityEngine.EventSystems {
    public interface IHasChanged : IEventSystemHandler {
        void HasChanged();
    }
}