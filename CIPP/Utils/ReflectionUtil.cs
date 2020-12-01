using System.IO;
using System.Reflection;

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
