using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

using ParametersSDK;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;

namespace CIPPProtocols
{
    public class PluginInfo
    {
        public string displayName;
        public string fullName;
        public Assembly assembly;
        public Type type;
        public List<IParameters> parameters;

        public PluginInfo(string displayName, string fullName, Assembly assembly, Type type, List<IParameters> parameters)
        {
            this.displayName = displayName;
            this.fullName = fullName;
            this.assembly = assembly;
            this.type = type;
            this.parameters = parameters;
        }
    }

    public class PluginHelper
    {
        private static List<Assembly> loadPlugInAssemblies(string path)
        {
            DirectoryInfo dInfo = new DirectoryInfo(path);
            FileInfo[] files = dInfo.GetFiles("*.dll");
            List<Assembly> plugInAssemblyList = new List<Assembly>();

            if (files != null)
            {
                foreach (FileInfo file in files)
                {
                    plugInAssemblyList.Add(Assembly.LoadFile(file.FullName));
                }
            }
            return plugInAssemblyList;
        }

        public static List<PluginInfo> getPluginsList(string path, Type searchedInterfaceType)
        {
            List<PluginInfo> pluginsList = new List<PluginInfo>();
            List<Assembly> assemblyList = loadPlugInAssemblies(path);

            foreach (Assembly currentAssembly in assemblyList)
            {
                foreach (Type type in currentAssembly.GetTypes())
                {
                    foreach (Type interfaceType in type.GetInterfaces())
                    {
                        if (interfaceType.Equals(searchedInterfaceType))
                        {
                            try
                            {
                                List<IParameters> parameterList = (List<IParameters>)type.InvokeMember("getParametersList", BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, null);
                                pluginsList.Add(new PluginInfo(type.Name, type.FullName, currentAssembly, type, parameterList));
                                break;
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            return pluginsList;
        }
    }
}
