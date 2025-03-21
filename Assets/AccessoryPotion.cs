using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using NUnit.Framework;
using System.Collections.Generic;

public class AccessoryPotion : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }

    public bool collisionsEnabled;
    public Rigidbody rigidBody;
    public BoxCollider triggerCollider; // Implements 'splash' logic
    public BoxCollider boxCollider; // Implements actual collisions

    public float expansionDuration = 5f;
    public float expansionSize = 10f;
    private float timer = 0f;

    private bool smashed = false;
    private bool expanding = false;
    public bool destroyed = false;

    private List<string> affectedAvatars = new List<string>();

    private Vector3 initialSize;

    public AccessoryManager accessoryManager; // Injected by design-time button that holds reference to accessoryManager

    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private bool owner = false;
    private bool isOwnedBySomeoneElse = false;

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
            // TODO: Check that this is only called on the client which grabs the potion and not on all clients when someone in the room grabs the potion
            owner = true;
            context.SendJson(new TransformMessage
            {
                position = transform.position,
                rotation = transform.rotation,
                smashed = smashed,
                isOwnedBySomeoneElse = true
            });
        });
        grab.selectExited.AddListener((SelectExitEventArgs args) =>
        {
            owner = false;
            context.SendJson(new TransformMessage
            {
                position = transform.position,
                rotation = transform.rotation,
                smashed = smashed,
                isOwnedBySomeoneElse = false
            });
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (destroyed) return; // We're waiting for the NetworkSpawner to despawn this object

        if (transform.position != lastPosition || transform.rotation != lastRotation)
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;

            context.SendJson(new TransformMessage
            {
                position = transform.position,
                rotation = transform.rotation,
                smashed = smashed,
                isOwnedBySomeoneElse = isOwnedBySomeoneElse & owner
            });
        }

        if (smashed && !expanding)
        {
            // The potion smashed on someone elses client but not our one - take their word for it
            SmashPotion();
        }

        if (expanding)
        {
            timer += Time.deltaTime;
            // Stop expanding after duration is reached
            if (timer >= expansionDuration)
            {
                Debug.Log("Potion stopped expanding");
                destroyed = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collisionsEnabled)
        {
            float velocity = rigidBody.linearVelocity.magnitude;

            Debug.Log("Potion collided with velocity " + velocity);
            Debug.Log("Head: " + accessories.head + " Neck :" + accessories.neck + " Face: " + accessories.face + " Back: " + accessories.back);

            if (velocity > 0.1)
            {
                SmashPotion();
            }
        }
        if (expanding)
        {
            Debug.Log($"OnTriggerEnter called by collider: {other.gameObject.name}");
            Ubiq.Avatars.Avatar avatar = other.GetComponentInParent<Ubiq.Avatars.Avatar>();

            if (avatar != null && !affectedAvatars.Contains(avatar.name))
            {
                Debug.Log("Potion touched avatar " + avatar.name);

                HatNetworkedObject head = accessoryManager.SpawnHat(accessories.head, false, AccessorySlot.Head);
                HatNetworkedObject back = accessoryManager.SpawnHat(accessories.back, false, AccessorySlot.Back);
                HatNetworkedObject face = accessoryManager.SpawnHat(accessories.face, false, AccessorySlot.Face);

                if (head != null)
                {
                    head.AttachHat(avatar, AccessorySlot.Head);
                }
                if (back != null)
                {
                    back.AttachHat(avatar, AccessorySlot.Back);
                }
                if (face != null)
                {
                    face.AttachHat(avatar, AccessorySlot.Face);
                }

                affectedAvatars.Add(avatar.name); // don't apply to this avatar again or else it just freezes the game
            }
        }
    }

    private void SmashPotion()
    {
        Debug.Log("Potion was smashed");

        if (rigidBody != null)
        {
            rigidBody.isKinematic = true;
        }
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // renderer.enabled = false;
        }
        if (triggerCollider != null)
        {
            initialSize = triggerCollider.size;
            expanding = true; // Start expansion
        }
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.enabled = false;
            Destroy(grab);
        }

        boxCollider.enabled = false;
        collisionsEnabled = false;

        triggerCollider.size = initialSize * expansionSize;

        // NOTE: If this does not synchronize potion smashing well, put back the message here
    }

    void OnDrawGizmos()
    {
        if (triggerCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(triggerCollider.center, triggerCollider.size);
        }
    }

    private struct TransformMessage
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool smashed;
        public bool isOwnedBySomeoneElse;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<TransformMessage>();

        if (!owner && msg.isOwnedBySomeoneElse)
        {
            // Someone picked up the potion (not us)
            isOwnedBySomeoneElse = true;
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
        }
        else if (isOwnedBySomeoneElse && !msg.isOwnedBySomeoneElse)
        {
            // Someone dropped the potion (not us)
            isOwnedBySomeoneElse = false;
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
        }

        transform.position = msg.position;
        transform.rotation = msg.rotation;

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        if (msg.smashed && !expanding)
        {
            smashed = true;
            // If !msg.smashed, just keep it as is; we want to hold true until we finish calling SmashPotion locally
        }
    }
}
