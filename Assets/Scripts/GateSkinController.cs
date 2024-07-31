using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateSkinController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer gate;
    [SerializeField] private Sprite[] gateSprites;

    [SerializeField] private SpriteRenderer left;
    [SerializeField] private Sprite[] leftSprites;

    [SerializeField] private SpriteRenderer right;
    [SerializeField] private Sprite[] rightSprites;

    public void SetRandomSkin()
    {
        int skinIndex = Random.Range(0, gateSprites.Length);

        gate.sprite = gateSprites[skinIndex];
        left.sprite = leftSprites[skinIndex];
        right.sprite = rightSprites[skinIndex];
    }
}
