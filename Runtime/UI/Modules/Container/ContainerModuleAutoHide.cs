using Cysharp.Threading.Tasks;
using Evbishop.Runtime.ModulesSystem;
using System.Threading;
using UnityEngine;

namespace Evbishop.Runtime.UI.Modules.Container
{
    public class ContainerModuleAutoHide : ContainerModule,
        IModuleAwakable<UIContainer>, IModuleDisposable
    {
        [SerializeField] private float _delayInSeconds;

        private UIContainer _container;
        private CancellationTokenSource _cts;

        public void HandleAwake(UIContainer component)
        {
            _container = component;
            _container.OnShown.AddListener(HandleShown);
            _container.OnHide.AddListener(HandleHide);
        }

        public void Dispose()
        {
            if (_container)
            {
                _container.OnShown.RemoveListener(HandleShown);
                _container.OnHide.RemoveListener(HandleHide);
                _container = null;
            }
            _cts?.Cancel();
        }

        public void HandleShown()
        {
            HandleShownAsync().Forget();
        }

        private async UniTaskVoid HandleShownAsync()
        {
            _cts?.Cancel();
            _cts = new();
            bool isCancelled = await UniTask
                .WaitForSeconds(_delayInSeconds, true, cancellationToken: _cts.Token)
                .SuppressCancellationThrow();
            if (isCancelled) return;

            _container.Hide();
        }

        public void HandleHide()
        {
            _cts?.Cancel();
        }
    }
}