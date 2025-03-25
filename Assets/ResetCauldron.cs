using System.Collections;
using NUnit.Framework.Interfaces;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ResetCauldron : MonoBehaviour
{
    [SerializeField] private AccessoryPotionMaker accessoryPotionMaker;
    private XRSimpleInteractable interactable;
    private RoomClient roomClient;
    private AvatarManager avatarManager;

    private void Start()
    {
        if (accessoryPotionMaker == null)
        {
            Debug.LogWarning("(ResetCauldron) AccessoryPotionMaker == null");
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
        accessoryPotionMaker.ResetState();
    }
}
