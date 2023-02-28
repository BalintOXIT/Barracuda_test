using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOnTexture : MonoBehaviour
{
    public Texture2D baseTexture;

    public GetInterferenceFromModel AI;
    // Update is called once per frame
    void Update()
    {
        DoMouseDrawing();

        if(Input.GetKeyDown(KeyCode.LeftShift))
            AI.PredictNumber(baseTexture);

    }
    /// <summary>
    /// Allows drawing to the texture with a mouse
    /// </summary>
    /// <exception cref="Exception" ></exception>
    private void DoMouseDrawing()
    {
        if (Camera.main == null)
        {
            throw new Exception("Cannot find main camera");
        }

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1)) return;

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(mouseRay, out hit)) return;
        if (hit.collider.transform != transform) return;


        Vector2 pixelUV = hit.textureCoord;

        pixelUV.x *= baseTexture.width;
        pixelUV.y *= baseTexture.height;

        Color colorToSet = Input.GetMouseButton(0) ? Color.white : Color.black;

        baseTexture.SetPixel((int)pixelUV.x, (int)pixelUV.y, colorToSet);
        baseTexture.Apply();
    }
}
