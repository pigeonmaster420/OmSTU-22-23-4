namespace Server
{
    public class HardThreadStopStrategy: IStrategy
    {
        public object Execute(params object[] args)
        {
            int id = (int)args[0];
            ServerThread thread = Hwdtech.IoC.Resolve<ServerThread>("Thread.Get", id);
            Action? action = (Action?)args.ElementAtOrDefault(1);
            ICommand actionCommand = Hwdtech.IoC.Resolve<ICommand>("ActionCommand.Get", action!);
            ICommand macroCommand = Hwdtech.IoC.Resolve<MacroCommand>("MacroCommand.Get", new HardThreadStop(thread), actionCommand);
            return macroCommand;
        }
    }
}
