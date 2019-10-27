namespace CIPPProtocols
{
    public enum TrasmissionFlagsEnum
    {
        ClientName = 1,   //followed by machine name (string)
        TaskRequest = 2,
        Task = 3,         //followed by Task Package (TaskPackage)
        Result = 4,       //followed by Result Package (ResultPackage)
        AbortWork = 5,
        Listening = 6
    }
}