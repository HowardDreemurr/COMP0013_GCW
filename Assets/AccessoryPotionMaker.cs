using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

public class AccessoryPotionMaker : MonoBehaviour
{
    public BoxCollider triggerCollider;

    [SerializeField] private AccessoryManager accessoryManager;
    private float timer = 0f;
    private float logInterval = 1f;
    private NetworkContext context;

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

        context = NetworkScene.Register(this);

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

    }

    private void OnTriggerEnter(Collider other)
    {
        HatNetworkedObject hat = other.GetComponentInParent<HatNetworkedObject>();
        
        if (hat != null)
        {
            // TODO: Switch on HatNetworkedObject slot enumerator
            accessories.head = hat.idx;
            accessoryManager.headSpawner.Despawn(hat.gameObject);

            context.SendJson(new Accessories
            {
                head = accessories.head,
                neck = accessories.neck,
                back = accessories.back,
                face = accessories.face
            });
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Accessories>();

        accessories.head = msg.head;
        accessories.neck = msg.neck;
        accessories.back = msg.back;
        accessories.face = msg.face;

        Debug.Log("Head: " + accessories.head + " Neck: " + accessories.neck + " Back: " + accessories.back + " Face: " + accessories.face);
    }
}
