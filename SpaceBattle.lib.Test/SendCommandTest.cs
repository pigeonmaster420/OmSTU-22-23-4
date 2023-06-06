using Server;
using Moq;
using System.Collections.Concurrent;
using static UnitTests.IoCTestHelper;

namespace UnitTests
{
    [Collection("Threads")]
    public class SendCommandTest
    {
        [Fact]
        public void Successful_Send_Command_Execution()
        {
            BlockingCollection<ICommand> queue = new();
            Mock<ICommand> commandToSend = new(MockBehavior.Strict);
            commandToSend.Setup(x => x.Execute());
            Mock<ISender> sender = new(MockBehavior.Strict);
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(x => queue.Add(x));
            SendCommand sendCommand = new(commandToSend.Object, sender.Object);

            sendCommand.Execute();

            Assert.Contains<ICommand>(commandToSend.Object, queue);
        }
        [Fact]
        public void Successful_Send_Command_Strategy_Execution()
        {
            CreateNewScope();
            BlockingCollection<ICommand> queue = new();
            Mock<ICommand> commandToSend = new(MockBehavior.Strict);
            commandToSend.Setup(x => x.Execute());
            Mock<ISender> sender = new(MockBehavior.Strict);
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(x => queue.Add(x));
            SendCommandStrategy sendStrategy = new();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Property.Get", (object[] args) => sender.Object).Execute();

            ICommand sendCommand = (ICommand)sendStrategy.Execute(default(int), commandToSend.Object);
            sendCommand.Execute();

            Assert.Contains<ICommand>(commandToSend.Object, queue);
        }
    }
}
