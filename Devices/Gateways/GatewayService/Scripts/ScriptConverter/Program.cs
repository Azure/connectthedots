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

namespace ScriptConverter
{
    using System;
    using System.IO;

    //--//

    class Program
    {
        const byte CR_CODE = 0x0D;
        const byte LF_CODE = 0x0A;

        //--//

        static void Main( string[] args )
        {
            foreach ( string t in args )
            {
                try
                {
                    string path = t;
                    string directory = Path.GetDirectoryName( path );
                    string file = Path.GetFileName( path );

                    Console.Out.WriteLine( "Processing dos2unix for " + file );
                    
                    string newDirectory = Path.Combine( directory, "Modified" );
                    if ( !Directory.Exists( newDirectory ) )
                    {
                        Directory.CreateDirectory( newDirectory );
                    }
                    Dos2Unix( path, Path.Combine( newDirectory, file ) );
                }
                catch ( Exception ex )
                {
                    Console.Out.WriteLine( "Exception on dos2unix: " + ex.Message );
                }
            }
        }

        private static void Dos2Unix( string inputFileName, string outputFileName )
        {
            byte[] data = File.ReadAllBytes( inputFileName );
            using( FileStream outputStream = File.OpenWrite( outputFileName ) )
            {
                BinaryWriter file = new BinaryWriter( outputStream );
                int position = 0;
                int index;
                do
                {
                    index = Array.IndexOf( data, CR_CODE, position );
                    if( ( index >= 0 ) && ( data[ index + 1 ] == LF_CODE ) )
                    {
                        // Write before the CR
                        file.Write( data, position, index - position );
                        // from LF
                        position = index + 1;
                    }
                }
                while( index > 0 );
                file.Write( data, position, data.Length - position );
                outputStream.SetLength( outputStream.Position );
                outputStream.Flush( );
                outputStream.Close( );
            }
        }
    }
}
