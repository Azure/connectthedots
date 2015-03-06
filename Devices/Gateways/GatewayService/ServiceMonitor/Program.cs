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

namespace ServiceMonitor
{
    using System;
    using System.Configuration;
    using Microsoft.ConnectTheDots.Common;

    //--//

    class Program
    {
        private static ILogger _logger = SafeLogger.FromLogger( MonitorLogger.Instance );

        //--//

        static void Main( string[] args )
        {
            // try and open the GatewayService process
            string monitoringTarget = ConfigurationManager.AppSettings.Get( "MonitoringTarget" );
            string monitoringExecutable = ConfigurationManager.AppSettings.Get( "TargetExecutable" );

            if( String.IsNullOrEmpty( monitoringTarget ) || String.IsNullOrEmpty( monitoringExecutable ) )
            {
                _logger.LogError( "Error in configuration, cannot start monitoring" );

                return;
            }

            IMonitor sm = new ServiceMonitor( monitoringTarget, _logger );
            if( sm.Lock( monitoringTarget ) )
            {
                sm.Monitor( );
            }

            IMonitor pm = new ProcessMonitor( monitoringExecutable, _logger );

            if( pm.Lock( monitoringTarget ) )
            {
                // never returns unless IMonitor.QuitMonitor is called
                pm.Monitor( );
            }

        }

    }
}
