using Server;
using System.Collections.Concurrent;
using Moq;
using static UnitTests.IoCTestHelper;

namespace UnitTests
{
    [Collection("Threads")]
    public class HardThreadStopTest
    {
        [Fact]
        public void Successful_Hard_Stop_Execution()
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
            int id = default;
            ServerThread thread = new(id: id, parentScope: Hwdtech.IoC.Resolve<object>("Scopes.Current"));

            Mock<ISender> sender = new();
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();

            Action finishingAction = () => resetEvent.Set();
            ICommand threadStopCommand = new HardThreadStop(thread);
            ICommand threadStopCommandWithResetEvent = new MacroCommand(new ICommand[]{threadStopCommand, new ActionCommand(finishingAction)});
            sender.Object.Send(threadStopCommandWithResetEvent);
            Mock<ICommand> ignoredCommand = new(MockBehavior.Loose);
            sender.Object.Send(ignoredCommand.Object);

            thread.Start();

            resetEvent.WaitOne();
            Assert.True(thread.IsStopped);
            Assert.Contains<ICommand>(ignoredCommand.Object, queue);
        }

        [Fact]
        public void Failed_Hard_Stop_Due_To_Thread_Id_Mismatch()
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
            ServerThread targetThread = new(id: 1, parentScope: Hwdtech.IoC.Resolve<object>("Scopes.Current"));
            ServerThread anotherThread = new(id: 2, parentScope: Hwdtech.IoC.Resolve<object>("Scopes.Current"));

            Mock<ISender> sender = new();
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => targetThread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Mock<ICommand> exceptionHandlerCommand = new(MockBehavior.Strict);
            exceptionHandlerCommand.Setup(x => x.Execute()).Verifiable();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ExceptionHandler.Command", (object[] args) => exceptionHandlerCommand.Object).Execute();

            Action finishingAction = () => resetEvent.Set();
            ICommand threadStopCommand = new HardThreadStop(anotherThread);
            ICommand resetEventCommand = new ActionCommand(finishingAction);
            sender.Object.Send(threadStopCommand);
            sender.Object.Send(resetEventCommand);

            targetThread.Start();

            resetEvent.WaitOne();
            exceptionHandlerCommand.Verify();
            Assert.False(targetThread.IsStopped);
            Assert.Empty(queue);

            targetThread.Stop();
        }

        [Fact]
        public void Successful_Hard_Stop_Strategy_Execution()
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
            int id = default;
            ServerThread thread = new(id: id, parentScope: Hwdtech.IoC.Resolve<object>("Scopes.Current"));

            Mock<ISender> sender = new();
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "MacroCommand.Get", (object[] args) => new MacroCommand(args.Cast<ICommand>().ToList())).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ActionCommand.Get", (object[] args) => new ActionCommandStrategy().Execute(args)).Execute();

            ICommand threadStop = (ICommand)new HardThreadStopStrategy().Execute(id);
            ICommand resetEventCommand = new ActionCommand(() => resetEvent.Set());
            ICommand threadStopWithResetEvent = new MacroCommand(new ICommand[] {threadStop, resetEventCommand});
            sender.Object.Send(threadStopWithResetEvent);
            Mock<ICommand> ignoredCommand = new(MockBehavior.Loose);
            sender.Object.Send(ignoredCommand.Object);

            thread.Start();

            resetEvent.WaitOne();
            Assert.True(thread.IsStopped);
            Assert.Contains<ICommand>(ignoredCommand.Object, queue);
        }

        [Fact]
        public void Successful_Hard_Stop_Strategy_Execution_With_Finishing_Action()
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
            int id = default;
            ServerThread thread = new(id: id, parentScope: Hwdtech.IoC.Resolve<object>("Scopes.Current"));

            Mock<ISender> sender = new();
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "MacroCommand.Get", (object[] args) => new MacroCommand(args.Cast<ICommand>().ToList())).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ActionCommand.Get", (object[] args) => new ActionCommandStrategy().Execute(args)).Execute();

            Action finishingAction = () => resetEvent.Set();
            ICommand threadStop = (ICommand)new HardThreadStopStrategy().Execute(id, finishingAction);
            sender.Object.Send(threadStop);
            Mock<ICommand> ignoredCommand = new(MockBehavior.Loose);
            sender.Object.Send(ignoredCommand.Object);

            thread.Start();

            resetEvent.WaitOne();
            Assert.True(thread.IsStopped);
            Assert.Contains<ICommand>(ignoredCommand.Object, queue);
        }
    }
}
