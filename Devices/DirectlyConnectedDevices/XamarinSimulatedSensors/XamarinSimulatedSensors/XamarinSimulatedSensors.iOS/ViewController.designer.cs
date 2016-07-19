// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace XamarinSimulatedSensors.iOS
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton buttonConnect { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton buttonSend { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblConnectionString { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblHumidity { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblTemperature { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISlider sliderHumidity { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISlider sliderTemperature { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField textConnectionString { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField textDisplayName { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (buttonConnect != null) {
                buttonConnect.Dispose ();
                buttonConnect = null;
            }

            if (buttonSend != null) {
                buttonSend.Dispose ();
                buttonSend = null;
            }

            if (lblConnectionString != null) {
                lblConnectionString.Dispose ();
                lblConnectionString = null;
            }

            if (lblHumidity != null) {
                lblHumidity.Dispose ();
                lblHumidity = null;
            }

            if (lblTemperature != null) {
                lblTemperature.Dispose ();
                lblTemperature = null;
            }

            if (sliderHumidity != null) {
                sliderHumidity.Dispose ();
                sliderHumidity = null;
            }

            if (sliderTemperature != null) {
                sliderTemperature.Dispose ();
                sliderTemperature = null;
            }

            if (textConnectionString != null) {
                textConnectionString.Dispose ();
                textConnectionString = null;
            }

            if (textDisplayName != null) {
                textDisplayName.Dispose ();
                textDisplayName = null;
            }
        }
    }
}