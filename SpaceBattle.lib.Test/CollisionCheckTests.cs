using Hwdtech;
using Moq;

namespace SpaceBattle.lib.Test;
public class CollisionCheckCommandTests {
    public CollisionCheckCommandTests() {
        new Hwdtech.Ioc.InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var RStrategy = new Mock<IStrategy>();
        RStrategy.Setup(m => m.executeStrategy(It.IsAny<object[]>())).Returns(new List<int>());

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Collision.GetList", (object[] args) => RStrategy.Object.executeStrategy(args)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Collision.GetDeltas", (object[] args) => new GetDeltasStrategy().executeStrategy(args)).Execute();
    }

    [Fact]
    public void CollisionCheckTrue() {
        var uobj_1 = new Mock<IUObject>();
        var uobj_2 = new Mock<IUObject>();

        var CHReturns = new Mock<IStrategy>();
        CHReturns.Setup(m => m.executeStrategy(It.IsAny<object[]>())).Returns((object) true);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Collision.CheckWithTree", (object[] args) => CHReturns.Object.executeStrategy(args)).Execute();
        
        ICommand ColCheck = new CollisionCheck(uobj_1.Object, uobj_2.Object);
        Assert.ThrowsAny<Exception>(() => ColCheck.execute());
    }

    [Fact]
    public void CollisionCheckFalse() {   

        var uobj_1 = new Mock<IUObject>();
        var uobj_2 = new Mock<IUObject>();

        var CHReturns = new Mock<IStrategy>();
        CHReturns.Setup(m => m.executeStrategy(It.IsAny<object[]>())).Returns((object) false);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Collision.CheckWithTree", (object[] args) => CHReturns.Object.executeStrategy(args)).Execute();
        
        ICommand ColCheck = new CollisionCheck(uobj_1.Object, uobj_2.Object);
        ColCheck.execute();
    }

    [Fact]
    public void CollisionCheckNull() {
        var uobj_1 = new Mock<IUObject>();
        var uobj_2 = new Mock<IUObject>();

        var CHReturns = new Mock<IStrategy>();
        CHReturns.Setup(m => m.executeStrategy(It.IsAny<object[]>())).Throws((new NullReferenceException()));
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Collision.CheckWithTree", (object[] args) => CHReturns.Object.executeStrategy(args)).Execute();
        
        ICommand ColCheck = new CollisionCheck(uobj_1.Object, uobj_2.Object);
        Assert.ThrowsAny<Exception>(() => ColCheck.execute());
    }
}