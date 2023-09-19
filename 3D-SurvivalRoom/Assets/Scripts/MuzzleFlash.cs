using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float flashTime;
    public GameObject flashHolder;
    public Sprite[] flashSprites;
    public SpriteRenderer[] spriteRenderers;

    private void Start()
    {
        DeActivate();
    }
    public void Activate()
    {
        flashHolder.SetActive(true);
        int flashSpriteIndex = Random.Range(0, flashSprites.Length);
        for ( int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = flashSprites[flashSpriteIndex];
        }
        Invoke("DeActivate", flashTime);
    }
    public void DeActivate()
    {
        flashHolder.SetActive(false);
    }
}
