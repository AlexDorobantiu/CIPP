using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using ParametersSDK;

namespace CIPPProtocols.Plugin
{
    public static class PluginHelper
    {
        private const string GET_PARAMETERS_METHOD_NAME = "getParametersList";

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
                                MethodInfo getParametersList = type.GetMethod(GET_PARAMETERS_METHOD_NAME, BindingFlags.Default | BindingFlags.Static | BindingFlags.Public);
                                if (getParametersList != null)
                                {
                                    parameterList = (List<IParameters>)getParametersList.Invoke(null, null);
                                }                            
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
