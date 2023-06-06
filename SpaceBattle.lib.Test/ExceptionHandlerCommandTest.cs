using Server;
using Moq;
using static UnitTests.IoCTestHelper;

namespace UnitTests
{
    [CollectionDefinition("ExceptionHandler", DisableParallelization = true)]
    [Collection("ExceptionHandler")]
    public class ExceptionHandlerCommandTest
    {
        [Fact]
        public void Successful_Exception_Handler_Command_Execution()
        {
            CreateNewScope();
            ExceptionHandler exceptionHandler = new();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ExceptionHandler.Get", (object[] args) => exceptionHandler).Execute();
            Mock<ICommand> handlerCommand = new(MockBehavior.Strict);
            handlerCommand.Setup(x => x.Execute()).Verifiable();
            exceptionHandler.AddExceptionHandler("Server.Rotation", new NullReferenceException(), handlerCommand.Object);
            ICommand failingCommand = new Rotation(null!);

            try
            {
                failingCommand.Execute();
            }
            catch(Exception exception)
            {
                new ExceptionHandlerCommand(failingCommand, exception).Execute();
            }

            handlerCommand.Verify();
        }

        [Fact]
        public void Successful_Exception_Handler_Strategy_Execution()
        {
            CreateNewScope();
            ExceptionHandler exceptionHandler = new();
            Hwdtech.IoC.Resolve<Hwdtech.ICommand>(RegisterStrategy, "ExceptionHandler.Get", (object[] args) => exceptionHandler).Execute();
            Mock<ICommand> handlerCommand = new(MockBehavior.Strict);
            handlerCommand.Setup(x => x.Execute()).Verifiable();
            exceptionHandler.AddExceptionHandler("Server.Rotation", new NullReferenceException(), handlerCommand.Object);
            ICommand failingCommand = new Rotation(null!);

            try
            {
                failingCommand.Execute();
            }
            catch(Exception exception)
            {
                ICommand exceptionHandlerCommand = (ICommand)new ExceptionHandlerStrategy().Execute(failingCommand, exception);
                exceptionHandlerCommand.Execute();
            }

            handlerCommand.Verify();
        }
    }
}
