namespace VideoRecorder.Services;
using OpenCvSharp;


public class ComputerVision
{
    // open a connection to a camera > i am using internal webcam for testing
    public void OpenCvAnalyze(string filename)
    {
        //create video capture object; 0 means default device(webcam)
        using var capture = new VideoCapture(filename, VideoCaptureAPIs.FFMPEG);
        capture.Set(VideoCaptureProperties.FrameWidth, 640);
        capture.Set(VideoCaptureProperties.FrameHeight, 480);
        
        if (!capture.IsOpened())
        {
            Console.WriteLine("Cam not opened");
            return;
        }
        
        // camera is at 30 fps divided by 1000ms = 33 ms between frames
        var sleepTime = (int)Math.Round(1000 / capture.Fps);
        using var window = new Window("capture");
        
        var image = new Mat(); // matrix of pixels

        while (true)
        {
            capture.Read(image);
            if (image.Empty())
            {
                Console.WriteLine("No Video. Break.");
                break; 
            }

            Console.WriteLine("Receiving Video...");
            window.ShowImage(image);
            
            var key = Cv2.WaitKey(1);
            
            if (key == 'q')
            {
                window.Close();
                break;
            }
        }
        capture.Release();
        Cv2.DestroyAllWindows();
    }
}