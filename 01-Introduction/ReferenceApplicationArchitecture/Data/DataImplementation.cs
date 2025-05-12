using System.Diagnostics;
using System.Runtime.Intrinsics.X86;


namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation() {}

    #endregion ctor

    #region DataAbstractAPI

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));
      for (int i = 0; i < numberOfBalls; i++)
      {
                Vector startingPosition;
                bool isGoodPosition = false;
                do
                {
                    startingPosition = new(RandomGenerator.Next(0, 773), RandomGenerator.Next(0, 573));        // 2 * 4 od ramki, 20 od średnicy
                    isGoodPosition = true;
                    foreach (var ball in BallsList)
                    {
                        Vector ballPosition = ball.GetPosition();
                        double distance = Math.Sqrt(Math.Pow(startingPosition.x - ballPosition.x, 2) + Math.Pow(startingPosition.y - ballPosition.y, 2));
                        if (distance <= 25)
                        {
                            isGoodPosition = false;
                            break;
                        }
                    }
                } while (!isGoodPosition);
                Vector ballVelocity;
                do
                {
                    ballVelocity = new((RandomGenerator.NextDouble() - 0.5) * 4, 
                                       (RandomGenerator.NextDouble() - 0.5) * 3);
                } while (ballVelocity.x == 0 || ballVelocity.y == 0);
                int randomMassValue = RandomGenerator.Next(1, 5);
                Mass randomMass = 0;
                switch (randomMassValue)
                {
                    case 1:
                        randomMass = Mass.Light;
                        break;
                    case 2:
                        randomMass = Mass.Medium;
                        break;
                    case 3:
                        randomMass = Mass.Heavy;
                        break;
                    case 4:
                        randomMass = Mass.VeryHeavy;
                        break;
                }
                Ball newBall = new(startingPosition, ballVelocity, randomMass);
                upperLayerHandler(startingPosition, newBall);
                lock (_lock)
                {
                    BallsList.Add(newBall);
                }
                Task.Run(() => Move(newBall));
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
                    lock (_lock)
                    {
                        foreach (var thread in ThreadsList)
                        {
                            if (thread.IsAlive)
                            {
                                thread.Join();
                            }
                        }
                        ThreadsList.Clear();
                        BallsList.Clear();
                    }
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
    private Random RandomGenerator = new();
    private List<Ball> BallsList = [];
    private List<Thread> ThreadsList = new();
    private readonly object _lock = new();
    private readonly Dictionary<(Ball, Ball), DateTime> _collisionCooldowns = new Dictionary<(Ball, Ball), DateTime>();
    private readonly TimeSpan _collisionCooldown = TimeSpan.FromMilliseconds(100);


    private (Ball, Ball) GetCollisionKey(Ball a, Ball b)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(a) <= System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(b)
                ? (a, b)
                : (b, a);
        }
        private async Task Move(Ball ball)
        {
            while (!Disposed)
            {
                lock (_lock)
                {
                    foreach (var anotherBall in BallsList)
                    {
                        if (ball == anotherBall) continue;
                        if (CollisionDetector(ball, anotherBall))
                        {
                            var key = GetCollisionKey(ball, anotherBall);
                            if (_collisionCooldowns.TryGetValue(key, out DateTime lastCollisionTime))
                            {
                                if (DateTime.Now - lastCollisionTime < _collisionCooldown)
                                {
                                    continue;
                                }
                            }
                            CollisionMovement(ball, anotherBall);
                            _collisionCooldowns[key] = DateTime.Now;
                        }

                    }
                        IVector velocity = ball.Velocity;
                        Vector position = ball.GetPosition();
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
                        ball.Move(new Vector(velocity.x, velocity.y));
                        ball.Velocity = velocity;

                    

                }
                await Task.Delay(16);
            }
        }
    private bool CollisionDetector(Ball b1, Ball b2)
        {
            Vector b1pos = b1.GetPosition();
            Vector b2pos = b2.GetPosition();
            double distance = Math.Sqrt(Math.Pow(b1pos.x - b2pos.x, 2) + Math.Pow(b1pos.y - b2pos.y, 2));
            return distance <= 20;
        }

        private void CollisionMovement(Ball a, Ball b)
        {
                {   
                    // Pobranie mas
                    int ma = (int)a.Mass;
                    int mb = (int)b.Mass;

                    // Pobranie pozycji
                    Vector aPos = a.GetPosition();
                    Vector bPos = b.GetPosition();
                    
                    // obliczenie różnicy w pozycji i dystansu między kulkami
                    Vector difference = new Vector(bPos.x - aPos.x, bPos.y - aPos.y);
                    double distance = Math.Sqrt(difference.x * difference.x + difference.y * difference.y);
                    
                    // Normalizacja wektora i utworzenie wektora stycznego do niego
                    Vector normalVector = new Vector(difference.x / distance, difference.y / distance);
                    Vector tangentVector = new Vector(-normalVector.y, normalVector.x);
                    
                    // Pobranie prędkości kulek
                    IVector vA = a.Velocity;
                    IVector vB = b.Velocity;

                    // Przeskalowanie do znormalizowanych i stycznych prędkości
                    double vA_normalized = vA.x * normalVector.x + vA.y * normalVector.y;
                    double vB_normalized = vB.x * normalVector.x + vB.y * normalVector.y;
                    double vA_tangent = vA.x * tangentVector.x + vA.y * tangentVector.y;
                    double vB_tangent = vB.x * tangentVector.x + vB.y * tangentVector.y;

                    // Dodatkowe informacje do debugowania kolizji
                    Debug.WriteLine($"Before collision: ma = {ma}, mb = {mb}");
                    Debug.WriteLine($"a.position = ({Math.Round(aPos.x, 2)}, {Math.Round(aPos.y, 2)}), a.velocity = ({Math.Round(vA.x, 2)}, {Math.Round(vA.y, 2)})");
                    Debug.WriteLine($"b.position = ({Math.Round(bPos.x, 2)}, {Math.Round(bPos.y, 2)}), b.velocity = ({Math.Round(vB.x, 2)}, {Math.Round(vB.y, 2)})");
                    
                    // przeliczenie prędkości nowych
                    double newV_A_normal = (vA_normalized * (ma - mb) + 2 * mb * vB_normalized) / (ma + mb);
                    double newV_B_normal = (vB_normalized * (mb - ma) + 2 * ma * vA_normalized) / (ma + mb);

                    // rozłożenie prędkości na składowe
                    double newVax = newV_A_normal * normalVector.x + vA_tangent * tangentVector.x;
                    double newVay = newV_A_normal * normalVector.y + vA_tangent * tangentVector.y;
                    double newVbx = newV_B_normal * normalVector.x + vB_tangent * tangentVector.x;
                    double newVby = newV_B_normal * normalVector.y + vB_tangent * tangentVector.y;
                    
                    // aktualizacja prędkości
                    a.Velocity = new Vector(newVax, newVay);
                    b.Velocity = new Vector(newVbx, newVby);

                    // Dodatkowe informacje do debugowania kolizji
                    Debug.WriteLine($"After collision: ");
                    Debug.WriteLine($"a.position = ({Math.Round(aPos.x, 2)}, {Math.Round(aPos.y, 2)}), a.velocity = ({Math.Round(a.Velocity.x, 2)}, {Math.Round(a.Velocity.y, 2)})");
                    Debug.WriteLine($"b.position = ({Math.Round(bPos.x, 2)}, {Math.Round(bPos.y, 2)}), b.velocity = ({Math.Round(b.Velocity.x, 2)}, {Math.Round(b.Velocity.y, 2)})");
                    Debug.WriteLine("");

            }
        }

        internal bool callCollisionCheckerForTesting(Ball b1, Ball b2)
        {
            return CollisionDetector(b1, b2);
        }

        internal void simulateCollisionToTest(Ball b1, Ball b2)
        {
            CollisionMovement(b1, b2);
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