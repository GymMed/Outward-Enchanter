using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragParent : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private RectTransform parentRectTransform;
    private Vector3 offset;

    void Start()
    {
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = parentRectTransform.position - Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        parentRectTransform.position = Input.mousePosition + offset;
    }
}
