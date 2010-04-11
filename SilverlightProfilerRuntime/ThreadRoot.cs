namespace SilverlightProfilerRuntime
{
    public class ThreadRoot : Call
    {
        private readonly int threadId;

        public ThreadRoot(int key) : base("THREAD", null)
        {
            threadId = key;
        }

        public int ThreadId
        {
            get { return threadId; }
        }

        public override bool IsThreadRoot
        {
            get { return true;}
        }
    }
}