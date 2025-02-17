using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace WeatherAndFacts
{
    public class WeatherController : IInitializable, IDisposable
    {
        [Inject] private WeatherModel _model;
        [Inject] private WeatherView _view;

        private readonly SignalBus _signalBus;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        // Start is called before the first frame update
        public WeatherController(SignalBus signalBus)
        {
            Debug.Log("Создание WeatherController!!!");
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _model.temperature.Subscribe(temp => _view.SetTemperature(temp)).AddTo(_disposable);
            _model.isUserOnWeatherScreen.Subscribe(isOnScreen => _view.ActivateUI(isOnScreen)).AddTo(_disposable);
            
            _signalBus.Subscribe<OpenWeatherSignal>((signal) => _model.SetScreen(signal.activate));
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

    }
}