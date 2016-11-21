﻿using UnityEngine;
using System.Collections;

// Extends PT_MonoBehaviour
public class FireGroundSpell : PT_MonoBehaviour
{

    public float duration = 4; // Lifetime of this GameObject
    public float durationVariance = 0.5f;
    // ^ This allows the duration to range from 3.5 to 4.5
    public float fadeTime = 1f; // Length of time to fade
    public float timeStart; // Birth time of this GameObject

    // Use this for initialization
    void Start()
    {
        timeStart = Time.time;
        duration = Random.Range(duration - durationVariance, duration + durationVariance);
        // ^ Set the duration to a number between 3.5 and 4.5 (defaults)
    }

    // Update is called once per frame
    void Update()
    {
        // Determine a number [0..1] (between 0 and 1) that stores the percentage of duration that has passed
        float u = (Time.time - timeStart) / duration;

        // At what u value should this start fading
        float fadePercent = 1 - (fadeTime / duration);
        if (u > fadePercent)
        { // If it's after the time to start fading then sink into the ground
            float u2 = (u - fadePercent) / (1 - fadePercent);
            // ^ u2 is a number [0..1] for just the fadeTime
            Vector3 loc = pos;
            loc.z = u2 * 2; // move lower over time
            pos = loc;
        }

        if (u > 1)
        { // If this has lived longer than duration
            Destroy(gameObject); //destroy it
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Announce when another object enters the collider
        GameObject go = Utils.FindTaggedParent(other.gameObject);
        if (go == null)
        {
            go = other.gameObject;
        }
        Utils.tr("Flame hit", go.name);
    }

    //TODO: Actually damage the other object

}