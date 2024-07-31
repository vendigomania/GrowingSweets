using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallItem : MonoBehaviour
{
    [SerializeField] private GameObject[] skins;

    public static UnityAction<BallItem, BallItem> OnCollision;

    public int Value { get; private set; }

    public void SetValue(int _value)
    {
        Value = _value;
        transform.localScale = Vector3.zero;
        for (int i = 0; i < skins.Length; i++)
        {
            skins[i].gameObject.SetActive(i == Value);
        }
    }

    private void Update()
    {
        if(transform.localScale.x < 1f)
        {
            float size = Mathf.Min(1f, transform.localScale.x + Time.deltaTime * 2);
            transform.localScale = Vector2.one * size;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent<BallItem>(out var item))
        {
            if(Value == item.Value)
            {
                OnCollision?.Invoke(this, item);
            }
        }
    }
}
