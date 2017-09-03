using Microsoft.Kinect;
using System;

namespace LightBuzz.Vitruvius
{
    /// <summary>
    /// Represents a floor plane.
    /// </summary>
    public class Floor
    {
        /// <summary>
        /// The X value of the FloorClipPlane quaternion.
        /// </summary>
        public float X { get; internal set; }

        /// <summary>
        /// The Y value of the FloorClipPlane quaternion.
        /// </summary>
        public float Y { get; internal set; }

        /// <summary>
        /// The Z value of the FloorClipPlane quaternion.
        /// </summary>
        public float Z { get; internal set; }

        /// <summary>
        /// The W value of the FloorClipPlane quaternion.
        /// </summary>
        public float W { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="Floor"/>.
        /// </summary>
        /// <param name="floorClipPlane">The FloorClipPlane quaternion that describes the floor.</param>
        public Floor(Vector4 floorClipPlane)
        {
            X = floorClipPlane.X;
            Y = floorClipPlane.Y;
            Z = floorClipPlane.Z;
            W = floorClipPlane.W;
        }

        /// <summary>
        /// Returns the height of the sensor.
        /// </summary>
        public float Height
        {
            get { return W; }
        }

        /// <summary>
        /// Returns the sensor's tilt angle (in degrees).
        /// </summary>
        public double Tilt
        {
            get
            {
                return Math.Atan(Z / Y) * (180.0 / Math.PI);
            }
        }

        /// <summary>
        /// Calculates the distance between the specified joint and the floor.
        /// </summary>
        /// <param name="point">The point to measure the distance from.</param>
        /// <returns>The distance between the floor and the point (in meters).</returns>
        public double DistanceFrom(CameraSpacePoint point)
        {
            double numerator = X * point.X + Y * point.Y + Z * point.Z + W;
            double denominator = Math.Sqrt(X * X + Y * Y + Z * Z);

            return numerator / denominator;
        }
    }
}
