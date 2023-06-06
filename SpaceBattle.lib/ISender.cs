namespace Server
{
    public interface ISender
    {
        public void Send(ICommand command);
    }
}
