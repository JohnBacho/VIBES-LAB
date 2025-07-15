using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TalkingAmbience : MonoBehaviour
{
    private AudioSource audioSource;
    private Vector3 originalPosition;
    private float moveTimer;
    private AudioLowPassFilter lowPass;

    [Header("Movement Settings")]
    public float moveRadius = 0.4f;         
    public float moveInterval = 4f;

    [Header("Muffling Settings")]
    public float muffledCutoffFrequency = 1200f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
        originalPosition = transform.position;
        moveTimer = moveInterval;

        lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = muffledCutoffFrequency;
        lowPass.lowpassResonanceQ = 1f;
    }

    void Update()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * moveRadius;
            randomOffset.y = 0; // keep it horizontal
            transform.position = originalPosition + randomOffset;

            moveTimer = moveInterval + Random.Range(-1f, 1.5f);
        }
    }
}
