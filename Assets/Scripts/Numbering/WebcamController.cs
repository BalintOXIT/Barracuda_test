using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamController : MonoBehaviour
{
    //Model is declared here
    public GetInterferenceFromModel AI;

    [NonSerialized] public Texture2D m_texture = null;
    [NonSerialized] public WebCamTexture m_webCamTexture;


    // Start is called before the first frame update
    void Start()
    {
        m_webCamTexture = new WebCamTexture();
        //GetComponent<Renderer>().material.mainTexture = _webCamTexture;
        m_webCamTexture.Play();

    }

    // Update is called once per frame
    void Update()
    {

        m_texture = GetTexture2DFromWebcamTexture(m_webCamTexture);
    }


    public static Texture2D GetTexture2DFromWebcamTexture(WebCamTexture webCamTexture)
    {
        // Create new texture2d
        Texture2D tx2d = new Texture2D(webCamTexture.width, webCamTexture.height);
        // Gets all color data from web cam texture and then Sets that color data in texture2d
        tx2d.SetPixels(webCamTexture.GetPixels());
        // Applying new changes to texture2d
        tx2d.Apply();

        Resources.UnloadUnusedAssets(); //Clean the memory or you will make your pc crash

        return tx2d;
    }
    public void PredictNumWithAI(Texture text)
    {
        AI.PredictNumber(text);
    }
}
