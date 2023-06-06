namespace Server
{
    public class SoftThreadStopStrategy: IStrategy
    {
        public object Execute(params object[] args)
        {
            int id = (int)args[0];
            Action? action = (Action?)args.ElementAtOrDefault(1);
            ServerThread thread = Hwdtech.IoC.Resolve<ServerThread>("Thread.Get", id);
            ICommand actionCommand = Hwdtech.IoC.Resolve<ICommand>("ActionCommand.Get", action!);
            ICommand macroCommand = Hwdtech.IoC.Resolve<MacroCommand>("MacroCommand.Get", new SoftThreadStop(thread), actionCommand);
            return macroCommand;
        }
    }
}
