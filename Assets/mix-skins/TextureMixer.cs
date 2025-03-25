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


    public string AddIngradient(int operationNumer, Texture2D ingredient, string oldblob)
    {
        Texture2D mixedTexture;
        if (oldblob == null)
        {
            Debug.Log("(AddIngradient) oldblob == null");
            mixedTexture = ingredient;
        }
        else
        {
            Debug.Log("(AddIngradient) oldblob != null");
            mixedTexture = ApplyMaskToTexture(Base64ToTexture2D(oldblob), GetMask(operationNumer % this.NumberOfMask), ingredient);
        }

        return Texture2DToBase64(mixedTexture);
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
            this.currentTexture = ApplyMaskToTexture(this.currentTexture, GetMask(this.operationNumber % this.NumberOfMask), texture);
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

    public Texture2D ApplyMaskToTexture(Texture2D basedTexture, Texture2D mask, Texture2D ingredient)
    {
        if (basedTexture == null || mask == null || ingredient == null)
        {
            Debug.LogWarning("[TextureMixer](ApplyMaskToTexture) Empty Texture or Mask!");
            return null;
        }

        // Ensure same size of the texture
        if (basedTexture.width != mask.width || basedTexture.height != mask.height ||
            basedTexture.width != ingredient.width || basedTexture.height != ingredient.height)
        {
            Debug.LogWarning("[TextureMixer](ApplyMaskToTexture) In-Matched Texture or Mask Size!");
            return null;
        }

        // Fetch all pixels
        Color[] basePixels = basedTexture.GetPixels();
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
        Texture2D resultTexture = new Texture2D(basedTexture.width, basedTexture.height, basedTexture.format, false);
        resultTexture.SetPixels(basePixels);
        return resultTexture;
    }


    public Texture2D Base64ToTexture2D(string base64)
    {
        byte[] pngData = Convert.FromBase64String(base64);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(pngData);
        return texture;
    }

    public string Texture2DToBase64(Texture2D texture)
    {
        byte[] pngData = texture.EncodeToPNG();
        string base64 = Convert.ToBase64String(pngData);
        return base64;
    }

}
