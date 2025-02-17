using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace WeatherAndFacts
{
    public class FactsController : IInitializable, IDisposable
    {
        [Inject] private FactsModel _model;
        [Inject] private FactsView _view;

        private readonly SignalBus _signalBus;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public FactsController(SignalBus signalBus)
        {
            Debug.Log("Создание FactsController!!!");
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _model.isUserOnFactsScreen.Subscribe(isOnScreen => _view.ActivateUI(isOnScreen)).AddTo(_disposable);
            
            _signalBus.Subscribe<OpenFactsSignal>(signal => _model.SetScreen(signal.activate));
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}