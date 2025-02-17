using UnityEngine;
using Zenject;


namespace WeatherAndFacts
{
    [CreateAssetMenu(menuName = "WeatherAndFacts/Facts Settings")]
    public class FactsSettingsInstaller : ScriptableObjectInstaller<FactsSettingsInstaller>
    {
        public FactsInstaller.Settings FactsInstaller;

        public override void InstallBindings()
        {
            Container.BindInstance(FactsInstaller);
        }
    }
}
