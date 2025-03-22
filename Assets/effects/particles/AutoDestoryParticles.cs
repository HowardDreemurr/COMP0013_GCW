using UnityEngine;

public class AutoDestroyParticles : MonoBehaviour
{
    private ParticleSystem particles;

    void Start()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        particles.Play();
        Destroy(gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
    }
}