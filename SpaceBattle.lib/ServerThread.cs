namespace Server
{
    public class ServerThread
    {
        private Action _action;
        private Thread _thread;
        private bool _isStopped;
        public bool IsStopped
        {
            get => _isStopped;
        }
        public void Stop() => _isStopped = true;
        private readonly int _id;
        private void CreateNewScope(object parentScope)
        {
            object newScope = Hwdtech.IoC.Resolve<object>("Scopes.New", parentScope);
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", newScope).Execute();
        }
        public int Id
        {
            get => _id;
        }
        public void ActionUpdate(Action action)
        {
            _action = action;
        }
        
        public ServerThread(int id, object parentScope)
        {
            _id = id;
            _action = () =>
            {
                ICommand command = Hwdtech.IoC.Resolve<IReceiver>("Receiver.Get", Id).Receive();
                try
                {
                    command.Execute();
                }
                catch(Exception exception)
                {
                    Hwdtech.IoC.Resolve<ICommand>("ExceptionHandler.Command", command, exception).Execute();
                }
            };
            _thread = new(() =>
            {
                CreateNewScope(parentScope);
                
                while(!_isStopped)
                {
                    _action();
                }
            });
        }

        public void Start()
        {
            _isStopped = false;
            _thread.Start();
        }

        public void Wait() => _thread.Join();

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
