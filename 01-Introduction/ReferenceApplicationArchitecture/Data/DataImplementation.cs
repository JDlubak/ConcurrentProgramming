//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;
using System.Xml;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
      MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(16));      // wieksza plynnosc - ok. 60 fps
    }

    #endregion ctor

    #region DataAbstractAPI

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));
      Random random = new Random();
      for (int i = 0; i < numberOfBalls; i++)
      {
                Vector startingPosition = new(random.Next(0, 773), random.Next(0, 573));        // 2 * 4 od ramki, 20 od średnicy
                Vector ballVelocity;
                do
                {
                    ballVelocity = new((RandomGenerator.NextDouble() - 0.5) * 4, 
                                       (RandomGenerator.NextDouble() - 0.5) * 3);
                } while (ballVelocity.x == 0 || ballVelocity.y == 0);
                Ball newBall = new(startingPosition, ballVelocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
      }
    }

    #endregion DataAbstractAPI

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          MoveTimer.Dispose();
          BallsList.Clear();
        }
        Disposed = true;
      }
      else
        throw new ObjectDisposedException(nameof(DataImplementation));
    }

    public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    //private bool disposedValue;
    private bool Disposed = false;

    private readonly Timer MoveTimer;
    private Random RandomGenerator = new();
    private List<Ball> BallsList = [];

    private void Move(object? x)
    {
            foreach (Ball item in BallsList) {
                IVector velocity = item.Velocity;
                Vector position = item.GetPosition();
                double xResult = position.x + velocity.x;
                double yResult = position.y + velocity.y;
                if (xResult < 0 || xResult > 772)       // 2 * 4 od ramki, 20 od średnicy
                {
                    velocity = new Vector(-velocity.x, velocity.y);
                } 
                if (yResult < 0 || yResult > 572)       // 2 * 4 od ramki, 20 od średnicy
                {
                    velocity = new Vector(velocity.x, -velocity.y);
                }
                item.Velocity = new Vector(velocity.x, velocity.y);
                item.Move(new Vector(item.Velocity.x, item.Velocity.y));
            }
        }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}