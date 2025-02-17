using System;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Unit = UniRx.Unit;
using Zenject;

namespace WeatherAndFacts
{
    public class WeatherModel : IInitializable, IDisposable
    {
        private readonly WeatherInstaller.Settings _settings;

        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        private readonly CompositeDisposable _disposableQueue = new CompositeDisposable();

        public readonly ReactiveProperty<string> temperature = new ReactiveProperty<string>();
        public readonly ReactiveProperty<bool> isUserOnWeatherScreen = new ReactiveProperty<bool>(false);

        // Создаем Subject для обработки запросов
        private Subject<Unit> _weatherUpdateQueue;

        // Для отмены запроса
        private CancellationTokenSource _currentRequestCancellationSource = null;

        public WeatherModel(WeatherInstaller.Settings settings)
        {
            _settings = settings;
        }

        public void Initialize()
        {
            Debug.Log("Инициализация WeatherModel!!!");
            isUserOnWeatherScreen
                .Where(isOnScreen => isOnScreen)
                .Subscribe(_ => StartWeatherUpdates())
                .AddTo(_disposable);
            isUserOnWeatherScreen
                .Where(isOnScreen => !isOnScreen)
                .Subscribe(_ => ClearWeatherUpdates())
                .AddTo(_disposable);
        }

        public void SetScreen(bool activate)
        {
            isUserOnWeatherScreen.Value = activate;
            temperature.Value = "";
        }

        private void StartWeatherUpdates()
        {
            _weatherUpdateQueue = new Subject<Unit>();

            // Создаем подписку на очередь обновлений погоды
            Observable.Interval(TimeSpan.FromSeconds(_settings.weatherUpdateIntervalInSeconds))
                .Subscribe(_ => EnqueueWeatherUpdate()) // Помещаем запрос в очередь
                .AddTo(_disposableQueue);
        }

        private void EnqueueWeatherUpdate()
        {
            _weatherUpdateQueue.OnNext(Unit.Default);
            ProcessQueue();
        }

        // Метод для обработки запросов в очереди
        private void ProcessQueue()
        {
            _weatherUpdateQueue
             .Take(1) // Обрабатываем только первый запрос из очереди
             .Subscribe(async _ => await GetWeatherData())
             .AddTo(_disposableQueue);
        }

        // Метод для получения данных погоды с сервера
        private async UniTask GetWeatherData()
        {
            // Если есть активный запрос - игнорируем дальнейшую логику
            if (_currentRequestCancellationSource != null) return;

            // Создаем новый CancellationTokenSource для нового запроса
            _currentRequestCancellationSource = new CancellationTokenSource();
            var cancellationToken = _currentRequestCancellationSource.Token;

            using (UnityWebRequest request = UnityWebRequest.Get(_settings.weatherUrl))
            {
                var asyncOp = request.SendWebRequest();

                // Ожидаем завершения запроса
                await asyncOp.ToUniTask(cancellationToken: cancellationToken);

                // Если запрос был отменен, ничего не делаем
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Weather request canceled.");
                    return;
                }

                // Обработка успешного ответа
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var parsedResponse = JsonUtility.FromJson<WeatherResponse>(request.downloadHandler.text);
                    var periods = parsedResponse.properties.periods[0];
                    temperature.Value = $"{periods.temperature} {periods.temperatureUnit.ToUpper()}";
                }
                else
                {
                    Debug.LogError("Error fetching weather data: " + request.error);
                }
            }

            _currentRequestCancellationSource = null;
        }


        // Метод для очистки очереди и отписки от события
        private void ClearWeatherUpdates()
        {
            // Очистка очереди (выкидываем все события, которые могут быть в очереди)
            _weatherUpdateQueue?.OnCompleted();

            // Отписка от всех событий
            _disposableQueue?.Clear();

            // Очистка cancellation token
            _currentRequestCancellationSource?.Cancel();
            _currentRequestCancellationSource = null;
        }

        public void Dispose()
        {
            ClearWeatherUpdates();

            _disposable?.Dispose();
            _disposableQueue?.Dispose();
            _weatherUpdateQueue?.Dispose();

            Debug.Log("Weather updates queue cleared and subscription disposed.");
        }
    }

    [System.Serializable]
    public class WeatherResponse
    {
        public string type;
        public Properties properties;

        [System.Serializable]
        public class Properties
        {
            public Period[] periods;

            [System.Serializable]
            public class Period
            {
                public string name;
                public byte temperature;
                public string temperatureUnit;
            }
        }
    }
}