using UnityEngine;
using OpenCvSharp;

namespace UniCV {
    public class WebCamTextureCVBehaviour : MonoBehaviour {
        // PUBLIC
        public MeshRenderer feedRenderer;
        public MeshRenderer processedOutputRenderer;

        public int camIndex;
        public const int camWidth = 1280;
        public const int camHeight = 720;

        public bool showFeed;

        // UNITY PRIVATES
        WebCamTexture mFeedTexture;
        Color32[] mWebCamPixels;
        Color32[] mProcessedPixels;

        // CV PRIVATES
        Mat mSourceCVMat;
        Vec3b[] mSourceCVImage;
        Mat mProcessedCVMat;
        byte[] mProcessedCVImageBytes;
        Texture2D mProcessedTexture;

        // FLAGS
        bool doMatToTexture;
    
        void Start() {
            ThreadPool.InitInstance();

            WebCamDevice[] devices = WebCamTexture.devices;

            if (devices.Length == 0) {
                Debug.LogError("There are no web cameras connected to this computer. Aborting!");
                return;
            }

            // Attach the web cam feed to a renderer
            mFeedTexture = new WebCamTexture(devices[camIndex].name, camWidth, camHeight);
            feedRenderer.material.mainTexture = mFeedTexture;
            mFeedTexture.Play();

            mWebCamPixels = new Color32[camWidth * camHeight];
            mProcessedPixels = new Color32[camHeight * camWidth];

            // Initialize CV fields
            mSourceCVMat = new Mat(camHeight, camWidth, MatType.CV_8UC3);
            mSourceCVImage = new Vec3b[camHeight * camWidth];

            mProcessedCVMat = new Mat(camHeight, camWidth, MatType.CV_8UC1);
            mProcessedCVImageBytes = new byte[camHeight * camWidth];

            // Create the processed Unity Texture and attach it
            mProcessedTexture = new Texture2D(camWidth, camHeight, TextureFormat.RGBA32, true, true);
            processedOutputRenderer.material.mainTexture = mProcessedTexture;

            // create opencv window to display the original video
            if(showFeed)
                Cv2.NamedWindow("Web Cam Feed");
        }
    
        void Update() {
            if (!mFeedTexture.isPlaying || !mFeedTexture.didUpdateThisFrame)
                return;

            UnityTextureToCVMat();
            if(showFeed)
                RefreshWindow(mSourceCVMat);
            ProcessAndUpdateUnityTexture(mSourceCVMat);

            CVMatToUnityTexture();
        }

        // Convert Unity Texture2D object to OpenCVSharp Mat object
        // This cannot run on a different thread, but Parallel.cs can be used
        void UnityTextureToCVMat() {
            var mWebCamPixels = mFeedTexture.GetPixels32();

            // Convert Color32 object to Vec3b object
            // Vec3b is the representation of pixel for Mat
            Parallel.For(0, camHeight, i => {
                for (var j = 0; j < camWidth; j++) {
                    var col = mWebCamPixels[j + i * camWidth];
                    var vec3 = new Vec3b {
                        Item0 = col.b,
                        Item1 = col.g,
                        Item2 = col.r
                    };
                    // set pixel to an array
                    mSourceCVImage[j + i * camWidth] = vec3;
                }
            });
            // assign the Vec3b array to Mat
            mSourceCVMat.SetArray(0, 0, mSourceCVImage);
        }

        // Convert OpenCVSharp Mat object to Unity Texture2D object
        // This cannot run on a different thread, but Parallel.cs can be used
        void CVMatToUnityTexture() {
            // cannyImageData is byte array, because canny image is grayscale
            mProcessedCVMat.GetArray(0, 0, mProcessedCVImageBytes);

            // parallel for loop
            Parallel.For(0, camHeight, i => {
                for (var j = 0; j < camWidth; j++) {
                    byte vec = mProcessedCVImageBytes[j + i * camWidth];
                    var color32 = new Color32 {
                        r = vec,
                        g = vec,
                        b = vec,
                        a = 0
                    };
                    mProcessedPixels[j + i * camWidth] = color32;
                }
            });
            mProcessedTexture.SetPixels32(mProcessedPixels);
            mProcessedTexture.Apply();
        }

        // Multithreaded Processing
        void ProcessAndUpdateUnityTexture(Mat _image) {
            ThreadPool.QueueUserWorkItem(ProcessAndUpdateUnityTexture_Threaded, _image);
        }

        void ProcessAndUpdateUnityTexture_Threaded(object state) {
            var _image = (Mat)state;
            // NOTE : 
            // Place your CV logic over the frame here
            // As an example, we do a Canny filter
            Cv2.Canny(_image, mProcessedCVMat, 100, 100);
            doMatToTexture = true;
        }

        // Display the original video in a opencv window
        // Run on main thread
        void RefreshWindow(Mat _image) {
            Cv2.ImShow("Web Cam Feed", _image);
        }

        // close the opencv window
        public void OnDestroy() {
            Cv2.DestroyAllWindows();
        }
    }
}