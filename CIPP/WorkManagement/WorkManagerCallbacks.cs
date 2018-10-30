using System;
using System.Collections.Generic;
using System.Text;

namespace CIPP.WorkManagement
{
    class WorkManagerCallbacks
    {
        public readonly addMessageCallback addMessage;
        public readonly addWorkerItemCallback addWorkerItem;
        public readonly addImageCallback addImageResult;
        public readonly addMotionCallback addMotion;
        public readonly jobFinishedCallback jobDone;
        public readonly numberChangedCallBack numberChanged;
        public readonly updateTCPListCallback updateTcpList;

        public WorkManagerCallbacks(addMessageCallback addMessage, addWorkerItemCallback addWorkerItem, addImageCallback addImageResult,
            addMotionCallback addMotion, jobFinishedCallback jobDone, numberChangedCallBack numberChanged, updateTCPListCallback updateTCPList)
        {
            this.addMessage = addMessage;
            this.addWorkerItem = addWorkerItem;
            this.addImageResult = addImageResult;
            this.addMotion = addMotion;
            this.jobDone = jobDone;
            this.numberChanged = numberChanged;
            this.updateTcpList = updateTCPList;
        }
    }
}
