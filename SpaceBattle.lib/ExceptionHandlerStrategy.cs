namespace Server
{
    public class ExceptionHandlerStrategy: IStrategy
    {
        public object Execute(params object[] args)
        {
            ICommand command = (ICommand)args[0];
            Exception exception = (Exception)args[1];
            return new ExceptionHandlerCommand(command, exception);
        }
    }
}
