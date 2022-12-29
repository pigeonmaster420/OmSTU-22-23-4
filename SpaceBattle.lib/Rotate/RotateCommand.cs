namespace SpaceBattle.lib;

public class RotateCommand : ICommand
{
    IRotatable obj;
    public RotateCommand(IRotatable a)
    {
        obj = a;
    }
    public void execute()
    {
        obj.angle = obj.angle + obj.rotatespd;
    }
}