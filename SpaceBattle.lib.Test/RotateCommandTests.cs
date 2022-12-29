namespace SpaceBattle.lib.Test;
using Moq;

public class RotateTests
{
    [Fact]
    public void PositiveRotateCommand()
    {
        var mock = new Mock<IRotatable>();
        mock.SetupProperty<Rational>(m => m.angle, new Rational(45));
        mock.SetupGet<Rational>(m => m.rotatespd).Returns(new Rational(90));
        RotateCommand rotate = new RotateCommand(mock.Object);

        rotate.execute();

        Assert.True(mock.Object.angle.Equality(135,1));
    }
    [Fact]
    public void NegativeIncorrectResults()
    {
        var mock = new Mock<IRotatable>();
        mock.SetupProperty<Rational>(m => m.angle, new Rational(45));
        mock.SetupGet<Rational>(m => m.rotatespd).Returns(new Rational(90));
        RotateCommand rotate = new RotateCommand(mock.Object);

        rotate.execute();

        Assert.False(mock.Object.angle.Equality(45,1));
    }
    [Fact]
    public void NegativeCantSetAngle()
    {
        var mock = new Mock<IRotatable>();
        mock.SetupProperty<Rational>(m => m.angle, new Rational(0));
        mock.SetupGet<Rational>(m => m.rotatespd).Returns(new Rational(30));
        mock.SetupSet(m => m.angle = It.IsAny<Rational>()).Throws<Exception>();
        RotateCommand rotate = new RotateCommand(mock.Object);

        Assert.Throws<Exception>(() => rotate.execute());
    }
    [Fact]
    public void NegativeCantGetAngle()
    {
        var mock = new Mock<IRotatable>();
        mock.SetupProperty<Rational>(m => m.angle, new Rational(0));
        mock.SetupGet<Rational>(m => m.rotatespd).Returns(new Rational(0,1));
        mock.SetupGet(m => m.angle).Throws<Exception>();
        RotateCommand rotate = new RotateCommand(mock.Object);

        Assert.Throws<Exception>(() => rotate.execute());
    }
    [Fact]
    public void NegativeCantGetRotateSpeed()
    {
        var mock = new Mock<IRotatable>();
        mock.SetupProperty<Rational>(m => m.angle, new Rational(0));
        mock.SetupGet(m => m.rotatespd).Throws<Exception>();
        RotateCommand rotate = new RotateCommand(mock.Object);

        Assert.Throws<Exception>(() => rotate.execute());
    }
    [Fact]
    public void NegativeInvalidRationalNumber()
    {
        var mock = new Mock<IRotatable>();
        Assert.Throws<Exception>(() => mock.SetupProperty<Rational>(m => m.angle, new Rational(0,0)));
    }
}