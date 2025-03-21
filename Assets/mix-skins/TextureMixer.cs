using System;
using UnityEngine;
using System.Collections.Generic;

public class TextureMixer : MonoBehaviour
{
    public static TextureMixer Instance { get; private set; }

    private Texture2D currentTexture;
    private int operationNumber;
    private int NumberOfMask;

    [SerializeField]
    public List<Texture2D> textureMask;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
            return;
        }

        DontDestroyOnLoad(gameObject); 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.currentTexture = null;
        this.operationNumber = 0;
        this.NumberOfMask = this.textureMask.Count;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddIngradient(string base64)
    {
        AddIngredient(Base64ToTexture2D(base64));
    }

    public void Reset()
    {
        this.currentTexture = null;
        this.operationNumber = 0;
    }

    public Texture2D GetMixedTexture()
    {
        if (this.currentTexture == null)
        {
            Debug.LogWarning("[TextureMixer](GetMask) Empty mask list!");
            return null;
        }
        return this.currentTexture;

    }

    public void SetMixedTexture(Texture2D texture)
    {
        this.currentTexture = texture;
    }

    public void AddIngredient(Texture2D texture)
    {
        if (this.currentTexture == null)
        {
            this.currentTexture = texture;
        }
        else
        {
            ApplyMaskToTexture(GetMask(this.operationNumber % this.NumberOfMask), texture);
        }

        this.operationNumber += 1;
    }

    private Texture2D GetMask(int index)
    {
        if (textureMask == null || textureMask.Count == 0)
        {
            Debug.LogWarning("[TextureMixer](GetMask) Empty mask list!");
            return null;
        }
        return textureMask[index];
    }

    public void ApplyMaskToTexture(Texture2D mask, Texture2D ingredient)
    {
        if (this.currentTexture == null || mask == null || ingredient == null)
        {
            Debug.LogWarning("[TextureMixer](ApplyMaskToTexture) Empty Texture or Mask!");
            return;
        }

        // Ensure same size of the texture
        if (this.currentTexture.width != mask.width || this.currentTexture.height != mask.height ||
            this.currentTexture.width != ingredient.width || this.currentTexture.height != ingredient.height)
        {
            Debug.LogWarning("[TextureMixer](ApplyMaskToTexture) In-Matched Texture or Mask Size!");
            return;
        }

        // Fetch all pixels
        Color[] basePixels = this.currentTexture.GetPixels();
        Color[] maskPixels = mask.GetPixels();
        Color[] ingredientPixels = ingredient.GetPixels();

        // Iteratively replaces the masked pixels
        for (int i = 0; i < basePixels.Length; i++)
        {
            if (maskPixels[i].r > 0.5f) 
            {
                basePixels[i] = ingredientPixels[i];
            }
        }

        // Update 
        this.currentTexture.SetPixels(basePixels);
    }


    private Texture2D Base64ToTexture2D(string base64)
    {
        byte[] pngData = Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(pngData);
        return texture;
    }

    private string Texture2DToBase64(Texture2D texture)
    {
        byte[] pngData = texture.EncodeToPNG();
        string base64 = Convert.ToBase64String(pngData);
        return base64;
    }

}
