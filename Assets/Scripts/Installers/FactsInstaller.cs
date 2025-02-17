using System;
using UnityEngine;
using Zenject;

namespace WeatherAndFacts
{
    public class FactsInstaller : MonoInstaller
    {
        [SerializeField]
        private FactsView view;

        [Inject]
        Settings _settings = null;

        public override void InstallBindings()
        {
            Container.Bind<FactsView>().FromInstance(view).AsSingle();
            Container.BindInterfacesAndSelfTo<FactsModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<FactsController>().AsSingle();
        }

        [Serializable]
        public class Settings
        {
            public string factsUrl;
        }
    }
}

