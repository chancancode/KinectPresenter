using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Research.Kinect.Nui;

namespace KinectPresenter
{
    public class GestureEngine
    {
        public event EventHandler<GestureRecognizedEventArgs> GestureRecognized;

        private Mutex engineLock;
        private Thread engineHost;
        private List<IGestureRecognizer> recognizers;
        private bool initialized;

        public GestureEngine()
        {
            initialized = false;
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            // test Kinect
            Runtime runtime = new Runtime();
            runtime.Initialize(RuntimeOptions.UseSkeletalTracking);
            runtime.Uninitialize();

            initialized = true;
        }

        public void Start(List<IGestureRecognizer> gestures)
        {
            if (!initialized)
            {
                return;
            }

            if (engineLock != null)
            {
                Stop();
            }

            recognizers = gestures;

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

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            foreach (IGestureRecognizer recognizer in recognizers)
            {
                recognizer.MatchFrame(e.SkeletonFrame);
            }
        }

        private void OnGestureRecognized(object sender, GestureRecognizedEventArgs e)
        {
            if (GestureRecognized != null)
            {
                GestureRecognized(this, e);
            }
        }

        private void EngineWorker()
        {
            Runtime engine = new Runtime();

            foreach (IGestureRecognizer recognizer in recognizers)
            {
                recognizer.GestureRecognized += OnGestureRecognized;
            }

            engine.SkeletonFrameReady += OnSkeletonFrameReady;
            engine.Initialize(RuntimeOptions.UseSkeletalTracking);

            engineLock.WaitOne();

            engine.Uninitialize();
            engine.SkeletonFrameReady -= OnSkeletonFrameReady;

            foreach (IGestureRecognizer recognizer in recognizers)
            {
                recognizer.GestureRecognized -= OnGestureRecognized;
            }
        }
    }
}
