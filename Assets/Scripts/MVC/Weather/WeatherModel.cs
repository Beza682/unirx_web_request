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

        // ������� Subject ��� ��������� ��������
        private Subject<Unit> _weatherUpdateQueue;

        // ��� ������ �������
        private CancellationTokenSource _currentRequestCancellationSource = null;

        public WeatherModel(WeatherInstaller.Settings settings)
        {
            _settings = settings;
        }

        public void Initialize()
        {
            Debug.Log("������������� WeatherModel!!!");
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

            // ������� �������� �� ������� ���������� ������
            Observable.Interval(TimeSpan.FromSeconds(_settings.weatherUpdateIntervalInSeconds))
                .Subscribe(_ => EnqueueWeatherUpdate()) // �������� ������ � �������
                .AddTo(_disposableQueue);
        }

        private void EnqueueWeatherUpdate()
        {
            _weatherUpdateQueue.OnNext(Unit.Default);
            ProcessQueue();
        }

        // ����� ��� ��������� �������� � �������
        private void ProcessQueue()
        {
            _weatherUpdateQueue
             .Take(1) // ������������ ������ ������ ������ �� �������
             .Subscribe(async _ => await GetWeatherData())
             .AddTo(_disposableQueue);
        }

        // ����� ��� ��������� ������ ������ � �������
        private async UniTask GetWeatherData()
        {
            // ���� ���� �������� ������ - ���������� ���������� ������
            if (_currentRequestCancellationSource != null) return;

            // ������� ����� CancellationTokenSource ��� ������ �������
            _currentRequestCancellationSource = new CancellationTokenSource();
            var cancellationToken = _currentRequestCancellationSource.Token;

            using (UnityWebRequest request = UnityWebRequest.Get(_settings.weatherUrl))
            {
                var asyncOp = request.SendWebRequest();

                // ������� ���������� �������
                await asyncOp.ToUniTask(cancellationToken: cancellationToken);

                // ���� ������ ��� �������, ������ �� ������
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Weather request canceled.");
                    return;
                }

                // ��������� ��������� ������
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


        // ����� ��� ������� ������� � ������� �� �������
        private void ClearWeatherUpdates()
        {
            // ������� ������� (���������� ��� �������, ������� ����� ���� � �������)
            _weatherUpdateQueue?.OnCompleted();

            // ������� �� ���� �������
            _disposableQueue?.Clear();

            // ������� cancellation token
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