using CIPPProtocols.Plugin;
using CIPPProtocols.Tasks;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;
using System;

namespace CIPPProtocols
{
    public class TaskHelper
    {
        public static void solveTask(Task task, PluginFinder pluginFinder)
        {
            try
            {
                PluginInfo pluginInfo = pluginFinder.findPluginForTask(task);
                switch (task.type)
                {
                    case Task.Type.FILTER:
                        FilterTask filterTask = (FilterTask)task;
                        IFilter filter = PluginHelper.createInstance<IFilter>(pluginInfo, filterTask.parameters);
                        filterTask.result = filter.filter(filterTask.originalImage);
                        break;
                    case Task.Type.MASK:
                        MaskTask maskTask = (MaskTask)task;
                        IMask mask = PluginHelper.createInstance<IMask>(pluginInfo, maskTask.parameters);
                        maskTask.result = mask.mask(maskTask.originalImage);
                        break;
                    case Task.Type.MOTION_RECOGNITION:
                        MotionRecognitionTask motionRecognitionTask = (MotionRecognitionTask)task;
                        IMotionRecognition motionRecognition = PluginHelper.createInstance<IMotionRecognition>(pluginInfo, motionRecognitionTask.parameters);
                        motionRecognitionTask.result = motionRecognition.scan(motionRecognitionTask.frame, motionRecognitionTask.nextFrame);
                        break;
                }
                task.status = Task.Status.SUCCESSFUL;
            }
            catch (Exception e)
            {
                task.status = Task.Status.FAILED;
                task.exception = e;
            }
        }
    }
}