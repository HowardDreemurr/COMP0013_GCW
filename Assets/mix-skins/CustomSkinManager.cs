using UnityEngine;
using Ubiq.Rooms;
using Ubiq.Avatars;
using Ubiq.Messaging;
using System;
using System.Collections;

public class CustomSkinManager : MonoBehaviour
{

    private RoomClient roomClient;
    private AvatarManager avatarManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Fetch RoomClient and AvatarManager.
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        avatarManager = NetworkScene.Find(this).GetComponentInChildren<AvatarManager>();

        // Monitor client events.
        roomClient.OnPeerAdded.AddListener(OnPeerAdded);
        roomClient.OnPeerUpdated.AddListener(OnPeerUpdated);

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    // Check for base64 skin when new player entered.
    private void OnPeerAdded(IPeer peer)
    {
        Debug.Log($"[CSM] Player {peer.uuid} Entered.");

        // Apply custom skin if base64 string exists.
        StartCoroutine(ApplyCustomIfAvailable(peer));
    }


    // Check for base64 skin when new player updated.
    private void OnPeerUpdated(IPeer peer)
    {
        Debug.Log($"[CSM] Player {peer.uuid} Updated.");

        // Apply custom skin if base64 string exists.
        StartCoroutine(ApplyCustomIfAvailable(peer));
    }

    // Apply custom skin if based64 exists.
    private IEnumerator ApplyCustomIfAvailable(IPeer peer)
    {
        if (!string.IsNullOrWhiteSpace((peer["ubiq.avatar.texture.base64"])))
        {
            string base64Data = peer["ubiq.avatar.texture.base64"];
            if (!string.IsNullOrEmpty(base64Data))
            {
                while (true)
                {
                    if (!avatarManager)
                    {
                        yield break;
                    }

                    // Find avatar by AvatarManager.
                    var avatar = avatarManager.FindAvatar(peer);
                    if (avatar)
                    {
                        var textured = avatar.GetComponentInChildren<TexturedAvatar>();
                        if (textured)
                        {
                            Debug.Log($"[CSM] Player {peer.uuid} avatar found, applying...");
                            Texture2D texture = ConvertBase64ToTexture(base64Data);
                            textured.SetTexture(texture);
                            Debug.Log($"[CSM] Player {peer.uuid} custom skin applied.");

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
        }
    }


    // Convert base64 string to Texture2D
    private Texture2D ConvertBase64ToTexture(string base64)
    {
        byte[] pngData = Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(pngData);
        return texture;
    }

}
