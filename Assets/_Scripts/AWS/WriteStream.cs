
//#define DEBUG_Webcam
#define AR_camera

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utils;


[RequireComponent(typeof(AWSClient))]
[RequireComponent(typeof(AWSConfig))]
public class WriteStream : MonoBehaviour
{
    
#region Webcam_fields
#if DEBUG_Webcam
    private bool camAvailable;
    private bool isInitialized = false;
    private WebCamTexture webCam;
    private Texture defaultBackground;

    Color32[] data;

    public RawImage background;
    public AspectRatioFitter fit;
#endif
#endregion


    public int captureRate;

    private int frames = 0;
    private RenderTexture arCamTexture;
    Texture2D tex;


    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    static private AWSClient _C;

    [System.Serializable]
    struct FramePackage
    {
        public System.DateTime ApproximateCaptureTime;
        public int FrameCount;
        public byte[] ImageBytes;


        public FramePackage(System.DateTime time, int count, byte[] data)
        {
            this.ApproximateCaptureTime = time;
            this.FrameCount = count;
            this.ImageBytes = data;
        }

        public string serialize(){
            return JsonConvert.SerializeObject(this);
        }

    }




    // Use this for initialization
    void Start()
    {
        _C = GetComponent<AWSClient>();
        Camera arCam = this.GetComponent<Camera>();
        arCamTexture = new RenderTexture(arCam.pixelWidth, arCam.pixelHeight, 16, RenderTextureFormat.ARGB32);
        arCamTexture.Create();
        arCam.targetTexture = arCamTexture;

        tex = new Texture2D(arCamTexture.width, arCamTexture.height);

    #region Webcam
#if DEBUG_Webcam
        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0){
            Debug.Log("No camera detected");
            return;
        }

        for (int i = 0; i < devices.Length; i++){
            if(devices[i].isFrontFacing){
                webCam = new WebCamTexture(devices[i].name, Screen.width/100, Screen.height/100);
            } 
        }

        if(webCam == null){
            Debug.Log("Unable to find webcam");
            return;
        }

        webCam.Play();
        background.texture = webCam;
        camAvailable = true;
#endif
    #endregion
    
    }


    void Update()
    {
        frames++;

        #region webcam
#if DEBUG_Webcam

        Texture2D t = new Texture2D(1280, 720);

        if (!camAvailable)
        {
            return;
        }

        if (!isInitialized && webCam.didUpdateThisFrame) {

            data = new Color32[webCam.width * webCam.height];
            print(webCam.width + "height" + webCam.height);
            isInitialized = true;
        }
        else if (isInitialized && webCam.didUpdateThisFrame && frames % captureRate == 0){
            
            webCam.GetPixels32(data);
            t.SetPixels32(data);
            t.Resize(128, 72);
            t.Apply();
            byte[] b = Color32ArrayToByteArray(t.GetPixels32());
            
            //File.WriteAllBytes(Application.dataPath + "/StreamingAssets/testbytefile.txt", b );

#endif
        #endregion

        if(frames % captureRate == 0){
            StartCoroutine(ExportFrame());
        }
    }


    public IEnumerator ExportFrame()
    {
        yield return frameEnd;
        tex.Resize(arCamTexture.width, arCamTexture.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();

        TextureScale.Bilinear(tex, tex.width / 10, tex.height / 10);

        //Debug to write texture into PNG
        //byte[] bytes = tex.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

        byte[] b = Color32ArrayToByteArray(tex.GetPixels32());
        //FramePackage dataToStream = new FramePackage(System.DateTime.UtcNow.ToString(),frames,b);
        FramePackage dataToStream = new FramePackage(System.DateTime.UtcNow, frames, b);
        Debug.Log(dataToStream.ApproximateCaptureTime);
        //string JSONdataToStream = JsonUtility.ToJson(dataToStream);
        string JSONdataToStream = dataToStream.serialize();

        Debug.Log("Sending image to Kinesis");
        _C.PutRecord(JSONdataToStream, "FrameStream", (response) =>{});
    }


    // A helper function to convert a color32 array to a byte array 
    private static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        byte[] bytes = new byte[colors.Length * 4];
        for (int i = 0; i < bytes.Length / 4; i += 4)
        {
            bytes[i] = colors[i].r;
            bytes[i + 1] = colors[i].g;
            bytes[i + 2] = colors[i].b;
            bytes[i + 3] = colors[i].a;
        }
        return bytes;
    }


    // A helper function to convert a JSON string to a byte array
    public static byte[] JsonStringToByteArray(string jsonString)
    {
        var encoding = new UTF8Encoding();
        return encoding.GetBytes(jsonString.Substring(1, jsonString.Length - 2));
    }


    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }
}
