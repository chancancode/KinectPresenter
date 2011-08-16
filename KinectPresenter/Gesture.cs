using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace KinectPresenter
{
    public interface IGesture
    {
        GestureType GestureType { get; }
        GestureSubType GestureSubType { get; }
        GestureHandType HandsUsed { get; }
        double Confidence { get; }
    }

    public enum GestureType
    {
        SingleHandedSwipe
    }

    public enum GestureSubType
    {
        LeftHandedSwipeFromLeftToRight,
        LeftHandedSwipeFromRightToLeft,
        LeftHandedSwipeFromTopToBottom,
        LeftHandedSwipeFromBottomToTop,
        RightHandedSwipeFromLeftToRight,
        RightHandedSwipeFromRightToLeft,
        RightHandedSwipeFromTopToBottom,
        RightHandedSwipeFromBottomToTop
    }

    public enum GestureHandType
    {
        LeftHand,
        RightHand,
        BothHands
    }

    public interface IGestureRecognizer
    {
        void MatchFrame(SkeletonFrame frame);
        event EventHandler<GestureRecognizedEventArgs> GestureRecognized;
    }

    public class GestureRecognizedEventArgs : EventArgs
    {
        public readonly IGesture Result;

        public GestureRecognizedEventArgs(IGesture result)
        {
            Result = result;
        }
    }

    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public bool Is3D { get; private set; }

        public Point(Vector v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public Point(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float DistaceFrom(Point other)
        {
            return DistanceBetween(this, other);
        }

        public static float DistanceBetween(Point a, Point b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
        }
    }
}
