using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

public class AccessoryPotion : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }

    public bool collisionsEnabled;
    public Rigidbody rigidBody;
    public BoxCollider triggerCollider; // Implements 'splash' logic
    public BoxCollider boxCollider; // Implements actual collisions
    public AccessoryManager accessoryManager; // Injected by design-time button that holds reference to accessoryManager

    private NetworkContext context;

    public struct Accessories
    {
        public int head;
        public int neck;
        public int back;
        public int face;
    }

    public Accessories accessories;

    void Start()
    {
        context = NetworkScene.Register(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
