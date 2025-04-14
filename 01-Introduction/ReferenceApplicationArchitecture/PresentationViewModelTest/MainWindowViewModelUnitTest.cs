//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using TP.ConcurrentProgramming.Presentation.Model;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel.Test
{
  [TestClass]
  public class MainWindowViewModelUnitTest
  {
    [TestMethod]
    public void ConstructorTest()
    {
      ModelNullFixture nullModelFixture = new();
      Assert.AreEqual<int>(0, nullModelFixture.Disposed);
      Assert.AreEqual<int>(0, nullModelFixture.Started);
      Assert.AreEqual<int>(0, nullModelFixture.Subscribed);
      using (MainWindowViewModel viewModel = new(nullModelFixture))
      {
        Random random = new Random();
        int numberOfBalls = random.Next(1, 10);
        viewModel.Start(numberOfBalls);
        Assert.IsNotNull(viewModel.Balls);
        Assert.AreEqual<int>(0, nullModelFixture.Disposed);
        Assert.AreEqual<int>(numberOfBalls, nullModelFixture.Started);
        Assert.AreEqual<int>(1, nullModelFixture.Subscribed);
      }
      Assert.AreEqual<int>(1, nullModelFixture.Disposed);
    }

    [TestMethod]
    public void BehaviorTestMethod()
    {
      ModelSimulatorFixture modelSimulator = new();
      MainWindowViewModel viewModel = new(modelSimulator);
      Assert.IsNotNull(viewModel.Balls);
      Assert.AreEqual<int>(0, viewModel.Balls.Count);
      Random random = new Random();
      int numberOfBalls = random.Next(1, 10);
      viewModel.Start(numberOfBalls);
      Assert.AreEqual<int>(numberOfBalls, viewModel.Balls.Count);
      viewModel.Dispose();
      Assert.IsTrue(modelSimulator.Disposed);
      Assert.AreEqual<int>(0, viewModel.Balls.Count);
    }

        [TestMethod]
        // testowanie prawid³owej reakcji odnoœnie mo¿liwoœci startu w zale¿noœci od danych wejœciowych
        public void BallCountTextValidationTestMethod()
        {
            ModelNullFixture model = new();
            MainWindowViewModel viewModel = new(model);
            bool propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.BallCountText))
                    propertyChangedRaised = true;
            };

            viewModel.BallCountText = "abc";
            bool cannotExecuteWithText = viewModel.StartCommand.CanExecute(null);

            viewModel.BallCountText = "5"; 
            bool canExecute = viewModel.StartCommand.CanExecute(null);

            viewModel.BallCountText = "0";
            bool cannotExecuteWithZero = viewModel.StartCommand.CanExecute(null);

            viewModel.BallCountText = "";
            bool cannotExecuteAfterRemovingInput = viewModel.StartCommand.CanExecute(null);

            viewModel.BallCountText = "-1";
            bool cannotExecuteWithNegativeNumber = viewModel.StartCommand.CanExecute(null);

            viewModel.BallCountText = "1.0";
            bool cannotExecuteWithDouble = viewModel.StartCommand.CanExecute(null);

            Assert.IsTrue(propertyChangedRaised, "PropertyChanged nie zosta³o wywo³ane");
            Assert.IsFalse(cannotExecuteWithText, "Wpisano tekst - nie mo¿na wystartowaæ");
            Assert.IsTrue(canExecute, "Wpisano poprawne dane - mo¿na wystartowaæ");
            Assert.IsFalse(cannotExecuteWithZero, "Wpisano zero - nie mo¿na wystartowaæ");
            Assert.IsFalse(cannotExecuteAfterRemovingInput, "Usuniêto dane - nie mo¿na wystartowaæ");
            Assert.IsFalse(cannotExecuteWithNegativeNumber, "Wpisano ujemn¹ liczbê - nie mo¿na wystartowaæ");
            Assert.IsFalse(cannotExecuteWithDouble, "Wpisano liczbê zmiennoprzecinkow¹ - nie mo¿na wystartowaæ");

        }

        [TestMethod]
        // testowanie prawid³owoœci zmiany tekstu w polu tekstowym
        public void BallCountTextChangeTestMethod()
        {
            ModelNullFixture model = new();
            MainWindowViewModel viewModel = new(model);
            Assert.IsTrue(viewModel.BallCountText.Equals(""), "Pocz¹tkowy tekst jest pusty");
            viewModel.BallCountText = "tekst";
            Assert.IsTrue(viewModel.BallCountText.Equals("tekst"), "Zmieniliœmy tekst wiêc mia³ siê zmieniæ");

        }

        [TestMethod]
        // testowanie prawid³owej reakcji odnoœnie mo¿liwoœci startu w zale¿noœci od danych wejœciowych
        public void VisibilityTestMethod()
        {
            ModelNullFixture model = new();
            MainWindowViewModel viewModel = new(model);
            Assert.AreEqual(System.Windows.Visibility.Visible, viewModel.InputVisibility, "Pocz¹tkowa widocznoœæ - Visible.");
            viewModel.Start(5);
            Assert.AreEqual(System.Windows.Visibility.Collapsed, viewModel.InputVisibility, "Koñcowa widocznoœæ - Collapsed.");
        }

        #region testing infrastructure

        private class ModelNullFixture : ModelAbstractApi
    {
      #region Test

      internal int Disposed = 0;
      internal int Started = 0;
      internal int Subscribed = 0;

      #endregion Test

      #region ModelAbstractApi

      public override void Dispose()
      {
        Disposed++;
      }

      public override void Start(int numberOfBalls)
      {
        Started = numberOfBalls;
      }

      public override IDisposable Subscribe(IObserver<ModelIBall> observer)
      {
        Subscribed++;
        return new NullDisposable();
      }

      #endregion ModelAbstractApi

      #region private

      private class NullDisposable : IDisposable
      {
        public void Dispose()
        { }
      }

      #endregion private
    }

    private class ModelSimulatorFixture : ModelAbstractApi
    {
      #region Testing indicators

      internal bool Disposed = false;

      #endregion Testing indicators

      #region ctor

      public ModelSimulatorFixture()
      {
        eventObservable = Observable.FromEventPattern<BallChaneEventArgs>(this, "BallChanged");
      }

      #endregion ctor

      #region ModelAbstractApi fixture

      public override IDisposable? Subscribe(IObserver<ModelIBall> observer)
      {
        return eventObservable?.Subscribe(x => observer.OnNext(x.EventArgs.Ball), ex => observer.OnError(ex), () => observer.OnCompleted());
      }

      public override void Start(int numberOfBalls)
      {
        for (int i = 0; i < numberOfBalls; i++)
        {
          ModelBall newBall = new ModelBall(0, 0) { };
          BallChanged?.Invoke(this, new BallChaneEventArgs() { Ball = newBall });
        }
      }

      public override void Dispose()
      {
        Disposed = true;
      }

      #endregion ModelAbstractApi

      #region API

      public event EventHandler<BallChaneEventArgs> BallChanged;

      #endregion API

      #region private

      private IObservable<EventPattern<BallChaneEventArgs>>? eventObservable = null;

      private class ModelBall : ModelIBall
      {
        public ModelBall(double top, double left)
        { }

        #region IBall

        public double Diameter => throw new NotImplementedException();

        public double Top => throw new NotImplementedException();

        public double Left => throw new NotImplementedException();

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion INotifyPropertyChanged

        #endregion IBall
      }

      #endregion private
    }

    #endregion testing infrastructure
  }
}