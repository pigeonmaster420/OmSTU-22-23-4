namespace Server
{
    public class SoftThreadStop: ICommand
    {
        private ServerThread _thread;
        public SoftThreadStop(ServerThread currentThread)
        {
            _thread = currentThread;
        }
        public void Execute()
        {
            ServerThread currentThread = Hwdtech.IoC.Resolve<ServerThread>("Thread.Current.Get");
            if (_thread != currentThread)
            {
                throw new InvalidOperationException($"Soft stop is forbidden for thread {_thread.GetHashCode()}");
            }
            _thread.ActionUpdate(() =>
            {
                IReceiver receiver = Hwdtech.IoC.Resolve<IReceiver>("Receiver.Get", _thread.Id);
                if (receiver.IsEmpty)
                {
                    _thread.Stop();
                    return;
                }
                ICommand command = receiver.Receive();
                try
                {
                    command.Execute();
                }
                catch(Exception exception)
                {
                    Hwdtech.IoC.Resolve<ICommand>("ExceptionHandler.Command", exception, command).Execute();
                }
            });

            Hwdtech.IoC.Resolve<ICommand>("Thread.Sender.Set.DeadEnd", _thread.Id).Execute();
        }
    }
}
