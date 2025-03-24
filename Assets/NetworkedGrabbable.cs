using UnityEngine;
using System.Collections;
using Ubiq.Spawning;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

// I should've made this a standalone script a long time ago... oh well
public class NetworkedGrabbable : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public Rigidbody rb;
    public BoxCollider bc;
    public bool collisionsEnabled;

    private bool physicsOwner;
    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private enum CollisionState
    {
        Unset,
        Enabled,
        Disabled
    }

    private struct GrabbableMessage
    {
        public Vector3 position;
        public Quaternion rotation;
        public CollisionState collisions;
    }

    void Awake()
    {
        // Add or retrieve the Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // Add or retrieve the main BoxCollider
        bc = GetComponent<BoxCollider>();
        if (bc == null)
        {
            bc = gameObject.AddComponent<BoxCollider>();
        }

        // Grabbing
        XRGrabInteractable grab = gameObject.GetComponent<XRGrabInteractable>();
        if (!grab)
        {
            grab = gameObject.AddComponent<XRGrabInteractable>();
        }
        grab.selectEntered.AddListener((SelectEnterEventArgs args) =>
        {
            DisablePhysics();
            physicsOwner = true;
        });
        grab.selectExited.AddListener((SelectExitEventArgs args) =>
        {
            EnablePhysics();
            physicsOwner = false;
        });
    }

    void Start()
    {
        context = NetworkScene.Register(this);
        EnablePhysics(); // Nobody is the owner (until they pick it up), so physics will be done client side
    }

    void Update()
    {
        if ((transform.position != lastPosition || transform.rotation != lastRotation))
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;

            if (physicsOwner)
            {
                context.SendJson(new GrabbableMessage
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    collisions = CollisionState.Unset
                });
            }
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<GrabbableMessage>();

        if (!physicsOwner)
        {
            transform.position = msg.position;
            transform.rotation = msg.rotation;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
        if (msg.collisions == CollisionState.Enabled && !collisionsEnabled)
        {
            EnablePhysics();
            physicsOwner = false;
        }
        else if (msg.collisions == CollisionState.Disabled && collisionsEnabled)
        {
            DisablePhysics();
            physicsOwner = false;
        }
    }

    public void DisablePhysics()
    {
        collisionsEnabled = false;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        context.SendJson(new GrabbableMessage
        {
            position = transform.position,
            rotation = transform.rotation,
            collisions = CollisionState.Disabled
        });
    }

    public void EnablePhysics()
    {
        collisionsEnabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        context.SendJson(new GrabbableMessage
        {
            position = transform.position,
            rotation = transform.rotation,
            collisions = CollisionState.Enabled
        });
    }
}
