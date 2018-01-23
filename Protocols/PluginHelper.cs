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
                            List<IParameters> parameterList = null;
                            try
                            {
                                parameterList = (List<IParameters>)type.InvokeMember("getParametersList", 
                                    BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, null);                                
                            }
                            catch
                            {
                                // student care cup
                            }
                            pluginsList.Add(new PluginInfo(type.Name, type.FullName, currentAssembly, type, parameterList));
                            break;
                        }
                    }
                }
            }
            return pluginsList;
        }

        public static T createInstance<T>(PluginInfo pluginInfo, object[] parameters) 
        {
            return (T)pluginInfo.assembly.CreateInstance(pluginInfo.fullName, false, BindingFlags.CreateInstance, null, parameters, null, null);
        }
    }
}
