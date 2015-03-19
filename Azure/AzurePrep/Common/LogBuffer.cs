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

namespace Microsoft.ConnectTheDots.CloudDeploy.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    //--//

    public class LogBuffer
    {
        private readonly List<string>         _Buffer;
        private readonly Action<string>       _OnMessageAdd;

        //--//

        public LogBuffer( Action<string> onMessageAdd )
        {
            _Buffer = new List<string>( );
            _OnMessageAdd = onMessageAdd;
        }

        public void Add( string messageLine )
        {
            lock( _Buffer )
            {
                _Buffer.Add( messageLine );

                if ( _OnMessageAdd != null )
                {
                    _OnMessageAdd.Invoke( messageLine );
                }
            }
        }

        public bool FlushToFile( string fileName )
        {
            try
            {
                lock( _Buffer )
                {
                    using( StreamWriter file = new StreamWriter( fileName ) )
                    {
                        foreach( var messageLine in _Buffer )
                        {
                            file.WriteLine( messageLine );
                        }
                        file.Flush( );
                        file.Close( );
                    }
                    _Buffer.Clear( );
                }
            }
            catch ( Exception )
            {
                return false;
            }

            return true;
        }
    }
}
