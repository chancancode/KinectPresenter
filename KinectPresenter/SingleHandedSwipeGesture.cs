using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace KinectPresenter
{
    public enum SingleHandedSwipeGestureDirectionType
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public class SingleHandedSwipeGestureRecognizer : IGestureRecognizer
    {
        public event EventHandler<GestureRecognizedEventArgs> GestureRecognized;

        public GestureType GestureType { get { return GestureType.SingleHandedSwipe; } }
        public GestureHandType HandsUsed { get { return hand; } }
        public SingleHandedSwipeGestureDirectionType SwipeDirection { get { return direction; } }

        private const int FRAMES_THRESHOLD = 20; // TODO: tune this
        private const float PRIMARY_AXIS_MIN_DISTANCE_THRESHOLD = 0.0F; // TODO: tune this
        private const float PRIMARY_AXIS_MAX_DISTANCE_THRESHOLD = float.PositiveInfinity; // TODO: tune this
        private const float SECONDARY_AXES_VARIANCE_THRESHOLD = float.PositiveInfinity; // TODO: tune this

        private readonly GestureHandType hand;
        private readonly SingleHandedSwipeGestureDirectionType direction;

        private int framesMatched;

        // TODO: switch to a circular queue
        /* The current behavior isn't exactly correct. Ideally, we should be
         * keeping track of the previous FRAMES_THRESHOLD frames; whenever we
         * have to reset we should set the new head to be the earliest frame
         * in the queue such that all subsequent frames would still met the
         * requirements (distance, variance etc).
         */
        private Point initial;
        private Point previous;

        public SingleHandedSwipeGestureRecognizer(GestureSubType subType)
        {
            switch (subType)
            {
                case GestureSubType.LeftHandedSwipeFromLeftToRight:
                    hand = GestureHandType.LeftHand;
                    direction = SingleHandedSwipeGestureDirectionType.LeftToRight;
                    break;
                case GestureSubType.LeftHandedSwipeFromRightToLeft:
                    hand = GestureHandType.LeftHand;
                    direction = SingleHandedSwipeGestureDirectionType.RightToLeft;
                    break;
                case GestureSubType.RightHandedSwipeFromLeftToRight:
                    hand = GestureHandType.RightHand;
                    direction = SingleHandedSwipeGestureDirectionType.LeftToRight;
                    break;
                case GestureSubType.RightHandedSwipeFromRightToLeft:
                    hand = GestureHandType.RightHand;
                    direction = SingleHandedSwipeGestureDirectionType.RightToLeft;
                    break;
                default:
                    throw new NotSupportedException();
            }

            Reset(null);
        }

        public SingleHandedSwipeGestureRecognizer(GestureHandType handType, SingleHandedSwipeGestureDirectionType directionType)
        {
            if(handType == GestureHandType.BothHands)
            {
                throw new NotSupportedException();
            }

            hand = handType;
            direction = directionType;

            Reset(null);
        }

        private void Reset(Point initialPoint)
        {
            framesMatched = (initialPoint == null) ? 0 : 1;
            initial = initialPoint;
            previous = initialPoint;
        }

        public void MatchFrame(SkeletonFrame frame)
        {
            foreach (SkeletonData skeleton in frame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                Point handPosition = null;

                switch (hand)
                {
                    case GestureHandType.LeftHand:
                        handPosition = new Point(skeleton.Joints[JointID.HandLeft].Position);
                        break;
                    case GestureHandType.RightHand:
                        handPosition = new Point(skeleton.Joints[JointID.HandRight].Position);
                        break;
                }

                // special case: first frame
                if (framesMatched == 0)
                {
                    Reset(handPosition);
                    return;
                }

                bool primaryConditionMet = false;

                switch (direction)
                {
                    case SingleHandedSwipeGestureDirectionType.BottomToTop:
                        primaryConditionMet = handPosition.Y > previous.Y;
                        break;
                    case SingleHandedSwipeGestureDirectionType.TopToBottom:
                        primaryConditionMet = handPosition.Y < previous.Y;
                        break;
                    case SingleHandedSwipeGestureDirectionType.LeftToRight:
                        primaryConditionMet = handPosition.X > previous.X;
                        break;
                    case SingleHandedSwipeGestureDirectionType.RightToLeft:
                        primaryConditionMet = handPosition.X < previous.X;
                        break;
                }

                if (!primaryConditionMet)
                {
                    Reset(handPosition);
                    return;
                }

                bool varianceConditionMet = false;

                switch (direction)
                {
                    case SingleHandedSwipeGestureDirectionType.BottomToTop:
                    case SingleHandedSwipeGestureDirectionType.TopToBottom:
                        varianceConditionMet = Math.Abs(handPosition.X - initial.X) < SECONDARY_AXES_VARIANCE_THRESHOLD;
                        break;
                    case SingleHandedSwipeGestureDirectionType.LeftToRight:
                    case SingleHandedSwipeGestureDirectionType.RightToLeft:
                        varianceConditionMet = Math.Abs(handPosition.Y - initial.Y) < SECONDARY_AXES_VARIANCE_THRESHOLD;
                        break;
                }

                varianceConditionMet = varianceConditionMet && Math.Abs(handPosition.Z - initial.Z) < SECONDARY_AXES_VARIANCE_THRESHOLD;

                if (!varianceConditionMet)
                {
                    Reset(handPosition);
                    return;
                }

                if (framesMatched > FRAMES_THRESHOLD)
                {
                    // We have already fired an event for this gesture
                    previous = handPosition;
                    framesMatched++;
                    return;
                }
                else if (framesMatched < FRAMES_THRESHOLD)
                {
                    if (handPosition.DistaceFrom(initial) > PRIMARY_AXIS_MAX_DISTANCE_THRESHOLD)
                    {
                        Reset(handPosition);
                        return;
                    }

                    previous = handPosition;
                    framesMatched++;
                    return;
                }
                else if (framesMatched == FRAMES_THRESHOLD)
                {
                    float dist = handPosition.DistaceFrom(initial);

                    if (dist > PRIMARY_AXIS_MIN_DISTANCE_THRESHOLD && dist < PRIMARY_AXIS_MAX_DISTANCE_THRESHOLD)
                    {
                        previous = handPosition;
                        framesMatched++;

                        if (GestureRecognized != null)
                        {
                            SingleHandedSwipeGesture result = new SingleHandedSwipeGesture(hand, direction, 1.0, initial, previous);
                            GestureRecognized(this, new GestureRecognizedEventArgs(result));
                        }
                        return;
                    }
                    else
                    {
                        Reset(handPosition);
                        return;
                    }
                }
            }

            // We reach here if there are no tracked skeleton
            Reset(null);
        }
    }

    public class SingleHandedSwipeGesture : IGesture
    {

        public GestureType GestureType { get { return GestureType.SingleHandedSwipe; } }
        public GestureSubType GestureSubType { get; private set; }
        public GestureHandType HandsUsed { get; private set; }
        public SingleHandedSwipeGestureDirectionType Direction { get; private set; }
        public double Confidence { get; private set; }
        public Point StartPosition { get; private set; }
        public Point EndPosition { get; private set; }

        public SingleHandedSwipeGesture(GestureHandType handsUsed, SingleHandedSwipeGestureDirectionType direction,
            double confidence, Point startPosition, Point endPosition)
        {
            HandsUsed = handsUsed;
            Direction = direction;
            Confidence = confidence;
            StartPosition = startPosition;
            EndPosition = endPosition;
            GestureSubType = GetGestureSubType(handsUsed, direction);
        }

        public static GestureSubType GetGestureSubType(GestureHandType handsUsed, SingleHandedSwipeGestureDirectionType direction)
        {
            if (handsUsed == GestureHandType.LeftHand && direction == SingleHandedSwipeGestureDirectionType.LeftToRight)
            {
                return GestureSubType.LeftHandedSwipeFromLeftToRight;
            }
            else if (handsUsed == GestureHandType.LeftHand && direction == SingleHandedSwipeGestureDirectionType.RightToLeft)
            {
                return GestureSubType.LeftHandedSwipeFromRightToLeft;
            }
            else if (handsUsed == GestureHandType.LeftHand && direction == SingleHandedSwipeGestureDirectionType.TopToBottom)
            {
                return GestureSubType.LeftHandedSwipeFromTopToBottom;
            }
            else if (handsUsed == GestureHandType.LeftHand && direction == SingleHandedSwipeGestureDirectionType.BottomToTop)
            {
                return GestureSubType.LeftHandedSwipeFromBottomToTop;
            }
            else if (handsUsed == GestureHandType.RightHand && direction == SingleHandedSwipeGestureDirectionType.LeftToRight)
            {
                return GestureSubType.RightHandedSwipeFromLeftToRight;
            }
            else if (handsUsed == GestureHandType.RightHand && direction == SingleHandedSwipeGestureDirectionType.RightToLeft)
            {
                return GestureSubType.RightHandedSwipeFromRightToLeft;
            }
            else if (handsUsed == GestureHandType.RightHand && direction == SingleHandedSwipeGestureDirectionType.TopToBottom)
            {
                return GestureSubType.RightHandedSwipeFromTopToBottom;
            }
            else if (handsUsed == GestureHandType.RightHand && direction == SingleHandedSwipeGestureDirectionType.BottomToTop)
            {
                return GestureSubType.RightHandedSwipeFromBottomToTop;
            }

            throw new InvalidOperationException();
        }
    }
}
