using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace WeatherAndFacts
{
    public class GeneralModel : IDisposable
    {
        private readonly GameInstaller.Settings _settings;
        private readonly SignalBus _signalBus;

        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public GeneralModel(GameInstaller.Settings settings, SignalBus signalBus)
        {
            _settings = settings;
            _signalBus = signalBus;
        }

        public void OpenWeather()
        {
            _signalBus.Fire(new OpenWeatherSignal { activate = true });
            _signalBus.Fire(new OpenFactsSignal { activate = false });

            Debug.Log("OpenWeatherSignal Fire!!!");
        }

        public void OpenFacts()
        {
            _signalBus.Fire(new OpenFactsSignal { activate = true });
            _signalBus.Fire(new OpenWeatherSignal { activate = false });

            Debug.Log("OpenFactsSignal Fire!!!");
        }

        public void Dispose()
        {
            _disposable.Clear();
        }
    }

}