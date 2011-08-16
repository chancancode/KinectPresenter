using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Speech.Recognition;

namespace KinectPresenter
{
    public class SpeechCommandDetector : ISlideShowCommandDetector
    {
        public event EventHandler<SlideShowCommandDetectedEventArgs> SlideShowCommandDetected;

        // Built-in commands
        private const string SPEECH_COMMAND_PREFIX = "powerpoint";
        private const string SPEECH_COMMAND_NEXT = "next";
        private const string SPEECH_COMMAND_PREVIOUS = "previous";
        private const string SPEECH_COMMAND_END_SLIDESHOW = "end slideshow";

        private const string SPEECH_COMMAND_GOTO_SLIDE = "slide"; // this one is special as it takes a parameter

        private static readonly string[] SPEECH_SIMPLE_COMMANDS = {
            SPEECH_COMMAND_NEXT,
            SPEECH_COMMAND_PREVIOUS,
            SPEECH_COMMAND_END_SLIDESHOW
        };

        private SlideShowController slideShowController;
        private SpeechEngine speechEngine;
        private CueStore cueStore;

        public SpeechCommandDetector(SlideShowController controller, SpeechEngine engine, CueStore store)
        {
            slideShowController = controller;
            speechEngine = engine;
            cueStore = store;
        }

        public void Initialize()
        {
            speechEngine.Initialize();
        }

        public void Start()
        {
            speechEngine.SpeechRecognized += OnSpeechRecognized;
            speechEngine.Start(ConstructGrammarBuilder(cueStore.Flatten(), slideShowController.GetSlideCount()));
        }

        public void Stop()
        {
            speechEngine.Stop();
            speechEngine.SpeechRecognized -= OnSpeechRecognized;
        }

        private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (SlideShowCommandDetected != null)
            {
                SlideShowCommandDetectedEventArgs args = null;

                if (e.Result.Text.StartsWith(SPEECH_COMMAND_PREFIX))
                {
                    string command = e.Result.Text.Substring(SPEECH_COMMAND_PREFIX.Length + 1);

                    if(command.StartsWith(SPEECH_COMMAND_GOTO_SLIDE))
                    {
                        int slideIndex = int.Parse(command.Substring(SPEECH_COMMAND_GOTO_SLIDE.Length + 1));
                        args = new SlideShowCommandDetectedEventArgs(SlideShowCommandType.GotoSlide, slideIndex);
                    }
                    else
                    {
                        switch(command)
                        {
                            case SPEECH_COMMAND_NEXT:
                                args = new SlideShowCommandDetectedEventArgs(SlideShowCommandType.Next);
                                break;
                            case SPEECH_COMMAND_PREVIOUS:
                                args = new SlideShowCommandDetectedEventArgs(SlideShowCommandType.Previous);
                                break;
                            case SPEECH_COMMAND_END_SLIDESHOW:
                                args = new SlideShowCommandDetectedEventArgs(SlideShowCommandType.EndSlideShow);
                                break;
                            default:
                                // a cue word that starts with "powerpoint"?
                                args = new SlideShowCommandDetectedEventArgs(SlideShowCommandType.Cue, command);
                                break;
                        }
                    }
                }
                else
                {
                    args = new SlideShowCommandDetectedEventArgs(SlideShowCommandType.Cue, e.Result.Text);
                }

                if (args.EventType == SlideShowCommandType.Cue && !IsCurrentCue(args.Cue))
                {
                    return;
                }

                SlideShowCommandDetected(this, args);
            }
        }

        private bool IsCurrentCue(string cue)
        {
            int slideId = slideShowController.GetCurrentSlideId();
            int step = slideShowController.GetCurrentStep();
            return cue == cueStore.Get(slideId, step);
        }

        public static GrammarBuilder ConstructGrammarBuilder(HashSet<string> cues, int slideCount)
        {
            Choices choices = new Choices();

            foreach (string command in SPEECH_SIMPLE_COMMANDS)
            {
                choices.Add(SPEECH_COMMAND_PREFIX + " " + command);
            }

            for (int i = 1; i < slideCount + 1; i++)
            {
                choices.Add(SPEECH_COMMAND_PREFIX + " " + SPEECH_COMMAND_GOTO_SLIDE + " " + i);
            }

            foreach (string cue in cues)
            {
                choices.Add(cue);
            }

            return choices.ToGrammarBuilder();
        }
    }
}
