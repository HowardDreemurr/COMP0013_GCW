using System.Collections;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RemoveAccessories : MonoBehaviour
{
    [SerializeField] private AccessoryManager accessoryManager;
    private XRSimpleInteractable interactable;
    private RoomClient roomClient;
    private AvatarManager avatarManager;

    private void Start()
    {
        if (accessoryManager == null)
        {
            Debug.LogWarning("(RemoveAccessories) AccessoryManager == null");
        }

        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(Interactable_SelectEntered);

        var networkScene = NetworkScene.Find(this);
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
    }

    private void OnDestroy()
    {
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
        }
    }

    private void Interactable_SelectEntered(SelectEnterEventArgs arg0)
    {
        Ubiq.Avatars.Avatar avatar = avatarManager.FindAvatar(roomClient.Me);

        FloatingAvatar floatingAvatar = avatar.GetComponentInChildren<FloatingAvatar>();
        if (floatingAvatar == null)
        {
            Debug.LogWarning("FloatingAvatar component not found on avatar");
            return;
        }

        Transform headTransform = floatingAvatar.head;
        if (headTransform == null)
        {
            Debug.LogWarning("Head transform not found on avatar");
        }

        Transform torsoTransform = floatingAvatar.torso;
        if (torsoTransform == null)
        {
            Debug.LogWarning("Torso transform not found on avatar");
        }

        Transform existingHat = headTransform.Find("NetworkHead");
        if (existingHat != null)
        {
            Debug.Log("Removing headwear...");
            accessoryManager.headSpawner.Despawn(existingHat.gameObject);
        }

        Transform existingNeck = headTransform.Find("NetworkNeck");
        if (existingNeck != null)
        {
            Debug.Log("Removing neckwear...");
            accessoryManager.neckSpawner.Despawn(existingNeck.gameObject);
        }

        Transform existingBack = torsoTransform.Find("NetworkBack");
        if (existingBack != null)
        {
            Debug.Log("Removing backwear...");
            accessoryManager.backSpawner.Despawn(existingBack.gameObject);
        }

        Transform existingFace = headTransform.Find("NetworkFace");
        if (existingFace != null)
        {
            Debug.Log("Removing facewear...");
            accessoryManager.faceSpawner.Despawn(existingFace.gameObject);
        }
    }
}
