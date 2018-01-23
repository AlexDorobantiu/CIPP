using System;
using System.Collections.Generic;
using System.Text;

namespace CIPPProtocols
{
    public class PluginFinder
    {
        private Dictionary<TaskTypeEnum, Dictionary<string, PluginInfo>> map;

        public PluginFinder(List<PluginInfo> filterPluginList, List<PluginInfo> maskPluginList, List<PluginInfo> motionRecognitionPluginList)
        {
            updatePluginLists(filterPluginList, maskPluginList, motionRecognitionPluginList);
        }

        public void updatePluginLists(List<PluginInfo> filterPluginList, List<PluginInfo> maskPluginList, List<PluginInfo> motionRecognitionPluginList)
        {
            map = new Dictionary<TaskTypeEnum, Dictionary<string, PluginInfo>>();
            map.Add(TaskTypeEnum.filter, new Dictionary<string, PluginInfo>());
            map.Add(TaskTypeEnum.mask, new Dictionary<string, PluginInfo>());
            map.Add(TaskTypeEnum.motionRecognition, new Dictionary<string, PluginInfo>());

            foreach (PluginInfo plugin in filterPluginList)
            {
                map[TaskTypeEnum.filter].Add(plugin.fullName, plugin);
            }

            foreach (PluginInfo plugin in maskPluginList)
            {
                map[TaskTypeEnum.mask].Add(plugin.fullName, plugin);
            }

            foreach (PluginInfo plugin in motionRecognitionPluginList)
            {
                map[TaskTypeEnum.motionRecognition].Add(plugin.fullName, plugin);
            }
        }

        public PluginInfo findPluginForTask(Task task)
        {
            return map[task.taskType][task.pluginFullName];
        }
    }
}
