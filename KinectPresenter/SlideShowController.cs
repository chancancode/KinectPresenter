using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;

namespace KinectPresenter
{
    public class SlideShowController
    {
        private Application pptApp;
        private List<ISlideShowCommandDetector> detectors;

        private SpeechEngine speechEngine;

        private SlideShowView slideShowView;
        private int slideId;
        private int step;

        public SlideShowController(Application application, SpeechEngine speech, GestureEngine gesture, CueStore store)
        {
            pptApp = application;
            pptApp.SlideShowBegin += OnSlideShowBegin;
            pptApp.SlideShowEnd += OnSlideShowEnd;
            pptApp.SlideShowNextSlide += OnSlideShowNextSlide;

            detectors = new List<ISlideShowCommandDetector>();
            detectors.Add(new SpeechCommandDetector(this, speech, store));
            detectors.Add(new GestureCommandDetector(this, gesture));

            slideShowView = null;
            slideId = 0;
            step = 0;
        }

        public int GetSlideCount()
        {
            return pptApp.ActivePresentation.Slides.Count;
        }

        public int GetCurrentSlideId()
        {
            return slideId;
        }

        public int GetCurrentStep()
        {
            return step;
        }

        private void OnSlideShowBegin(SlideShowWindow Wn)
        {
            try
            {
                foreach (ISlideShowCommandDetector detector in detectors)
                {
                    detector.Initialize();
                }

                foreach (ISlideShowCommandDetector detector in detectors)
                {
                    detector.SlideShowCommandDetected += OnSlideShowCommandDetected;
                    detector.Start();
                }
            }
            catch
            {
                ErrorHandler.ShowSlideShowModeKinectNotFoundDialog();
            }

            slideShowView = Wn.View;
            slideId = slideShowView.Slide.SlideID;
            step = 0;
        }

        private void OnSlideShowEnd(Presentation Pres)
        {
            foreach (ISlideShowCommandDetector detector in detectors)
            {
                detector.Stop();
                detector.SlideShowCommandDetected -= OnSlideShowCommandDetected;
            }

            slideShowView = null;
            slideId = 0;
            step = 0;
        }

        private void OnSlideShowNextSlide(SlideShowWindow Wn)
        {
            slideId = slideShowView.Slide.SlideID;
            step = 0;
        }

        private void OnSlideShowCommandDetected(object sender, SlideShowCommandDetectedEventArgs e)
        {
            switch(e.EventType)
            {
                case SlideShowCommandType.Next:
                    slideShowView.Next();
                    break;
                case SlideShowCommandType.Previous:
                    slideShowView.Previous();
                    break;
                case SlideShowCommandType.GotoSlide:
                    slideShowView.GotoSlide(e.SlideIndex);
                    break;
                case SlideShowCommandType.EndSlideShow:
                    slideShowView.Exit();
                    break;
                case SlideShowCommandType.Cue:
                    slideShowView.Next();
                    step++;
                    break;
            }
        }
    }
}
