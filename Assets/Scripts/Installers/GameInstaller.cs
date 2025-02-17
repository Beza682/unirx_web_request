using System;
using UnityEngine;
using Zenject;

namespace WeatherAndFacts
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField]
        private GeneralView view;

        [Inject]
        Settings _settings = null;

        public override void InstallBindings()
        {
            Container.Bind<GeneralView>().FromInstance(view).AsSingle();
            Container.BindInterfacesAndSelfTo<GeneralModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<GeneralController>().AsSingle();
            
            InstallSignals();
        }

        void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            // Signals can be useful for game-wide events that could have many interested parties
            Container.DeclareSignal<OpenWeatherSignal>();
            Container.DeclareSignal<OpenFactsSignal>();
        }

        [Serializable]
        public class Settings
        {

        }
    }
}

