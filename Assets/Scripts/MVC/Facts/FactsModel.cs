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
    public class FactsModel : IInitializable, IDisposable
    {
        private readonly FactsInstaller.Settings _settings;

        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        private readonly CompositeDisposable _disposableQueue = new CompositeDisposable();

        public readonly ReactiveProperty<bool> isUserOnFactsScreen = new ReactiveProperty<bool>(false);

        // Создаем Subject для обработки запросов
        private Subject<Unit> _factsUpdateQueue;

        // Для отмены запроса
        private CancellationTokenSource _currentRequestCancellationSource = null;

        public FactsModel(FactsInstaller.Settings settings)
        {
            _settings = settings;
        }

        public void Initialize()
        {
            Debug.Log("Инициализация FactsModel!!!");
            isUserOnFactsScreen
                .Where(isOnScreen => isOnScreen)
                .Subscribe(_ => StartFactsUpdates())
                .AddTo(_disposable);
            isUserOnFactsScreen
                .Where(isOnScreen => !isOnScreen)
                .Subscribe(_ => ClearFactsUpdates())
                .AddTo(_disposable);
        }

        public void SetScreen(bool activate)
        {
            isUserOnFactsScreen.Value = activate;
        }

        private void StartFactsUpdates()
        {
            _factsUpdateQueue = new Subject<Unit>();

            //Observable.Interval(TimeSpan.FromSeconds(5))
            //    .Subscribe(_ => EnqueueFactsUpdate()) // Помещаем запрос в очередь
            //    .AddTo(_disposableQueue);

            // Создаем подписку на очередь получения фактов
            Observable.Start(() => EnqueueFactsUpdate())
                .Subscribe(_ => EnqueueFactsUpdate())
                .AddTo(_disposableQueue);
        }

        private void EnqueueFactsUpdate()
        {
            Debug.Log("Start facts EnqueueFactsUpdate!");
            _factsUpdateQueue.OnNext(Unit.Default);
            ProcessQueue();
        }

        // Метод для обработки запросов в очереди
        private void ProcessQueue()
        {
            _factsUpdateQueue
             .Take(1) // Обрабатываем только первый запрос из очереди
             .Subscribe(async _ => await GetFactsData())
             .AddTo(_disposableQueue);

            Debug.Log("End facts ProcessQueue!");
        }

        // Метод для получения данных погоды с сервера
        private async UniTask GetFactsData()
        {
            Debug.Log("Start facts GetFactsData!");
            // Если есть активный запрос - игнорируем дальнейшую логику
            if (_currentRequestCancellationSource != null) return;
            Debug.Log("condition");

            // Создаем новый CancellationTokenSource для нового запроса
            _currentRequestCancellationSource = new CancellationTokenSource();
            var cancellationToken = _currentRequestCancellationSource.Token;

            using (UnityWebRequest request = UnityWebRequest.Get(_settings.factsUrl))
            {
                var asyncOp = request.SendWebRequest();

                // Ожидаем завершения запроса
                await asyncOp.ToUniTask(cancellationToken: cancellationToken);

                // Если запрос был отменен, ничего не делаем
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Facts request canceled.");
                    return;
                }

                // Обработка успешного ответа
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var parsedResponse = JsonUtility.FromJson<FactsResponse>(request.downloadHandler.text);
                    var periods = parsedResponse.data[0];
                    Debug.Log($"{periods.type} {periods.id}");
                }
                else
                {
                    Debug.LogError("Error fetching facts data: " + request.error);
                }
            }

            _currentRequestCancellationSource = null;
        }



        // Метод для очистки очереди и отписки от события
        private void ClearFactsUpdates()
        {
            // Очистка очереди (выкидываем все события, которые могут быть в очереди)
            _factsUpdateQueue?.OnCompleted();

            // Отписка от всех событий
            _disposableQueue?.Clear();

            // Очистка cancellation token
            _currentRequestCancellationSource?.Cancel();
            _currentRequestCancellationSource = null;
        }

        public void Dispose()
        {
            ClearFactsUpdates();

            _disposable?.Dispose();
            _disposableQueue?.Dispose();
            _factsUpdateQueue?.Dispose();

            Debug.Log("Facts updates queue cleared and subscription disposed.");
        }
    }

    [System.Serializable]
    public class FactsResponse
    {
        public Fact[] data;

        [System.Serializable]
        public class Fact
        {
            public string id;
            public string type;

        }
    }
}