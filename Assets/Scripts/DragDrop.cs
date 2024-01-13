using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{

    [SerializeField] private Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponentInChildren<RectTransform>();
        //rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            Debug.Log("OnBeginDrag");
            //canvasGroup.alpha = .6f;
            canvasGroup.blocksRaycasts = false;
        }
        

    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        //new Vector2 transform.position += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            Debug.Log("OnEndDrag");
            //canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
            
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
    }

}