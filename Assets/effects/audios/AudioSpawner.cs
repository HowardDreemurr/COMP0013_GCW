using UnityEngine;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AudioSpawner : MonoBehaviour
{
    public GameObject audioPrefab;
    private XRSimpleInteractable interactable;

    private void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(SpawnAudio);
    }

    private void OnDestroy()
    {
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(SpawnAudio);
        }
    }

    private void SpawnAudio(SelectEnterEventArgs args)
    {
        var instance = NetworkSpawnManager.Find(this).SpawnWithPeerScope(audioPrefab);

        instance.transform.position = transform.position;
        instance.transform.rotation = transform.rotation;
    }
}
