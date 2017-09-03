using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KinectFloor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor = null;
        private DepthFrameReader _depthReader = null;
        private BodyFrameReader _bodyReader = null;

        private Body _body = null;
        private Floor _floor = null;

        public MainWindow()
        {
            InitializeComponent();

            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _depthReader = _sensor.DepthFrameSource.OpenReader();
                _depthReader.FrameArrived += DepthReader_FrameArrived;
                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;
            }
        }

        private int _floorY = 0;

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    _floor = frame.Floor();
                    _body = frame.Body();

                    if (_floor != null && _body != null)
                    {
                        CameraSpacePoint wrist3D = _body.Joints[JointType.WristLeft].Position;

                        double distance = _floor.DistanceFrom(wrist3D);

                        TblDistance.Text = distance.ToString("N2");

                        Point wrist2D = wrist3D.ToPoint();

                        _floorY = _floor.FloorY((int)wrist2D.X, (ushort)(wrist3D.Z * 1000));

                        Canvas.SetLeft(EllipseHand, wrist2D.X - EllipseHand.Width / 2.0);
                        Canvas.SetTop(EllipseHand, wrist2D.Y - EllipseHand.Height / 2.0);
                        Canvas.SetLeft(EllipseFloor, wrist2D.X - EllipseFloor.Width / 2.0);
                        Canvas.SetTop(EllipseFloor, _floorY - EllipseFloor.Height / 2.0);
                    }
                }
            }
        }

        private void DepthReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            using (DepthFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    Camera.Source = frame.Bitmap();
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_depthReader != null)
            {
                _depthReader.Dispose();
            }

            if (_bodyReader != null)
            {
                _bodyReader.Dispose();
            }

            if (_sensor != null && _sensor.IsOpen)
            {
                _sensor.Close();
            }
        }
    }
}
