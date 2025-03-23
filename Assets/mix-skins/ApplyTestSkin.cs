using System.Collections;
using System.IO;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Random = UnityEngine.Random;

public class ApplyTestSkin : MonoBehaviour
{
    public GameObject prefab;

    private XRSimpleInteractable interactable;
    private RoomClient roomClient;
    private AvatarManager avatarManager;
    private TextureMixer textureMixer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Connect up the event for the XRI button.
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(Interactable_SelectEntered);

        var networkScene = NetworkScene.Find(this);
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();

        textureMixer = TextureMixer.Instance;

        if (textureMixer == null)
        {
            Debug.LogError("TextureMixer Instance Not Found!");
        }

    }

    private void OnDestroy()
    {
        // Cleanup the event for the XRI button so it does not get called after
        // we have been destroyed.
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
        }
    }

    private void Interactable_SelectEntered(SelectEnterEventArgs arg0)
    {
        // The button has been pressed.

        // Change the local avatar prefab to the default one, because we have
        // a few costumes for that avatar bundled with Ubiq. The AvatarManager
        // will do the work of letting other peers know about the prefab change.
        avatarManager.avatarPrefab = prefab;
        // Also, set the costume to a new, random one. We use a coroutine to
        // wait one frame to allow the AvatarManager time to spawn the new
        // prefab.
        StartCoroutine(SetSkin());
    }

    private IEnumerator SetSkin()
    {
        while (true)
        {
            if (!avatarManager)
            {
                // Yield break ends the coroutine.
                yield break;
            }

            var avatar = avatarManager.FindAvatar(roomClient.Me);
            if (avatar)
            {
                var textured = avatar.GetComponentInChildren<TexturedAvatar>();
                if (textured)
                {
                    //Texture2D newTexture = LoadTextureFromFile(Application.dataPath + "/avatar-example/my_texture_1.png");
                    Texture2D newTexture = textured.GetTexture();
                    if (textureMixer != null)
                    {
                        textureMixer.AddIngredient(newTexture);
                    }

                    // End the coroutine.
                    yield break;
                }
            }

            // Yield return null pauses the coroutine until the next frame. We
            // wait a few frames to allow the prefab to be spawned and to
            // initialise itself.
            yield return null;
            yield return null;
        }
    }

    private Texture2D LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Texture file not found: " + filePath);
            return null;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2); 
        if (texture.LoadImage(fileData)) 
        {
            return texture;
        }

        Debug.LogError("Failed to load texture from file: " + filePath);
        return null;
    }

}
