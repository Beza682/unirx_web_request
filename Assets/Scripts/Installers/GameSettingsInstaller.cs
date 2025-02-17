using UnityEngine;
using Zenject;


namespace WeatherAndFacts
{
    [CreateAssetMenu(menuName = "WeatherAndFacts/Game Settings")]
    public class GameSettingsInstaller : ScriptableObjectInstaller<GameSettingsInstaller>
    {
        public GameInstaller.Settings GameInstaller;

        public override void InstallBindings()
        {
            Container.BindInstance(GameInstaller);
        }
    }
}
