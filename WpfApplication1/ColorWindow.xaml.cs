using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace WpfApplication1
{

    public partial class ColorWindow : Window
    {
        KinectSensor kinect;
        public ColorWindow(KinectSensor sensor) : this()
        {
            kinect = sensor;
        }
        public ColorWindow()
        {
            InitializeComponent();
            Loaded += ColorWindow_Loaded;
            Unloaded += ColorWindow_Unloaded;
        }
        void ColorWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (kinect != null)
            {
                kinect.ColorStream.Disable();
                kinect.ColorFrameReady -= myKinect_ColorFrameReady;
                kinect.Stop();
            }
        }
        private WriteableBitmap _ColorImageBitmap;
        private Int32Rect _ColorImageBitmapRect;
        private int _ColorImageStride;
        void ColorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (kinect != null)
            {
                ColorImageStream colorStream = kinect.ColorStream;

                colorStream.Enable();
                kinect.ColorFrameReady += myKinect_ColorFrameReady;

                //_ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth,colorStream.FrameHeight, 96, 96,
                //                                        PixelFormats.Bgr32, null);

                _ColorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96,
                                        PixelFormats.Bgra32, null);

                _ColorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth,colorStream.FrameHeight);
                _ColorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorData.Source = _ColorImageBitmap;

                kinect.Start();
            }
        }

        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if(frame == null)
                    return ;
                
                byte[] pixelData = new byte[frame.PixelDataLength];
                frame.CopyPixelDataTo(pixelData);

                MyPixelShader("Transparent", pixelData, frame.BytesPerPixel);

                _ColorImageBitmap.WritePixels(_ColorImageBitmapRect, pixelData,_ColorImageStride, 0);
            }
        }

        public void MyPixelShader(string name, byte[] pixelData, int BytesPerPixel)
        {
            if (name == "RedFilter")
                RedFilterPixelShader(pixelData, BytesPerPixel);
            else if (name == "GreenFilter")
                GreenFilterPixelShader(pixelData, BytesPerPixel);
            else if (name == "BlueFilter")
                BlueFilterPixelShader(pixelData, BytesPerPixel);
            else if (name == "GrayFilter")
                GrayFilterPixelShader(pixelData, BytesPerPixel);
            else if (name == "InvertedColors")
                InvertedColorsPixelShader(pixelData, BytesPerPixel);
            else if (name == "Transparent")
                TransparentPixelShader(pixelData, BytesPerPixel);
        }
        public void RedFilterPixelShader(byte[] pixelData, int BytesPerPixel)
        {
            for (int i = 0; i < pixelData.Length; i += BytesPerPixel)
            {
                pixelData[i] = 0x00; //Blue
                pixelData[i + 1] = 0x00; //Green
            }
        }
        public void GreenFilterPixelShader(byte[] pixelData, int BytesPerPixel)
        {
            for (int i = 0; i < pixelData.Length; i += BytesPerPixel)
            {
                pixelData[i] = 0x00; //Blue
                pixelData[i + 2] = 0x00; //Red
            }
        }
        public void BlueFilterPixelShader(byte[] pixelData, int BytesPerPixel)
        {
            for (int i = 0; i < pixelData.Length; i += BytesPerPixel)
            {
                pixelData[i + 1] = 0x00; //Green
                pixelData[i + 2] = 0x00; //Red
            }
        }
        public void GrayFilterPixelShader(byte[] pixelData, int BytesPerPixel)
        {
            for (int i = 0; i < pixelData.Length; i += BytesPerPixel)
            {
                byte maxvalue = Math.Max(Math.Max(pixelData[i],pixelData[i + 1]),pixelData[i + 2]);
                pixelData[i] = maxvalue;
                pixelData[i + 1] = maxvalue;
                pixelData[i + 2] = maxvalue; 
            }
        }
        public void InvertedColorsPixelShader(byte[] pixelData, int BytesPerPixel)
        {
            for (int i = 0; i < pixelData.Length; i += BytesPerPixel)
            {
                pixelData[i] = (byte)~pixelData[i];
                pixelData[i + 1] = (byte)~pixelData[i + 1];
                pixelData[i + 2] = (byte)~pixelData[i + 2];
            }
        }
        public void TransparentPixelShader(byte[] pixelData, int BytesPerPixel)
        {
            for (int i = 0; i < pixelData.Length; i += BytesPerPixel)
            {
                pixelData[i + 3] = 128;
            }
        }


    }
}
