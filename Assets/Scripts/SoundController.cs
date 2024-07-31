using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource click;
    [SerializeField] private AudioSource win;
    [SerializeField] private AudioSource react;

    public static SoundController Instance;

    private void Start()
    {
        Instance = this;
    }

    public void Click()
    {
        if(enabled) click.Play();
    }

    public void React()
    {
        if(enabled) react.Play();
    }

    public void Win()
    {
        if(enabled) win.Play();
    }
}
