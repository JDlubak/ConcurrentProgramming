//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI ?? ModelAbstractApi.CreateModel();
      Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
      StartCommand = new RelayCommand(ExecuteStart, CanExecuteStart);
    }

    #endregion ctor

    #region public API

    public ICommand StartCommand { get; }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

    public string BallCountText
        {
            get => _ballCountText;
            set
            {
                if (_ballCountText != value)
                {
                    _ballCountText = value;
                    RaisePropertyChanged(nameof(BallCountText));
                    (StartCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

    public Visibility InputVisibility
        {
            get => _inputVisibility;
            set
            {
                _inputVisibility = value;
                RaisePropertyChanged(nameof(InputVisibility));
            }
        }

    private bool CanExecuteStart()
        {
            return int.TryParse(BallCountText, out int result) && result > 0;
        }

    private void ExecuteStart()
        {
            int.TryParse(BallCountText, out int numberOfBalls);
            ModelLayer.Start(numberOfBalls);
            InputVisibility = Visibility.Collapsed;
            Observer.Dispose();
        }
    // na potrzeby testów

        public void Start(int ballCount)
        {
            BallCountText = ballCount.ToString();
            ExecuteStart();
        }

    #endregion public API

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          Balls.Clear();
          Observer.Dispose();
          ModelLayer.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        Disposed = true;
      }
    }

    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;
    private string _ballCountText = "";
    private Visibility _inputVisibility = Visibility.Visible;

    #endregion private
  }
}