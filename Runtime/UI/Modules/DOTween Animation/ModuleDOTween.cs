using DG.Tweening;
using Evbishop.Runtime.ModulesSystem;
using Evbishop.Runtime.UI.Modules.Animator;
using UnityEngine;

namespace Evbishop.Runtime.UI.Modules.DOTween_Animation
{
    public enum EPlayMode : byte
    {
        Normal = 0,
        PingPong = 1,
    }

    public abstract class ModuleDOTween : AnimatorModule, IModuleDisposable
    {
        [SerializeField] protected float duration = 1;
        [SerializeField] protected Ease easePlay = Ease.OutQuad;
        [SerializeField] protected Ease easeReversePlay = Ease.InQuad;
        [SerializeField, Sirenix.OdinInspector.EnumToggleButtons] protected EPlayMode playMode;

        protected Sequence sequence;

        public void Dispose()
        {
            sequence?.Kill();
            sequence = null;
        }

        protected void InitSequence()
        {
            sequence?.Kill();
            sequence = DOTween
                .Sequence()
                .SetAutoKill(true);
            if (playMode == EPlayMode.PingPong)
            {
                sequence.SetLoops(1, LoopType.Yoyo);
            }
        }

        public override void Play()
        {
            base.Play();

            InitSequence();
        }

        public override void PlayReverse()
        {
            base.PlayReverse();

            InitSequence();
        }

        public override void Cancel()
        {
            base.Cancel();

            Dispose();
        }

        protected virtual void HandleTweenComplete()
        {
            OnPlayFinished?.Invoke(this);
        }

        protected virtual void HandleTweenRewind()
        {
            OnPlayReverseFinished?.Invoke(this);
        }
    }
}