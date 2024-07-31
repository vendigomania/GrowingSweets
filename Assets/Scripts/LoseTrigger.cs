using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LoseTrigger : MonoBehaviour
{
    public UnityAction OnLose;

    float delay = 0f;

    public void SetDelay()
    {
        delay = 1f;
    }

    private void Update()
    {
        if(delay > 0f) delay -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (delay > 0f) return;

        OnLose?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (delay > 0f) return;

        OnLose?.Invoke();
    }
}
