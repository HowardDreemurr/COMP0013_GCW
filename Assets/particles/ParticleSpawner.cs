using UnityEngine;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ParticleSpawner : MonoBehaviour
{
    public GameObject particlePrefab;
    public Vector3 position; 
    public Quaternion rotation;
    private XRSimpleInteractable interactable;

    private void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(SpawnParticle);
    }

    private void OnDestroy()
    {
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(SpawnParticle);
        }
    }

    private void SpawnParticle(SelectEnterEventArgs args)
    {
        var spawnManager = NetworkSpawnManager.Find(this);

        var instance = spawnManager.SpawnWithPeerScope(particlePrefab);

        instance.transform.position = transform.position;
        instance.transform.rotation = transform.rotation;
    }
}


