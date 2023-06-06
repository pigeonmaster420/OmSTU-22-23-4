namespace Server
{
    public class ActionCommandStrategy: IStrategy
    {
        public object Execute(params object[] args)
        {
            Action? action = (Action?)args[0];
            return new ActionCommand(action);
        }
    }
}
