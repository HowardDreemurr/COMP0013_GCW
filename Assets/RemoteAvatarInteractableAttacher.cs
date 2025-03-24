using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Avatars;

public class RemoteAvatarInteractableAttacher : MonoBehaviour
{
    private AvatarManager avatarManager;

    void Start()
    {
        avatarManager = FindObjectOfType<AvatarManager>();
        if (avatarManager != null)
        {
            avatarManager.OnAvatarCreated.AddListener(OnAvatarCreated);
        }
    }

    void OnDestroy()
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
            // Attempt to locate the head by name in the hierarchy.
            Transform head = avatar.transform.Find("Body/Floating_Head");
            if (head != null)
            {
                // Add the interactable if it's not already present.
                var interactable = head.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                if (interactable == null)
                {
                    interactable = head.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                }

                // Add your custom event listener.
                interactable.selectEntered.AddListener(OnHeadSelectEntered);
            }
            else
            {
                Debug.LogWarning("Could not find the Floating_Head in the remote avatar hierarchy.");
            }
        }
    }

    private void OnHeadSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("Remote avatar head selected");
        // Insert your custom function call here.
    }
}
