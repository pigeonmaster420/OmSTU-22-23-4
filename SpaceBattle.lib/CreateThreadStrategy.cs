namespace Server
{
    public class CreateThreadStrategy: IStrategy
    {
        public object Execute(params object[] args)
        {
            int id = (int)args[0];
            Action? action = (Action?)args.ElementAtOrDefault(1);
            IEnumerable<ICommand> queue = Hwdtech.IoC.Resolve<IEnumerable<ICommand>>("Queue.New.Get");
            IReceiver receiver = Hwdtech.IoC.Resolve<IReceiver>("Receiver.New.Get", queue);
            object parentScope = Hwdtech.IoC.Resolve<object>("Scopes.Current");
            ServerThread newThread = new(id, parentScope);
            ISender sender = Hwdtech.IoC.Resolve<ISender>("Sender.New.Get", queue);
            Hwdtech.IoC.Resolve<ICommand>("Thread.New.Add", id, newThread).Execute();
            Hwdtech.IoC.Resolve<ICommand>("Thread.Add.Property", id, "Receiver", receiver).Execute();
            Hwdtech.IoC.Resolve<ICommand>("Thread.Add.Property", id, "Sender", sender).Execute();
            ICommand command = new StartThreadCommand(newThread, action);
            return command;
        }
    }
}
