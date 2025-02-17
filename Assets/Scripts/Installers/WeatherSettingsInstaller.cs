using UnityEngine;
using Zenject;


namespace WeatherAndFacts
{
    [CreateAssetMenu(menuName = "WeatherAndFacts/Weather Settings")]
    public class WeatherSettingsInstaller : ScriptableObjectInstaller<WeatherSettingsInstaller>
    {
        public WeatherInstaller.Settings WeatherInstaller;

        public override void InstallBindings()
        {
            Container.BindInstance(WeatherInstaller);
        }
    }
}
