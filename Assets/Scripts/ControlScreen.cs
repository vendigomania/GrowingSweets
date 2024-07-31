using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ControlScreen : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Transform moved;

    public static UnityAction OnShot;

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        var pos = moved.position;
        pos.x = Mathf.Clamp(pos.x + eventData.delta.x * Time.deltaTime, -2.8f, 2.8f);

        moved.position = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnShot?.Invoke();
    }
}
