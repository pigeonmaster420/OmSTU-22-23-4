using Server;
using Moq;
using System.Collections.Concurrent;
using static UnitTests.IoCTestHelper;

namespace UnitTests
{
    [Collection("Threads")]
    public class SoftThreadStopTest
    {
        [Fact]
        public void Successful_Soft_Stop_Execution()
        {
            CreateNewScope();

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

            Mock<ISender> sender = new(MockBehavior.Strict);
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Mock<ISender> fakeSender = new(MockBehavior.Strict);
            fakeSender.Setup(x => x.Send(It.IsAny<ICommand>())).Verifiable();

            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Sender.Set.DeadEnd", (object[] args) => new ActionCommand(() => sender = fakeSender)).Execute();

            ICommand threadStopCommand = new SoftThreadStop(thread);
            sender.Object.Send(threadStopCommand);
            Mock<ICommand> ignoredCommand = new(MockBehavior.Loose);
            Mock<ICommand> anotherCommand = new(MockBehavior.Strict);
            anotherCommand.Setup(x => x.Execute()).Callback(() => 
            {
                sender.Object.Send(ignoredCommand.Object);
            }).Verifiable();
            sender.Object.Send(anotherCommand.Object);
            
            thread.Start();
            thread.Wait();
            
            Assert.True(thread.IsStopped);
            Assert.DoesNotContain<ICommand>(ignoredCommand.Object, queue);
            anotherCommand.Verify();
            fakeSender.Verify();
        }

        [Fact]
        public void Successful_Soft_Stop_Execution_With_Exception_Handler_Activation()
        {
            CreateNewScope();

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

            Mock<ISender> sender = new(MockBehavior.Strict);
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Mock<ISender> fakeSender = new(MockBehavior.Strict);
            fakeSender.Setup(x => x.Send(It.IsAny<ICommand>())).Verifiable();

            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Sender.Set.DeadEnd", (object[] args) => new ActionCommand(() => sender = fakeSender)).Execute();
            Mock<ICommand> exceptionHandlerCommand = new(MockBehavior.Strict);
            exceptionHandlerCommand.Setup(x => x.Execute()).Verifiable();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ExceptionHandler.Command", (object[] args) => exceptionHandlerCommand.Object).Execute();

            ICommand threadStopCommand = new SoftThreadStop(thread);
            sender.Object.Send(threadStopCommand);
            Mock<ICommand> ignoredCommand = new(MockBehavior.Loose);
            Mock<ICommand> anotherCommand = new(MockBehavior.Strict);
            anotherCommand.Setup(x => x.Execute()).Callback(() => 
            {
                sender.Object.Send(ignoredCommand.Object);
            }).Throws<Exception>().Verifiable();
            sender.Object.Send(anotherCommand.Object);
            
            thread.Start();
            thread.Wait();
            
            Assert.True(thread.IsStopped);
            Assert.DoesNotContain<ICommand>(ignoredCommand.Object, queue);
            anotherCommand.Verify();
            fakeSender.Verify();
            exceptionHandlerCommand.Verify();
        }

        [Fact]
        public void Failed_Soft_Stop_Due_To_Thread_Id_Mismatch()
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
            Mock<ISender> fakeSender = new(MockBehavior.Strict);
            fakeSender.Setup(x => x.Send(It.IsAny<ICommand>())).Verifiable();

            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => targetThread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Sender.Set.DeadEnd", (object[] args) => new ActionCommand(() => sender = fakeSender)).Execute();
            Mock<ICommand> exceptionHandlerCommand = new(MockBehavior.Strict);
            exceptionHandlerCommand.Setup(x => x.Execute()).Verifiable();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ExceptionHandler.Command", (object[] args) => exceptionHandlerCommand.Object).Execute();

            Action finishingAction = () => resetEvent.Set();
            ICommand threadStopCommand = new SoftThreadStop(anotherThread);
            ICommand resetEventCommand = new ActionCommand(finishingAction);
            sender.Object.Send(threadStopCommand);
            Mock<ICommand> anotherCommand = new(MockBehavior.Strict);
            anotherCommand.Setup(x => x.Execute()).Callback(() => 
            {
                sender.Object.Send(resetEventCommand);
            }).Verifiable();
            sender.Object.Send(anotherCommand.Object);

            targetThread.Start();

            resetEvent.WaitOne();
            exceptionHandlerCommand.Verify();
            anotherCommand.Verify();
            fakeSender.VerifyNoOtherCalls();
            Assert.False(targetThread.IsStopped);
            Assert.Empty(queue);

            targetThread.Stop();
        }

        [Fact]
        public void Successful_Soft_Stop_Strategy_Execution()
        {
            CreateNewScope();

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

            Mock<ISender> sender = new(MockBehavior.Strict);
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Mock<ISender> fakeSender = new(MockBehavior.Strict);
            fakeSender.Setup(x => x.Send(It.IsAny<ICommand>())).Verifiable();

            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Sender.Set.DeadEnd", (object[] args) => new ActionCommand(() => sender = fakeSender)).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "MacroCommand.Get", (object[] args) => new MacroCommand(args.Cast<ICommand>().ToList())).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ActionCommand.Get", (object[] args) => new ActionCommandStrategy().Execute(args)).Execute();

            ICommand threadStopCommand = (ICommand)new SoftThreadStopStrategy().Execute(id);
            sender.Object.Send(threadStopCommand);
            Mock<ICommand> ignoredCommand = new(MockBehavior.Loose);
            Mock<ICommand> anotherCommand = new(MockBehavior.Strict);
            anotherCommand.Setup(x => x.Execute()).Callback(() => 
            {
                sender.Object.Send(ignoredCommand.Object);
            }).Verifiable();
            sender.Object.Send(anotherCommand.Object);
            
            thread.Start();
            thread.Wait();
            
            Assert.True(thread.IsStopped);
            Assert.DoesNotContain<ICommand>(ignoredCommand.Object, queue);
            anotherCommand.Verify();
            fakeSender.Verify();
        }

        [Fact]
        public void Successful_Soft_Stop_Strategy_Execution_With_Finishing_Action()
        {
            CreateNewScope();

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

            Mock<ISender> sender = new(MockBehavior.Strict);
            sender.Setup(x => x.Send(It.IsAny<ICommand>())).Callback<ICommand>(command => queue.Add(command));
            Mock<ISender> fakeSender = new(MockBehavior.Strict);
            fakeSender.Setup(x => x.Send(It.IsAny<ICommand>())).Verifiable();

            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Sender.New.Get", (object[] args) => sender.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Current.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Receiver.Get", (object[] args) => receiver.Object).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Sender.Set.DeadEnd", (object[] args) => new ActionCommand(() => sender = fakeSender)).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "Thread.Get", (object[] args) => thread).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "MacroCommand.Get", (object[] args) => new MacroCommand(args.Cast<ICommand>().ToList())).Execute();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ActionCommand.Get", (object[] args) => new ActionCommandStrategy().Execute(args)).Execute();

            Mock<Action> finishingAction = new(MockBehavior.Strict);
            finishingAction.Setup(x => x.Invoke()).Verifiable();
            ICommand threadStopCommand = (ICommand)new SoftThreadStopStrategy().Execute(id, finishingAction.Object);
            sender.Object.Send(threadStopCommand);
            Mock<ICommand> ignoredCommand = new(MockBehavior.Loose);
            Mock<ICommand> anotherCommand = new(MockBehavior.Strict);
            anotherCommand.Setup(x => x.Execute()).Callback(() => 
            {
                sender.Object.Send(ignoredCommand.Object);
            }).Verifiable();
            sender.Object.Send(anotherCommand.Object);
            
            thread.Start();
            thread.Wait();
            
            Assert.True(thread.IsStopped);
            Assert.DoesNotContain<ICommand>(ignoredCommand.Object, queue);
            finishingAction.Verify();
            anotherCommand.Verify();
            fakeSender.Verify();
        }
    }
}
