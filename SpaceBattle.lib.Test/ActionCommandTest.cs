using Server;
using Moq;

namespace UnitTests
{
    public class ActionCommandTest
    {
        [Fact]
        public void Successful_Action_Command_Execution()
        {
            Mock<Action> action = new();
            action.Setup(x => x.Invoke()).Verifiable();
            ActionCommand actionCommand = new(action.Object);

            actionCommand.Execute();

            action.Verify();
        }

        [Fact]
        public void Successful_Empty_Action_Command_Execution()
        {
            ActionCommand actionCommand = new(null);

            actionCommand.Execute();
        }

        [Fact]
        public void Successful_Action_Command_Strategy_Execution()
        {
            Mock<Action> action = new();
            action.Setup(x => x.Invoke()).Verifiable();
            ActionCommandStrategy actionCommandStrategy = new();

            ICommand command = (ICommand)actionCommandStrategy.Execute(action.Object);
            command.Execute();

            action.Verify();
        }
    }
}
