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

    public GameObject ParticlePrefab;
    public GameObject AudioPrefab;

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
        Debug.Log($"{name} on {UnityEngine.SystemInfo.deviceName} has ID {context.Id}");

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
            switch (hat.slot)
            {
                case AccessorySlot.Head:
                    accessories.head = hat.idx;
                    accessoryManager.headSpawner.Despawn(hat.gameObject);
                    break;
                case AccessorySlot.Neck:
                    accessories.neck = hat.idx;
                    accessoryManager.neckSpawner.Despawn(hat.gameObject);
                    break;
                case AccessorySlot.Back:
                    accessories.back = hat.idx;
                    accessoryManager.backSpawner.Despawn(hat.gameObject);
                    break;
                case AccessorySlot.Face:
                    accessories.face = hat.idx;
                    accessoryManager.faceSpawner.Despawn(hat.gameObject);
                    break;
                default:
                    return;
            }

            context.SendJson(new Accessories
            {
                head = accessories.head,
                neck = accessories.neck,
                back = accessories.back,
                face = accessories.face
            });

            SpawnEffects(ParticlePrefab, transform.position);
            SpawnEffects(AudioPrefab, transform.position);
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

    private void SpawnEffects(GameObject prefab, Vector3 position)
    {
        if (prefab)
        {
            var instance = NetworkSpawnManager.Find(this).SpawnWithPeerScope(prefab);

            instance.transform.position = position;
        }
    }
}
