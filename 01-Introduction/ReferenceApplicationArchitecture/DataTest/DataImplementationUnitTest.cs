//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class DataImplementationUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      using (DataImplementation newInstance = new DataImplementation())
      {
        IEnumerable<IBall>? ballsList = null;
        newInstance.CheckBallsList(x => ballsList = x);
        Assert.IsNotNull(ballsList);
        int numberOfBalls = 0;
        newInstance.CheckNumberOfBalls(x => numberOfBalls = x);
        Assert.AreEqual<int>(0, numberOfBalls);
      }
    }

    [TestMethod]
    public void DisposeTestMethod()
    {
      DataImplementation newInstance = new DataImplementation();
      bool newInstanceDisposed = false;
      newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
      Assert.IsFalse(newInstanceDisposed);
      newInstance.Dispose();
      newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
      Assert.IsTrue(newInstanceDisposed);
      IEnumerable<IBall>? ballsList = null;
      newInstance.CheckBallsList(x => ballsList = x);
      Assert.IsNotNull(ballsList);
      newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(0, x));
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
    }

    [TestMethod]
    public void StartTestMethod()
    {
      using (DataImplementation newInstance = new DataImplementation())
      {
        int numberOfCallbackInvoked = 0;
        int numberOfBalls2Create = 10;
        newInstance.Start(
          numberOfBalls2Create,
          (startingPosition, ball) =>
          {
            numberOfCallbackInvoked++;
            Assert.IsTrue(startingPosition.x >= 0);
            Assert.IsTrue(startingPosition.y >= 0);
            Assert.IsNotNull(ball);
          });
        Assert.AreEqual<int>(numberOfBalls2Create, numberOfCallbackInvoked);
        newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(10, x));
      }
    }

        [TestMethod]

        public void CollisionMovementTest()
        {
            Vector b1pos = new(0, 0);
            Vector b2pos = new(15, 15);
            Vector b1vel = new(2, 3);
            Vector b2vel = new(5, 1);
            Ball b1 = new(b1pos, b1vel, Mass.Medium);
            Ball b2 = new(b2pos, b2vel, Mass.Heavy);
            DataImplementation di = new();
            di.simulateCollisionToTest(b1, b2);
            Vector b1vel2 = (Vector)b1.Velocity;
            Vector b2vel2 = (Vector)b2.Velocity;
            Vector expb1 = new(2.67, 3.67);
            Vector expb2 = new(4.67, 0.67);
            Assert.AreEqual(expb1.x, b1vel2.x, 0.01);
            Assert.AreEqual(expb1.y, b1vel2.y, 0.01);
            Assert.AreEqual(expb2.x, b2vel2.x, 0.01);
            Assert.AreEqual(expb2.y, b2vel2.y, 0.01);
        }

        [TestMethod]

        public void CollisionTestMethod()
        {
            Vector initialPosition = new(20.0, 20.0);
            Ball newBall = new(initialPosition, initialPosition, Mass.Light);
            DataImplementation di = new();
            Assert.IsTrue(di.callCollisionCheckerForTesting(newBall, newBall)); // w zasadzie to nie ma sensu, ale po prostu zakładam, że 2 piłka znajduje sie na tej samej pozycji - kolizja występuje
            Vector notCollidingPosition = new(40.0, 40.0);
            Ball notCollidingBall = new(notCollidingPosition, initialPosition, Mass.Light);
            Assert.IsFalse(di.callCollisionCheckerForTesting(newBall, notCollidingBall));
        }
    }
}