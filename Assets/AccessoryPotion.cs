using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

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

        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        if (rigidBody == null)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
        }
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        if (triggerCollider == null)
        {
            // Create a separate trigger collider for testing velocity on collision and breaking potion if other some threshold
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
        }

        collisionsEnabled = true;

        if (!collisionsEnabled)
        {
            //DisablePhysics();
        }

        // Grabbing
        var grab = gameObject.GetComponent<XRGrabInteractable>();
        if (!grab)
        {
            grab = gameObject.AddComponent<XRGrabInteractable>();
        }
        grab.selectEntered.AddListener((SelectEnterEventArgs args) =>
        {
            Debug.Log("Potion was selected!");
        });
        grab.selectExited.AddListener((SelectExitEventArgs args) =>
        {
            Debug.Log("Potion was dropped!");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collisionsEnabled)
        {
            float velocity = rigidBody.linearVelocity.magnitude;

            Debug.Log("Potion collided with velocity" + velocity);
            Debug.Log("Head: " + accessories.head + " Neck :" + accessories.neck + " Face: " + accessories.face + " Back: " + accessories.back);

            if (velocity > 0.1)
            {
                Debug.Log("Potion was smashed");
                // Make potion invisible
                // Freeze its position
                // Make the box collider expand outwards for a couple of seconds
                // Set a boolean that switches the behavior inside TriggerEnter s.t. if the Trigger BC intersects an avatar, it puts the accessories on them (use accessoryManager)
                // After the couple of seconds are up, set a boolean in accessoryCauldronButton (TODO: inject reference on spawn) that makes it delete this potion in Update()
            }
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        
    }
}
