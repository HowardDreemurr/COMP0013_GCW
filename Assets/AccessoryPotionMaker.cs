using Ubiq.Spawning;
using UnityEditor.UIElements;
using UnityEngine;
using Ubiq.Messaging;

public class AccessoryPotionMaker : MonoBehaviour
{
    public NetworkId NetworkId { get; set; }
    public BoxCollider triggerCollider;

    [SerializeField] private AccessoryManager accessoryManager;

    public struct Accessories
    {
        public int head;
        public int neck;
        public int back;
        public int face;
    }

    public Accessories accessories;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (accessoryManager == null)
        {
            Debug.LogWarning("AccessoryManager reference not set in inspector! (AccessoryPotionMaker)");
        }

        triggerCollider = gameObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        accessories.head = -1;
        accessories.neck = -1;
        accessories.back = -1;
        accessories.face = -1;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Head: " + accessories.head + " Neck: " + accessories.neck + " Back: " + accessories.back + " Face: " + accessories.face);
    }

    private void OnTriggerEnter(Collider other)
    {
        HatNetworkedObject hat = other.GetComponentInParent<HatNetworkedObject>();
        
        if (hat != null)
        {
            // TODO: Switch on HatNetworkedObject slot enumerator
            accessories.head = hat.idx;
            accessoryManager.headSpawner.Despawn(hat.gameObject);
        }
    }
}
