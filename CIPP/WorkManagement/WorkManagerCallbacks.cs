namespace CIPP.WorkManagement
{
    class WorkManagerCallbacks
    {
        public readonly addMessageCallback addMessage;
        public readonly addWorkerItemCallback addWorkerItem;
        public readonly addImageCallback addImageResult;
        public readonly addMotionCallback addMotion;
        public readonly jobFinishedCallback jobDone;
        public readonly numberChangedCallback numberChanged;
        public readonly updateTCPListCallback updateTcpList;

        public WorkManagerCallbacks(addMessageCallback addMessage, addWorkerItemCallback addWorkerItem, addImageCallback addImageResult,
            addMotionCallback addMotion, jobFinishedCallback jobDone, numberChangedCallback numberChanged, updateTCPListCallback updateTCPList)
        {
            this.addMessage = addMessage;
            this.addWorkerItem = addWorkerItem;
            this.addImageResult = addImageResult;
            this.addMotion = addMotion;
            this.jobDone = jobDone;
            this.numberChanged = numberChanged;
            updateTcpList = updateTCPList;
        }
    }
}
