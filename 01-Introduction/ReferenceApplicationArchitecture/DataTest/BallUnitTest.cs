using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.DataTest
{
  [TestClass]
  public class BallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      Vector testinVector = new(0.0, 0.0);
      Ball newInstance = new(testinVector, testinVector, Mass.Light);
            Assert.AreEqual(testinVector, newInstance.GetPosition());

        }

        [TestMethod]
    public void MoveTestMethod()
    {
      Vector initialPosition = new(10.0, 10.0);
      Ball newInstance = new(initialPosition, new Vector(0.0, 0.0), Mass.Light);
      IVector curentPosition = new Vector(0.0, 0.0);
      int numberOfCallBackCalled = 0;
      newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };
      newInstance.Move(new Vector(0.0, 0.0));
      Assert.AreEqual<int>(1, numberOfCallBackCalled);
      Assert.AreEqual<IVector>(initialPosition, curentPosition);
    }

        [TestMethod]
        public void GetPositionTestMethod()
        {
            Vector initialPosition = new(20.0, 20.0);
            Ball newBall = new(initialPosition, initialPosition, Mass.Light);
            Assert.AreEqual(initialPosition, newBall.GetPosition());
        }

        [TestMethod]
        public void MassTestMethod()
        {
            Mass m1 = Mass.Light;
            Mass m2 = Mass.VeryHeavy;
            Mass m3 = Mass.Medium;
            Mass m4 = Mass.Heavy;
            Assert.IsTrue(m2 > m1);
            Assert.AreEqual((int)m2 - (int)m1, 9);
            Assert.AreEqual((int)m1 + (int)m3 + (int)m4, (int)m2);
        }


    }
}