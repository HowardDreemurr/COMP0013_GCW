using System.Collections.Generic;
using NUnit.Framework;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AccessoryCauldronButton : MonoBehaviour
{
    public NetworkSpawner potionSpawner;
    // This will be a catalogue just containing the potion prefab, assigned in inspector
    // Must implement INetworkSpawnable and potion functionality
    public PrefabCatalogue potionCatalogue; 
    public RoomClient RoomClient { get; private set; }

    [SerializeField] private AccessoryManager accessoryManager;
    [SerializeField] private AccessoryPotionMaker accessoryPotionMaker;
    private XRSimpleInteractable interactable;

    private List<AccessoryPotion> potions = new List<AccessoryPotion>();

    private void Awake()
    {
        RoomClient = GetComponentInParent<RoomClient>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (accessoryPotionMaker == null)
        {
            Debug.LogWarning("AccessoryPotionMaker reference not set in inspector! (AccessorryCauldronButton)");
        }
        if (accessoryManager == null)
        {
            Debug.LogWarning("AccessoryManager reference not set in inspector! (AccessorryCauldronButton)");
        }

        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnSelectEntered);

        var networkScene = NetworkScene.Find(this);

        potionSpawner = new NetworkSpawner(networkScene, RoomClient, potionCatalogue, "ubiq.potion.");
    }

    // Update is called once per frame
    void Update()
    {
        List<AccessoryPotion> toRemove = new List<AccessoryPotion>();
        
        // Mark destoryed potions for deletion via toRemove and notify networkSpawner
        foreach (AccessoryPotion accessoryPotion in potions)
        {
            if (accessoryPotion.destroyed)
            {
                // Despawn the GameObject
                potionSpawner.Despawn(accessoryPotion.gameObject);
                // Mark for removal from list
                toRemove.Add(accessoryPotion);
            }
        }

        // Remove from the source list (removing from the list we are iterating over would be UB)
        foreach (var potion in toRemove)
        {
            potions.Remove(potion);
        }
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
        if (potionCatalogue.prefabs == null || potionCatalogue.prefabs.Count == 0)
        {
            return;
        }

        GameObject potionPrefab = potionCatalogue.prefabs[0];
        GameObject potion = potionSpawner.SpawnWithPeerScope(potionPrefab);
        potion.transform.localPosition = accessoryPotionMaker.transform.localPosition + new Vector3(0, 2, 0); // TODO: Put above cube
        potion.transform.localRotation = Quaternion.identity;

        AccessoryPotion accessoryPotion = potion.GetComponent<AccessoryPotion>();
        if (accessoryPotion != null)
        {
            // Possible race condition
            accessoryPotion.accessories.head = accessoryPotionMaker.accessories.head;
            accessoryPotion.accessories.neck = accessoryPotionMaker.accessories.neck;
            accessoryPotion.accessories.back = accessoryPotionMaker.accessories.back;
            accessoryPotion.accessories.face = accessoryPotionMaker.accessories.face;
            potions.Add(accessoryPotion);
        }
        else
        {
            Debug.LogWarning("accessoryPotion component not found on spawned potion.");
        }
    }
}
