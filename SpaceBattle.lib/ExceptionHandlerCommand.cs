namespace Server
{
    public class ExceptionHandlerCommand: ICommand
    {
        private ExceptionHandler _handler;
        private ICommand _command;
        private Exception _exception;
        public ExceptionHandlerCommand(ICommand command, Exception exception)
        {
            _command = command;
            _exception = exception;
            _handler = Hwdtech.IoC.Resolve<ExceptionHandler>("ExceptionHandler.Get");
        }
        public void Execute()
        {
            _handler.Handle(_command.GetType().ToString(), _exception);
        }
    }
}
