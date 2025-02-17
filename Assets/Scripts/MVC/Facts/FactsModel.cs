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

        // ������� Subject ��� ��������� ��������
        private Subject<Unit> _factsUpdateQueue;

        // ��� ������ �������
        private CancellationTokenSource _currentRequestCancellationSource = null;

        public FactsModel(FactsInstaller.Settings settings)
        {
            _settings = settings;
        }

        public void Initialize()
        {
            Debug.Log("������������� FactsModel!!!");
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
            //    .Subscribe(_ => EnqueueFactsUpdate()) // �������� ������ � �������
            //    .AddTo(_disposableQueue);

            // ������� �������� �� ������� ��������� ������
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

        // ����� ��� ��������� �������� � �������
        private void ProcessQueue()
        {
            _factsUpdateQueue
             .Take(1) // ������������ ������ ������ ������ �� �������
             .Subscribe(async _ => await GetFactsData())
             .AddTo(_disposableQueue);

            Debug.Log("End facts ProcessQueue!");
        }

        // ����� ��� ��������� ������ ������ � �������
        private async UniTask GetFactsData()
        {
            Debug.Log("Start facts GetFactsData!");
            // ���� ���� �������� ������ - ���������� ���������� ������
            if (_currentRequestCancellationSource != null) return;
            Debug.Log("condition");

            // ������� ����� CancellationTokenSource ��� ������ �������
            _currentRequestCancellationSource = new CancellationTokenSource();
            var cancellationToken = _currentRequestCancellationSource.Token;

            using (UnityWebRequest request = UnityWebRequest.Get(_settings.factsUrl))
            {
                var asyncOp = request.SendWebRequest();

                // ������� ���������� �������
                await asyncOp.ToUniTask(cancellationToken: cancellationToken);

                // ���� ������ ��� �������, ������ �� ������
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Facts request canceled.");
                    return;
                }

                // ��������� ��������� ������
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



        // ����� ��� ������� ������� � ������� �� �������
        private void ClearFactsUpdates()
        {
            // ������� ������� (���������� ��� �������, ������� ����� ���� � �������)
            _factsUpdateQueue?.OnCompleted();

            // ������� �� ���� �������
            _disposableQueue?.Clear();

            // ������� cancellation token
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