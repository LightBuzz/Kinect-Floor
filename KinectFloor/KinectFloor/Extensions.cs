using Microsoft.Kinect;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LightBuzz.Vitruvius
{
    public static class Extensions
    {
        private static Body[] _bodyData = new Body[6];
        private static ushort[] _depthData = new ushort[512 * 424];
        private static CameraSpacePoint[] _cameraData = new CameraSpacePoint[512 * 424];
        private static byte[] _colorData = new byte[512 * 424 * 4];
        private static WriteableBitmap _bitmap = new WriteableBitmap(512, 424, 96.0, 96.0, PixelFormats.Bgr32, null);

        private static CoordinateMapper _mapper = KinectSensor.GetDefault().CoordinateMapper;

        public static Body Body(this BodyFrame frame)
        {
            if (frame == null) return null;

            frame.GetAndRefreshBodyData(_bodyData);

            return _bodyData.Where(b => b != null && b.IsTracked).FirstOrDefault();
        }

        public static Floor Floor(this BodyFrame frame)
        {
            if (frame == null) return null;

            return new Floor(frame.FloorClipPlane);
        }

        public static Point ToPoint(this CameraSpacePoint point3D)
        {
            Point point = new Point();
            DepthSpacePoint point2D = _mapper.MapCameraPointToDepthSpace(point3D);

            if (!float.IsInfinity(point2D.X) && !float.IsInfinity(point2D.Y))
            {
                point.X = point2D.X;
                point.Y = point2D.Y;
            }

            return point;
        }

        public static int FloorY(this Floor floor, int x, ushort z)
        {
            _mapper.MapDepthFrameToCameraSpace(_depthData, _cameraData);

            for (int index = 0; index < _depthData.Length; index++)
            {
                ushort currentZ = _depthData[index];
                int currentX = index % 512;

                if (currentX >= x - 10 && currentX <= x + 10 && currentZ >= z - 10 && currentZ <= z + 10)
                {
                    CameraSpacePoint point3D = _cameraData[index];

                    if (floor.DistanceFrom(point3D) < 0.01)
                    {
                        return index / 512;
                    }
                }
            }

            return 424;
        }

        public static WriteableBitmap Bitmap(this DepthFrame frame, Floor floor = null)
        {
            if (frame == null) return null;

            frame.CopyFrameDataToArray(_depthData);
            _mapper.MapDepthFrameToCameraSpace(_depthData, _cameraData);
            
            int colorIndex = 0;

            for (int index = 0; index < _depthData.Length; index++)
            {
                ushort depth = _depthData[index];
                byte color = (byte)(depth * 255 / 8000);
                CameraSpacePoint point = _cameraData[index];

                if (floor != null && floor.DistanceFrom(point) < 0.01)
                {
                    _colorData[colorIndex++] = Colors.Green.B;
                    _colorData[colorIndex++] = Colors.Green.G;
                    _colorData[colorIndex++] = Colors.Green.R;
                    _colorData[colorIndex++] = Colors.Green.A;
                }
                else
                {
                    _colorData[colorIndex++] = color;
                    _colorData[colorIndex++] = color;
                    _colorData[colorIndex++] = color;
                    _colorData[colorIndex++] = 255;
                }
            }

            _bitmap.Lock();

            Marshal.Copy(_colorData, 0, _bitmap.BackBuffer, _colorData.Length);

            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Unlock();

            return _bitmap;
        }
    }
}
