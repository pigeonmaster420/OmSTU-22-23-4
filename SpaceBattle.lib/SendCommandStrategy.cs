namespace Server
{
    public class SendCommandStrategy: IStrategy
    {
        public object Execute(params object[] args)
        {
            int id = (int)args[0];
            ICommand command = (ICommand)args[1];
            ISender sender = Hwdtech.IoC.Resolve<ISender>("Thread.Property.Get", id, "Sender");
            return new SendCommand(command, sender);
        }
    }
}
