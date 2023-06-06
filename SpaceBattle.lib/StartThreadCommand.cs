namespace Server
{
    public class StartThreadCommand: ICommand
    {
        ServerThread _thread;
        Action? _action;
        public StartThreadCommand(ServerThread thread, Action? action)
        {
            _thread = thread;
            _action = action;
        }
        public void Execute()
        {
            _thread.Start();
            _action?.Invoke();
        }
    }
}
