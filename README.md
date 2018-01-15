# UniCV
[WIP] OpenCV for Unity with boilerplate code

UniCV works on OpenCvSharp and is a modification of this repository : https://github.com/qian256/OpenCVSharp3Unity  

# Performance   
- Reduced GC allocations per frame. Currently is ~3.5MB/frame  
- Multithreaded CV processing using ThreadPool class by Nyahoon Games. [Article and credits](https://nyahoon.com/blog-en/366)  
- Overall > 2x improvement in framerate compared to OpenCVSharp3Unity project due to the above. Test machine => i7 4700HQ, GTX 770M, 8 GB RAM  
  
# How to use 
- Open `Assets/UniCV/Demo/DemoScene`. Run the app in the Unity editor to see two planes. One showing the camera feed and the other the output of a Canny filter on the feed.  
- Use the `ProcessAndUpdateUnityTexture_Threaded` method in `WebCamTextureCVBehaviour` class to place your own CV logic.  
  
# Known Issues
- WebCamTexture in Unity is known to have inconsistencies. One of these is that `new WebCamTexture();` doesn't create an object with the height and width of the camera. It creates one with an unrelated resolution (16x16 in my case) and corrects it after about 0.5 seconds. Currently UniCV expects the correct resolution in the inspector.   

# Soon  
- An abstract class that can be extended by your implementations for your own CV effects
- Probably, CV effects on VideoPlayer texture  
- Start webcam without specifying the resolution  

# Contact
[@twitter](https://www.twitter.com/vatsalAmbastha)  
[@github](https://githib.com/adrenak)  
[@www](http://www.vatsalAmbastha.com)