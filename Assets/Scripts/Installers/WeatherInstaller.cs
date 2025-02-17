using System;
using UnityEngine;
using Zenject;

namespace WeatherAndFacts
{
    public class WeatherInstaller : MonoInstaller
    {
        [SerializeField]
        private WeatherView view;

        [Inject]
        Settings _settings = null;

        public override void InstallBindings()
        {
            Container.Bind<WeatherView>().FromInstance(view).AsSingle();
            Container.BindInterfacesAndSelfTo<WeatherModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<WeatherController>().AsSingle();
        }

        [Serializable]
        public class Settings
        {
            public string weatherUrl;
            public int weatherUpdateIntervalInSeconds;
        }
    }
}

