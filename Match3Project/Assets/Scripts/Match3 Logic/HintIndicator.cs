using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This script allows displaying a sprite over a transform
 * automatically after a time delay
 * 
 * This is a singleton and requires a sprite renderer component
 */

//TODO: Make hint list for all possible matches

public class HintIndicator : Singleton<HintIndicator>
{
    private SpriteRenderer spriteRenderer;

    private Transform hintLocation;

    private Coroutine autoHintCR;

    [SerializeField] private Button hintButton;

    [SerializeField] private float delayBeforeAutoHint;

    protected override void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        hintButton.interactable = false;
    }

    public void IndicateHint(Transform hintLocation)
    {
        transform.position = hintLocation.position;
        spriteRenderer.enabled = true;
    }

    public void CancelHint()
    {
        spriteRenderer.enabled = false;
        hintButton.interactable = false;

        if (autoHintCR != null)
            StopCoroutine(autoHintCR);

        autoHintCR = null;
    }

    public void EnableHintButton()
    {
        hintButton.interactable = true;
    }

    public void StartAutoHint(Transform hintLocation)
    {
        this.hintLocation = hintLocation;

        autoHintCR = StartCoroutine(WaitAndIncateHint());
    }

    private IEnumerator WaitAndIncateHint()
    {
        yield return new WaitForSeconds(delayBeforeAutoHint);
        IndicateHint(hintLocation);
    }

}
