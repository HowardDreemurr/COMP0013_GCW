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

        // Get the local avatar using the RoomClient's peer.
        var avatar = avatarManager.FindAvatar(roomClient.Me);
        if (avatar)
        {
            accessoryManager.AttachRandomHat(avatar);
        }
        else
        {
            Debug.LogWarning("Local avatar not found!");
        }

        Debug.Log("Leaving OnSelectEntered");
    }
}
