using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Ubiq.Avatars;

public class RemoteAvatarInteractableAttacher : MonoBehaviour
{
    private AvatarManager avatarManager;

    private void Start()
    {
        avatarManager = FindObjectOfType<AvatarManager>();
        if (avatarManager != null)
        {
            avatarManager.OnAvatarCreated.AddListener(OnAvatarCreated);
        }
        else
        {
            Debug.LogWarning("No AvatarManager found in scene.");
        }
    }

    private void OnDestroy()
    {
        if (avatarManager != null)
        {
            avatarManager.OnAvatarCreated.RemoveListener(OnAvatarCreated);
        }
    }

    private void OnAvatarCreated(Ubiq.Avatars.Avatar avatar)
    {
        // Only modify remote avatars.
        if (!avatar.IsLocal)
        {
            Debug.Log("Adding interactable to " + avatar.name + "...");
            // Adjust the hierarchy path to your head object.
            Transform head = avatar.transform.Find("Body/Floating_Head");
            if (head == null)
            {
                Debug.LogWarning("Could not find head for avatar: " + avatar.name);
                return;
            }

            // Ensure an XR Simple Interactable is attached.
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable = head.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (interactable == null)
            {
                Debug.Log("\tAdding XRSimpleInteractable...");
                interactable = head.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            }

            // Add a BoxCollider (or use an existing one) and set it as trigger.
            BoxCollider boxCollider = head.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.Log("\tAdding head BoxCollider...");
                boxCollider = head.gameObject.AddComponent<BoxCollider>();
            }
            boxCollider.isTrigger = false; // Unfortunately it needs to have physical collisions to be clicked on...

            // Add the BoxCollider to the interactable's colliders list if not already there
            if (!interactable.colliders.Contains(boxCollider))
            {
                Debug.Log("\tAdding BoxCollider to XRSimpleInteractable colliders...");
                interactable.colliders.Add(boxCollider);
            }

            // Attach the XRPokeFilter and configure it.
            XRPokeFilter pokeFilter = head.GetComponent<XRPokeFilter>();
            if (pokeFilter == null)
            {
                Debug.Log("\tAdding XRPokeFilter...");
                pokeFilter = head.gameObject.AddComponent<XRPokeFilter>();
            }
            pokeFilter.pokeInteractable = interactable;
            pokeFilter.pokeCollider = boxCollider;

            // Attach the XRPokeFollowAffordance if you want visual feedback.
            XRPokeFollowAffordance pokeFollowAffordance = head.GetComponent<XRPokeFollowAffordance>();
            if (pokeFollowAffordance == null)
            {
                Debug.Log("\tAdding XRPokeFollowAffordance...");
                pokeFollowAffordance = head.gameObject.AddComponent<XRPokeFollowAffordance>();
            }
            // For simplicity, use the head transform as the follow transform.
            pokeFollowAffordance.pokeFollowTransform = head;

            // Finally, add your interaction listener.
            Debug.Log("\tAdding selectEntered listener...");
            interactable.selectEntered.AddListener(OnHeadSelectEntered);

            // I don't get Unity. Why does this help
            interactable.enabled = false;
            interactable.enabled = true;
        }
    }

    private void OnHeadSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("Remote avatar head selected");
        // Place your custom interaction code here.
    }
}
