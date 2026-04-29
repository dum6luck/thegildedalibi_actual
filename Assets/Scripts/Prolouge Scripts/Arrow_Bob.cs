using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow_Bob : MonoBehaviour
{
    public float speed = 5f;
    public float strength = 5f;

    private float startY;

    void Start()
    {
        // Remember the original position so we bob around it
        startY = transform.localPosition.y;
    }

    void Update()
    {
        // Calculate the new Y position using a Sine wave
        float newY = startY + Mathf.Sin(Time.time * speed) * strength;

        // Apply it to the local position
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }
}