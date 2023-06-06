namespace Server
{
    public interface IReceiver
    {
        public ICommand Receive();
        public bool IsEmpty
        {
            get;
        }
    }
}
