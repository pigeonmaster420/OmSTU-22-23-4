namespace Server
{
    public class HardThreadStop: ICommand
    {
        private ServerThread _thread;
        public HardThreadStop(ServerThread currentThread)
        {
            _thread = currentThread;
        }
        public void Execute()
        {
            ServerThread currentThread = Hwdtech.IoC.Resolve<ServerThread>("Thread.Current.Get");
            if (_thread != currentThread)
            {
                throw new InvalidOperationException($"Hard stop is forbidden for thread {_thread.GetHashCode()}");
            }
            _thread.Stop();
        }
    }
}
