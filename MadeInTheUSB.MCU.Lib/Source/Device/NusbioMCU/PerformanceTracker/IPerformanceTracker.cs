namespace MadeInTheUSB.MCU
{
    public interface IPerformanceTracker
    {
        void AddByte(long byteCount);
        string GetByteSecondSentStatus(bool reset = false);
        void ResetBytePerSecondCounters();
    }
}