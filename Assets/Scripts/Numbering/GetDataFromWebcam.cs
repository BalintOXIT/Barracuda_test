using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;

public class GetDataFromWebcam : MonoBehaviour
{
    public WebcamController Controller;
    private Texture2D _texture;
    private WebCamTexture _webcamtexture;
    [Header("Script Type")]
    public ControllerType controllerType;

    public enum ControllerType {Base, PhotoResize, OpenCV, TesseRact }

    //Photo with Resize
    [Header("PhotoTakeForAi")]
    private bool _UseButtonForCapture = false;

    //OPENCV
    [Header("OPENCV")]
    private bool _UseOpencv;
    private CascadeClassifier cascade;
    private OpenCvSharp.Rect _myFace;

    //TESSERACT
    [Header("TESSERACT")]
    public float TimeBeetweenCaptures;
    public GameObject TestGO;
    private bool _UseTesseract;
    private TesseractDriver _tesseractDriver;
    public Texture2D textureBaseType;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetTextureFromWebCam());       
    }

    private IEnumerator SetTextureFromWebCam()
    {
        yield return new WaitUntil(() => Controller.m_texture != null);

        GetComponent<Renderer>().material.mainTexture = Controller.m_webCamTexture;
        _texture = Controller.m_texture;
        _webcamtexture = Controller.m_webCamTexture;
        
        //INIT
        cascade = new CascadeClassifier(Application.dataPath + "\\Models_haarcascade\\haarcascade_frontalface_default.xml");

        _UseTesseract = false;
        _UseOpencv = false;
        _UseButtonForCapture = false;
        switch (controllerType)
        {
            case ControllerType.PhotoResize:
                _UseButtonForCapture = true;
                break;
            case ControllerType.OpenCV:
                _UseOpencv = true;
                break;
            case ControllerType.TesseRact:
                //Use at enter
                _tesseractDriver = new TesseractDriver();
                break;
            default:
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (_webcamtexture == null) return;


        if (_UseButtonForCapture) SetTexture();
        else _texture = Controller.m_texture;

        if (_UseOpencv) OpenCV_FaceRecognization();

        if (Input.GetKeyDown(KeyCode.KeypadEnter) && controllerType == ControllerType.TesseRact)
            _UseTesseract = true;

        if (_UseTesseract) StartCoroutine(TesseRactTextDetection(TimeBeetweenCaptures));
    }

    //PHOTOWITHRESIZE
    private void SetTexture()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        { 
            _texture = FilteredDownscale(Controller.m_texture, 28, 28);
            GetComponent<Renderer>().material.mainTexture = _texture;
            Controller.PredictNumWithAI(_texture);
        }

    }
    public Texture2D FilteredDownscale(Texture2D a_Source, int a_NewWidth, int a_NewHeight, TextureFormat format = TextureFormat.ARGB32)
    {
        // Keep the last active RT
        RenderTexture _LastActiveRT = RenderTexture.active;

        // Start by halving the source dimensions
        int _Width = a_Source.width / 2;
        int _Height = a_Source.height / 2;

        // Cap to the target dimensions.
        // This could be done with Mathf.Max() but that wouldn't take into account aspect ratio.
        if (_Width < a_NewWidth || _Height < a_NewHeight)
        {
            _Width = a_NewWidth;
            _Height = a_NewHeight;
        }

        // Create a temporary downscaled RT
        RenderTexture _Tmp1 = RenderTexture.GetTemporary(_Width, _Height, 0, RenderTextureFormat.ARGB32);

        // Copy the source into our temporary RT
        Graphics.Blit(a_Source, _Tmp1);

        // Loop until our target dimensions have been reached
        while (_Width > a_NewWidth && _Height > a_NewHeight)
        {
            // Keep halving our current dimensions
            _Width /= 2;
            _Height /= 2;

            // And match our target dimensions once small enough
            if (_Width < a_NewWidth || _Height < a_NewHeight)
            {
                _Width = a_NewWidth;
                _Height = a_NewHeight;
            }

            // Downscale again into a smaller RT
            RenderTexture _Tmp2 = RenderTexture.GetTemporary(_Width, _Height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(_Tmp1, _Tmp2);

            // Swap our temporary RTs and release the oldest one
            (_Tmp1, _Tmp2) = (_Tmp2, _Tmp1);
            RenderTexture.ReleaseTemporary(_Tmp2);
        }

        // At this point _Tmp1 should hold our fully downscaled image,
        // so set it as the active RT
        RenderTexture.active = _Tmp1;

        // Create a new texture of the desired dimensions and copy our data into it
        Texture2D _Tex = new Texture2D(a_NewWidth, a_NewHeight, format, false);
        _Tex.ReadPixels(new UnityEngine.Rect(0, 0, a_NewWidth, a_NewHeight), 0, 0);
        _Tex.Apply();

        // Reset the active RT and release our last temporary copy
        RenderTexture.active = _LastActiveRT;
        RenderTexture.ReleaseTemporary(_Tmp1);

        return _Tex;
    }

    //TESSERACT
    private IEnumerator TesseRactTextDetection(float waittime)
    {
        _UseTesseract = false;
        _tesseractDriver.Setup(OnSetupCompleteRecognize);
        yield return new WaitForSeconds(waittime);
        //_UseTesseract = true;
    }
    private void OnSetupCompleteRecognize()
    {
        Texture2D text = new Texture2D(_texture.width, _texture.height);
        text.SetPixels(_texture.GetPixels());
        text.Apply();
        _tesseractDriver.Recognize(text);
        SetImageDisplay();
    }
    private void SetImageDisplay()
    {
        Texture2D TesseractTexture = _tesseractDriver.GetHighlightedTexture();
        List<UnityEngine.Rect> DaTas = _tesseractDriver.AllRect();
        SetTextureFromData(DaTas, _texture);
        GetComponent<Renderer>().material.mainTexture = TesseractTexture;    
    }

    private void SetTextureFromData(List<UnityEngine.Rect> daTas,Texture2D text)
    {
        if (daTas.Count < 1) return;

        int counter = 0;
        foreach (var data in daTas)
        {
            Color[] colors = text.GetPixels((int)data.x, (int)data.y, (int)data.width, (int)data.height);

            //Create texture
            Texture2D DownscaledTexture = new Texture2D((int)data.width, (int)data.height);
            DownscaledTexture.SetPixels(colors);
            DownscaledTexture.Apply();
            DownscaledTexture = FilteredDownscale(DownscaledTexture, 28, 28, TextureFormat.R8);

            //Test - save
            counter++;
            byte[] _bytes = DownscaledTexture.EncodeToPNG();
            string fulllPath = @"C:\Program Files\TESZT_ANDROID_UNITY\Barracuda_test\Assets\imgs\test"+ counter.ToString() + ".png" ;
            System.IO.File.WriteAllBytes(fulllPath, _bytes);
            
            //AI RECOG
            Controller.AI.PredictNumber(DownscaledTexture);
            //Visualize
            TestGO.GetComponent<RawImage>().texture = DownscaledTexture;
        }
    }
    //OPEN CV
    private void OpenCV_FaceRecognization()
    {
        Mat frame = OpenCvSharp.Unity.TextureToMat(_webcamtexture);
        //To Find
        FindNewFace(frame);
        //To Display
        DisPlay(frame);
    }
    private void FindNewFace(Mat frame)
    {
        var faces = cascade.DetectMultiScale(frame, 1.1, 2, HaarDetectionType.ScaleImage);

        if (faces.Length >= 1)
        {
           // Debug.Log(faces[0].Location);
            _myFace = faces[0];
        }

    }
    private void DisPlay(Mat frame)
    {
        if (_myFace != null)
            frame.Rectangle(_myFace, new Scalar(250, 0, 0), 2);

        Texture newtex = OpenCvSharp.Unity.MatToTexture(frame);
        GetComponent<Renderer>().material.mainTexture = newtex; 

    }
}
