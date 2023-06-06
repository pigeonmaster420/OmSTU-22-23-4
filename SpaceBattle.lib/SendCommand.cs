namespace Server
{
    public class SendCommand: ICommand
    {
        private readonly ICommand _commandToSend;
        private readonly ISender _sender;
        public SendCommand(ICommand command, ISender sender)
        {
            _commandToSend = command;
            _sender = sender;
        }
        public void Execute()
        {
            _sender.Send(_commandToSend);
        }
    }
}
