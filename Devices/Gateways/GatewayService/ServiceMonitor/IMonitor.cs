using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ServiceMonitor
{
    interface IMonitor
    {
        bool Lock( string monitoringTarget );

        void Monitor( );

        void QuitMonitor( );
    }
}
