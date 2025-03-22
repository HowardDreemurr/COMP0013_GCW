using UnityEngine;

public class AutoDestoryAudios : MonoBehaviour
{
    private AudioSource audios;

    void Start()
    {
        audios = GetComponentInChildren<AudioSource>();
        if (audios && audios.clip)
        {
            audios.Play();
            Destroy(gameObject, audios.clip.length + 0.2f);
        }
        else
        {
            Debug.LogWarning("Missing AudioSource or AudioClip on " + gameObject.name);
            Destroy(gameObject, 1f);
        }
    }
}
