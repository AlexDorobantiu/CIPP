using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CIPP.Utils
{
    static class ReflectionUtil
    {
        public static string getCurrentExeDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}
