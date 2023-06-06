using System.Collections.Concurrent;
using Moq;
using Server;
using static UnitTests.IoCTestHelper;

namespace UnitTests
{
    [CollectionDefinition("Threads", DisableParallelization = true)]
    [Collection("Threads")]
    public class ThreadTest
    {
        [Fact]
        public void Successful_Thread_Commands_Execution()
        {
            CreateNewScope();
            ManualResetEvent resetEvent = new(initialState: false);
            BlockingCollection<ICommand> queue = new();
            ServerThread thread = new(id: default, parentScope: Hwdtech.IoC.Resolve<object>("Scopes.Current"));
            Mock<IReceiver> receiver = new(MockBehavior.Strict);
            receiver.Setup(x => x.Receive()).Returns(() => queue.Take());
            Mock<ICommand> emptyCommand = new(MockBehavior.Strict);
            emptyCommand.Setup(x => x.Execute()).Verifiable();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();

            queue.Add(emptyCommand.Object);
            queue.Add(new ActionCommand(() => 
            {
                thread.Stop();
                resetEvent.Set();
            }));
            thread.Start();

            resetEvent.WaitOne();
            emptyCommand.Verify();
            Assert.True(thread.IsStopped);
            Assert.Empty(queue);
        }

        [Fact]
        public void Successful_Thread_Termination_After_Exception_Handling()
        {
            CreateNewScope();
            ManualResetEvent resetEvent = new(initialState: false);
            ServerThread thread = new(id: default, parentScope: Hwdtech.IoC.Resolve<object>("Scopes.Current"));
            Mock<IReceiver> receiver = new(MockBehavior.Strict);
            Mock<ICommand> emptyCommand = new(MockBehavior.Strict);
            emptyCommand.Setup(x => x.Execute()).Throws(new Exception());
            receiver.Setup(x => x.Receive()).Returns(() => emptyCommand.Object);
            Mock<ICommand> exceptionHandlerCommand = new(MockBehavior.Strict);
            exceptionHandlerCommand.Setup(x => x.Execute()).Callback(() => 
            {
                thread.Stop();
                resetEvent.Set();
            });
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ExceptionHandler.Command", (object[] args) => exceptionHandlerCommand.Object).Execute();

            thread.Start();

            resetEvent.WaitOne();
            Assert.True(thread.IsStopped);
        }
    }

    [CollectionDefinition("Threads")]
    [Collection("Threads")]
    public class ThreadStrategyTest
    {
        [Fact]
        public void Successful_Single_Thread_Creation_Strategy_Execution()
        {
            CreateNewScope();
            ManualResetEvent resetEvent = new(initialState: false);

            BlockingCollection<ICommand> queue = new();
            Mock<IReceiver> receiver = new(MockBehavior.Strict);
            receiver.Setup(x => x.Receive()).Returns(() => 
            {
                ICommand command = queue.Take();
                return command;
            });
            receiver.Setup(x => x.IsEmpty).Returns(() => queue.Count == 0);
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.New.Get", (object[] args) => receiver.Object).Execute();

            Mock<ISender> sender = new();
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();

            Mock<IDictionary<int, ServerThread>> threadsCollection = new(MockBehavior.Strict);
            threadsCollection.Setup(x => x.Add(It.IsAny<int>(), It.IsAny<ServerThread>())).Verifiable();

            Mock<IDictionary<int, IDictionary<string, object>>> threadsProperties = new(MockBehavior.Strict);
            Mock<IDictionary<string, object>> threadProperty = new();
            threadProperty.Setup(x => x.Add(It.Is<string>(x => x == "Receiver"), It.IsAny<IReceiver>())).Verifiable();
            threadProperty.Setup(x => x.Add(It.Is<string>(x => x == "Sender"), It.IsAny<ISender>())).Verifiable();
            threadsProperties.Setup(x => x[It.IsAny<int>()]).Returns<int>(key => threadProperty.Object);
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.New.Add", (object[] args) => new ActionCommand(() => threadsCollection.Object.Add((int)args[0], (ServerThread)args[1]))).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Add.Property", (object[] args) => new ActionCommand(() => 
            {
                threadsProperties.Object[(int)args[0]].Add((string)args[1], args[2]);
            })).Execute();

            sender.Object.Send(new ActionCommand(() =>
            {
                resetEvent.Set();
            }));

            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Queue.New.Get", (object[] args) => queue).Execute();

            int id = default;
            ICommand startThread = (ICommand)new CreateThreadStrategy().Execute(id);

            startThread.Execute();
            resetEvent.WaitOne();

            threadsCollection.Verify();
            threadProperty.Verify();
            Assert.Empty(queue);
            Assert.True(receiver.Object.IsEmpty);
        }

        [Fact]
        public void Successful_Single_Thread_Creation_Strategy_Execution_With_Starting_Action()
        {
            CreateNewScope();
            ManualResetEvent resetEvent = new(initialState: false);

            BlockingCollection<ICommand> queue = new();
            Mock<IReceiver> receiver = new(MockBehavior.Strict);
            receiver.Setup(x => x.Receive()).Returns(() => 
            {
                ICommand command = queue.Take();
                return command;
            });
            receiver.Setup(x => x.IsEmpty).Returns(() => queue.Count == 0);
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.New.Get", (object[] args) => receiver.Object).Execute();

            Mock<ISender> sender = new();
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();

            Mock<IDictionary<int, ServerThread>> threadsCollection = new(MockBehavior.Strict);
            threadsCollection.Setup(x => x.Add(It.IsAny<int>(), It.IsAny<ServerThread>())).Verifiable();

            Mock<IDictionary<int, IDictionary<string, object>>> threadsProperties = new(MockBehavior.Strict);
            Mock<IDictionary<string, object>> threadProperty = new();
            threadProperty.Setup(x => x.Add(It.Is<string>(x => x == "Receiver"), It.IsAny<IReceiver>())).Verifiable();
            threadProperty.Setup(x => x.Add(It.Is<string>(x => x == "Sender"), It.IsAny<ISender>())).Verifiable();
            threadsProperties.Setup(x => x[It.IsAny<int>()]).Returns<int>(key => threadProperty.Object);
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.New.Add", (object[] args) => new ActionCommand(() => threadsCollection.Object.Add((int)args[0], (ServerThread)args[1]))).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Add.Property", (object[] args) => new ActionCommand(() => 
            {
                threadsProperties.Object[(int)args[0]].Add((string)args[1], args[2]);
            })).Execute();

            sender.Object.Send(new ActionCommand(() =>
            {
                resetEvent.Set();
            }));

            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Queue.New.Get", (object[] args) => queue).Execute();

            int id = default;
            Mock<Action> startingAction = new(MockBehavior.Strict);
            startingAction.Setup(x => x.Invoke()).Verifiable();
            ICommand startThread = (ICommand)new CreateThreadStrategy().Execute(id, startingAction.Object);

            startThread.Execute();
            resetEvent.WaitOne();

            threadsCollection.Verify();
            threadProperty.Verify();
            startingAction.Verify();
            Assert.Empty(queue);
            Assert.True(receiver.Object.IsEmpty);
        }

        [Fact]
        public void Successful_Multiple_Threads_Creation_Strategies_Execution()
        {
            CreateNewScope();
            Barrier barrier = new(participantCount: 3);

            List<BlockingCollection<ICommand>> queues = new(){new(), new()};
            List<Mock<IReceiver>> receivers = new(){new(), new()};
            List<Mock<ISender>> senders = new(){new(), new()};
            for (int i = 0; i < 2; i++)
            {
                var queue = queues[i];
                receivers[i].Setup(x => x.Receive()).Returns(() => queue.Take());
                receivers[i].Setup(x => x.IsEmpty).Returns(() => queue.Count == 0);
                senders[i].Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            }

            senders[0].Object.Send(new ActionCommand(() => barrier.SignalAndWait()));
            senders[1].Object.Send(new ActionCommand(() => barrier.SignalAndWait()));

            int receiverId = 0;
            int senderId = 0;
            int queueId = 0;
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.New.Get", (object[] args) => receivers[receiverId++].Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => senders[senderId++].Object).Execute();

            Dictionary<int, ServerThread> threadsCollection = new();

            Mock<IDictionary<int, IDictionary<string, object>>> threadsProperties = new(MockBehavior.Strict);
            Mock<IDictionary<string, object>> threadProperty = new();
            threadProperty.Setup(x => x.Add(It.Is<string>(x => x == "Receiver"), It.IsAny<IReceiver>())).Verifiable();
            threadProperty.Setup(x => x.Add(It.Is<string>(x => x == "Sender"), It.IsAny<ISender>())).Verifiable();
            threadsProperties.Setup(x => x[It.IsAny<int>()]).Returns<int>(key => threadProperty.Object);

            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.New.Add", (object[] args) => new ActionCommand(() => threadsCollection.Add((int)args[0], (ServerThread)args[1]))).Execute();
            var a = (object[] args) => new ActionCommand(() => threadsProperties.Object[(int)args[0]].Add((string)args[1], args[2])).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Add.Property", (object[] args) => new ActionCommand(() => 
            {
                threadsProperties.Object[(int)args[0]].Add((string)args[1], args[2]);
            })).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Queue.New.Get", (object[] args) => queues[queueId++]).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receivers[(int)args[0]].Object).Execute();

            for (int i = 0; i < 2; i++)
            {
                var sender = senders[i];
                var receiver = receivers[i];

                ICommand startThread = (ICommand)new CreateThreadStrategy().Execute(i);
                startThread.Execute();
            }

            barrier.SignalAndWait();

            Assert.NotEmpty(threadsCollection);
            threadProperty.Verify();
            Assert.True(queues.TrueForAll(x => x.Count == 0));
            Assert.True(receivers.TrueForAll(x => x.Object.IsEmpty));

            threadsCollection.Values.ToList().ForEach(thread => thread.Stop());
        }
    }
}
