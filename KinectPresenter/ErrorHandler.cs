using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectPresenter
{
    public class ErrorHandler
    {
        private static string KINECT_NOT_FOUND_MESSAGE = "KINECT_NOT_FOUND_MESSAGE";
        private static string KINECT_NOT_FOUND_CAPTION = "KINECT_NOT_FOUND_CAPTION";

        private static string SLIDE_SHOW_MODE_KINECT_NOT_FOUND_MESSAGE = "SLIDE_SHOW_MODE_KINECT_NOT_FOUND_MESSAGE";
        private static string SLIDE_SHOW_MODE_KINECT_NOT_FOUND_CAPTION = "SLIDE_SHOW_MODE_KINECT_NOT_FOUND_CAPTION";

        public static void ShowKinectNotFoundDialog()
        {
            System.Windows.Forms.MessageBox.Show(KINECT_NOT_FOUND_MESSAGE, KINECT_NOT_FOUND_CAPTION, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
        }

        public static void ShowSlideShowModeKinectNotFoundDialog()
        {
            System.Windows.Forms.MessageBox.Show(SLIDE_SHOW_MODE_KINECT_NOT_FOUND_MESSAGE, SLIDE_SHOW_MODE_KINECT_NOT_FOUND_CAPTION, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
        }
    }
}
