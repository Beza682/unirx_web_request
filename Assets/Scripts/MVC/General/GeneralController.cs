using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace WeatherAndFacts
{
    public class GeneralController : IInitializable, IDisposable
    {
        [Inject] private GeneralModel _model;
        [Inject] private GeneralView _view;

        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public GeneralController()
        {
            Debug.Log("Создание GeneralController!!!");

        }

        public void Initialize()
        {
            _view.BindWeatherButton(_model.OpenWeather);
            _view.BindFactsButton(_model.OpenFacts);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

    }
}