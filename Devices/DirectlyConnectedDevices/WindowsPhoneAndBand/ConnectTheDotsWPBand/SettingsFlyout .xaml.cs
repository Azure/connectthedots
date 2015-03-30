//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ConnectTheDotsWPBand
{
    public sealed partial class SettingsFlyout : UserControl
    {
        private AppSettings settings;

        public SettingsFlyout()
        {

            this.InitializeComponent();
            settings = (AppSettings)Resources["appSettings"];

            this.ScrollViewer.Height = Window.Current.Bounds.Height;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }

        }

        private void txtServiceBusName_LostFocus(object sender, RoutedEventArgs e)
        {
            settings.ServicebusNamespace= this.txtServiceBusName.Text;
        }

        private void txtEventHubName_LostFocus(object sender, RoutedEventArgs e)
        {
            settings.EventHubName = this.txtEventHubName.Text;
        }

        private void txtKeyName_LostFocus(object sender, RoutedEventArgs e)
        {
            settings.KeyName = this.txtKeyName.Text;
        }

        private void txtKey_LostFocus(object sender, RoutedEventArgs e)
        {
            settings.Key = this.txtKey.Text;
        }

        private void txtDisplayName_LostFocus(object sender, RoutedEventArgs e)
        {
            settings.DisplayName = this.txtDisplayName.Text;
        }

        private void txtOrganization_LostFocus(object sender, RoutedEventArgs e)
        {
            settings.Organization = this.txtOrganization.Text;
        }

        private void txtLocation_LostFocus(object sender, RoutedEventArgs e)
        {
            settings.Location = this.txtLocation.Text;
        }
    }
}
