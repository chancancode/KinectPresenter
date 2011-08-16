using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectPresenter
{
    class GestureCommandDetector : ISlideShowCommandDetector
    {
        public event EventHandler<SlideShowCommandDetectedEventArgs> SlideShowCommandDetected;

        // TODO: read from settings on start
        private const GestureSubType GESTURE_COMMAND_NEXT = GestureSubType.RightHandedSwipeFromRightToLeft;
        private const GestureSubType GESTURE_COMMAND_PREVIOUS = GestureSubType.LeftHandedSwipeFromLeftToRight;
        
        private SlideShowController slideShowController;
        private GestureEngine gestureEngine;

        public GestureCommandDetector(SlideShowController controller, GestureEngine engine)
        {
            slideShowController = controller;
            gestureEngine = engine;
        }

        public void Initialize()
        {
            gestureEngine.Initialize();
        }

        public void Start()
        {
            List<IGestureRecognizer> gestures = new List<IGestureRecognizer>();
            // TODO: this should be generic - add a GestureRecognizerFactory or something
            gestures.Add(new SingleHandedSwipeGestureRecognizer(GESTURE_COMMAND_NEXT));
            gestures.Add(new SingleHandedSwipeGestureRecognizer(GESTURE_COMMAND_PREVIOUS));

            gestureEngine.GestureRecognized += OnGestureRecognized;
            gestureEngine.Start(gestures);
        }

        public void Stop()
        {
            gestureEngine.Stop();
            gestureEngine.GestureRecognized -= OnGestureRecognized;
        }

        private void OnGestureRecognized(object sender, GestureRecognizedEventArgs e)
        {
            if (SlideShowCommandDetected != null)
            {
                SlideShowCommandDetectedEventArgs args = null;

                switch (e.Result.GestureSubType)
                {
                    case GESTURE_COMMAND_NEXT:
                        args = new SlideShowCommandDetectedEventArgs(SlideShowCommandType.Next);
                        break;
                    case GESTURE_COMMAND_PREVIOUS:
                        args = new SlideShowCommandDetectedEventArgs(SlideShowCommandType.Previous);
                        break;
                }

                SlideShowCommandDetected(this, args);
            }
        }
    }
}
