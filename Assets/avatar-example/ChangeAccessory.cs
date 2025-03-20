using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChangeAccessory : MonoBehaviour
{
    private XRSimpleInteractable interactable;
    private RoomClient roomClient;
    private AvatarManager avatarManager;
    private AccessoryManager accessoryManager;

    public AccessorySlot slot; // Determines which slot this button will spawn

    private void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSelectEntered);

        var networkScene = NetworkScene.Find(this);
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
        accessoryManager = networkScene.GetComponentInChildren<AccessoryManager>();
    }

    private void OnDestroy()
    {
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("Entering OnSelectEntered");

        var avatar = avatarManager.FindAvatar(roomClient.Me);
        if (avatar)
        {
            accessoryManager.AttachRandomHat(avatar, slot);
        }
        else
        {
            Debug.LogWarning("Local avatar not found!");
        }

        Debug.Log("Leaving OnSelectEntered");
    }
}
