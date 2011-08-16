using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectPresenter
{
    public interface ISlideShowCommandDetector
    {
        void Initialize();
        void Start();
        void Stop();
        
        event EventHandler<SlideShowCommandDetectedEventArgs> SlideShowCommandDetected;
    }

    public enum SlideShowCommandType
    {
        Cue,
        Next,
        Previous,
        GotoSlide,
        EndSlideShow
    }

    public class SlideShowCommandDetectedEventArgs : EventArgs
    {
        public readonly SlideShowCommandType EventType;
        public readonly int SlideIndex;
        public readonly string Cue;

        public SlideShowCommandDetectedEventArgs(SlideShowCommandType eventType)
        {
            EventType = eventType;
            SlideIndex = 0;
            Cue = "";
        }

        public SlideShowCommandDetectedEventArgs(SlideShowCommandType eventType, int slideIndex)
        {
            EventType = eventType;
            SlideIndex = slideIndex;
            Cue = "";
        }

        public SlideShowCommandDetectedEventArgs(SlideShowCommandType eventType, string cue)
        {
            EventType = eventType;
            SlideIndex = 0;
            Cue = cue;
        }
    }
}
