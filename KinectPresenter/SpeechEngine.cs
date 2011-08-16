using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace KinectPresenter
{
    public class SpeechEngine
    {
        public event EventHandler<SpeechRecognizedEventArgs> SpeechRecognized;

        private const string RECOGNIZER_ID = "SR_MS_en-US_Kinect_10.0";
        private const double CONFIDENCE_THRESHOLD = 0.8;

        private Mutex engineLock;
        private Thread engineHost;
        private RecognizerInfo ri;
        private Grammar grammar;
        private bool initialized;

        public SpeechEngine()
        {
            initialized = false;
        }

        public void Initialize() 
        {
            if(initialized)
            {
                return;
            }

            KinectAudioSource audioSource = new KinectAudioSource(); // test Kinect

            ri = SpeechRecognitionEngine.InstalledRecognizers().Where(r => r.Id == RECOGNIZER_ID).FirstOrDefault();

            if (ri == null)
            {
                throw new Exception("Could not find speech recognizer " + RECOGNIZER_ID + ".");
            }

            initialized = true;
        }

        public void Start(GrammarBuilder grammarbuilder)
        {
            if (!initialized)
            {
                return;
            }

            if (engineLock != null)
            {
                Stop();
            }

            grammarbuilder.Culture = ri.Culture;
            grammar = new Grammar(grammarbuilder);

            engineLock = new Mutex(true);
            engineHost = new Thread(EngineWorker);
            engineHost.Start();
        }

        public void Stop()
        {
            if (engineLock != null)
            {
                engineLock.ReleaseMutex();
                engineHost.Join();
            }

            engineLock = null;
            engineHost = null;
        }

        private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > CONFIDENCE_THRESHOLD && SpeechRecognized != null)
            {
                SpeechRecognized(this, e);
            }
        }

        private void EngineWorker()
        {
            KinectAudioSource audioSource = new KinectAudioSource();
            audioSource.FeatureMode = true;
            audioSource.AutomaticGainControl = false;
            audioSource.SystemMode = SystemMode.OptibeamArrayOnly;

            Stream audioStream = audioSource.Start();

            SpeechRecognitionEngine engine = new SpeechRecognitionEngine(ri.Id);
            engine.LoadGrammar(grammar);
            engine.SpeechRecognized += OnSpeechRecognized;
            engine.SetInputToAudioStream(audioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            engine.RecognizeAsync(RecognizeMode.Multiple);

            engineLock.WaitOne();

            engine.RecognizeAsyncCancel();
            engine.SpeechRecognized -= OnSpeechRecognized;
            audioSource.Stop();
        }
    }
}
