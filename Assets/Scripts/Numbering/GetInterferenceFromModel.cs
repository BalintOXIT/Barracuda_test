using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class GetInterferenceFromModel : MonoBehaviour
{
    //Tensor is not affected my GC Just to know

    public Texture2D texture;

    public NNModel modelAsset;

    private Model _runtimeModel;

    private IWorker _engine;

    private bool _needToRunPrediction = false;

    [Serializable]
    public struct Prediction
    {
        public int predictedValue;
        private float[] predicted;

        public void SetPrediction(Tensor t)
        {
            predicted = t.AsFloats();
            predictedValue = Array.IndexOf(predicted, predicted.Max());
            Debug.Log($"Predicted {predictedValue}");
        }
    }

    public Prediction prediction;
    // Start is called before the first frame update
    void Start()
    {
        _runtimeModel = ModelLoader.Load(modelAsset);
        _engine = WorkerFactory.CreateWorker(_runtimeModel, WorkerFactory.Device.GPU);
        prediction = new Prediction();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.CapsLock))
            _needToRunPrediction = true;

        //Void  
        if (_needToRunPrediction)
            PredictNumber(); 
    }

    public void PredictNumber(Texture text = null)
    {
        if (text == null)
            text = texture;
        //making a tensor out of a grayscale texture
        var channelCount = 1; // 1== grayscale , 3 == color , 4 == color+alpha
        var inputX = new Tensor(text, channelCount);

        Tensor outputY = _engine.Execute(inputX).PeekOutput();
        inputX.Dispose();
        prediction.SetPrediction(outputY);

        _needToRunPrediction = false;
    }
    private void OnDestroy()
    {
        _engine?.Dispose();
    }

    [MenuItem("GameObject/Create Material")]
    static void CreateMaterial()
    {
        // Create a simple material asset

        Material material = new Material(Shader.Find("Specular"));
        AssetDatabase.CreateAsset(material, "Assets/MyMaterial.mat");

        // Print the path of the created asset
        Debug.Log(AssetDatabase.GetAssetPath(material));
    }
}
