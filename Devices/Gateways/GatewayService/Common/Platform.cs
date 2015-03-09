using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ConnectTheDots.Common
{
    public static class Platform
    {
        public static bool IsMono
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().ImageRuntimeVersion == "?";
            }
        }
    }
}
