﻿using System.Collections.Generic;

namespace CIPPProtocols.Plugin
{
    public class PluginFinder
    {
        private readonly Dictionary<Task.Type, Dictionary<string, PluginInfo>> map = new Dictionary<Task.Type, Dictionary<string, PluginInfo>>();

        public void updatePluginLists(List<PluginInfo> filterPluginList, List<PluginInfo> maskPluginList, List<PluginInfo> motionRecognitionPluginList)
        {
            map.Clear();
            map.Add(Task.Type.FILTER, new Dictionary<string, PluginInfo>());
            map.Add(Task.Type.MASK, new Dictionary<string, PluginInfo>());
            map.Add(Task.Type.MOTION_RECOGNITION, new Dictionary<string, PluginInfo>());

            foreach (PluginInfo plugin in filterPluginList)
            {
                map[Task.Type.FILTER].Add(plugin.fullName, plugin);
            }

            foreach (PluginInfo plugin in maskPluginList)
            {
                map[Task.Type.MASK].Add(plugin.fullName, plugin);
            }

            foreach (PluginInfo plugin in motionRecognitionPluginList)
            {
                map[Task.Type.MOTION_RECOGNITION].Add(plugin.fullName, plugin);
            }
        }

        public PluginInfo findPluginForTask(Task task)
        {
            return map[task.type][task.pluginFullName];
        }
    }
}
