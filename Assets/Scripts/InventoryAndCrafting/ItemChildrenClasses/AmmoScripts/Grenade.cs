using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : Explosive
{
    [Header("Grenade Variables")]
    public bool triggerPulled = false;
    public float timer = 2;

    [Header("Juice Variables")]
    public float flashingSpeed = 2;
    public Color activatedcolor = Color.red;

    private float lowActivationLerp = 0;
    private float maxActivationLerp = 1;
    private Color originalColor;
    private bool glowActivatedColor = false;

    [SerializeField]
    private float countdown;

    private bool hasPlayed = false;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        originalColor = rend.material.color;
    }

    void OnEnable()
    {
        countdown = timer;
        ActivateGrenade();
    }

    //Grenade countdown to explode
    void Update()
    {
        if (triggerPulled)
        {
            //hack for soundbite of grenade. Fix later with an audiomanager
            if (!hasPlayed)
            {
                glowActivatedColor = true;
                hasPlayed = true;
            }

            DetonationJuice();

            countdown -= Time.deltaTime;
            if (countdown <= 0 && !hasExploded)
            {
                Explode();
            }
        }
    }

    public void DetonationJuice()
    {
        if (glowActivatedColor)
        {
            if (lowActivationLerp < maxActivationLerp - 0.1f)
            {
                rend.material.color = Color.Lerp(rend.material.color, activatedcolor, flashingSpeed * Time.deltaTime);
                lowActivationLerp = Mathf.Lerp(lowActivationLerp, maxActivationLerp, flashingSpeed * Time.deltaTime);
                return;
            }
            lowActivationLerp = maxActivationLerp;
            glowActivatedColor = false;
        }
        else
        {
            if (lowActivationLerp > 0.1f)
            {
                rend.material.color = Color.Lerp(rend.material.color, originalColor, flashingSpeed * Time.deltaTime);
                lowActivationLerp = Mathf.Lerp(lowActivationLerp, 0, flashingSpeed * Time.deltaTime);
                return;
            }

            lowActivationLerp = 0;
            glowActivatedColor = true;
        }
    }

    public void ActivateGrenade()
    {
        triggerPulled = true;
    }

    private void OnDisable()
    {
        countdown = timer;
        triggerPulled = false;
        hasPlayed = false;

        rend.material.color = originalColor;
        glowActivatedColor = false;
        lowActivationLerp = 0;

        ResetExplosive();
    }
}
