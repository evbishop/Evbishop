using Evbishop.Runtime.ModulesSystem;
using Evbishop.Runtime.UI.Modules;
using Evbishop.Runtime.UI.Modules.Animator;
using Evbishop.Runtime.UI.Modules.Container;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Evbishop.Runtime.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UIContainer : UIBehaviour
    {
        private const string SELECTION = "Selection";
        private const string SELECTION_HORIZONTAL = "Selection/Horizontal";
        private const string ON_HIDE = "On Hide";
        private const string ON_HIDE_HORIZONTAL = "On Hide/Horizontal";

        [field: SerializeField, ReadOnly] public EVisibility Visibility { get; private set; }

        [SerializeField] private EStartBehaviour _startBehaviour;
        [SerializeField, BoxGroup(ON_HIDE), HorizontalGroup(ON_HIDE_HORIZONTAL, Width = 200), LabelText("Disable game object on hide"), LabelWidth(160)] private bool _isDisablingGameObjectOnHide;
        [SerializeField, HorizontalGroup(ON_HIDE_HORIZONTAL), LabelText("Disable canvas on hide"), LabelWidth(160)] private bool _isDisablingCanvasOnHide = true;

        [SerializeField, BoxGroup(SELECTION), HorizontalGroup(SELECTION_HORIZONTAL, Width = 200), LabelText("Clear selected on show"), LabelWidth(160)] private bool _isClearingSelectedOnShow;
        [SerializeField, HorizontalGroup(SELECTION_HORIZONTAL), LabelText("Clear selected on hide"), LabelWidth(160)] private bool _isClearingSelectedOnHide;

        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnShow;
        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnShown;
        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnHide;
        [SerializeField, FoldoutGroup(CALLBACKS)] public UnityEvent OnHidden;

        [OdinSerialize] public UIModulesSystem Modules { get; private set; } = new();

        private int _workingAnimators;

        public Canvas Canvas { get; private set; }

        public bool IsVisible => Visibility == EVisibility.Showing || Visibility == EVisibility.Shown;
        public bool HasAnimations => Modules.HasInterface<IModuleAnimator>();

        protected override void Awake()
        {
            base.Awake();

            Canvas = GetComponent<Canvas>();
            foreach (var module in Modules.GetModulesWithInterface<IModuleAwakable>())
            {
                module.HandleAwake();
            }
            foreach (var module in Modules.GetModulesWithInterface<IModuleAwakable<UIContainer>>())
            {
                module.HandleAwake(this);
            }
        }

        protected override void Start()
        {
            base.Start();

            if (Modules.TryGetModule(out ContainerModuleCustomStartPosition moduleStartPosition))
                moduleStartPosition.SetPosition(transform);
            switch (_startBehaviour)
            {
                case EStartBehaviour.InstantHide:
                    Hide(true);
                    break;
                case EStartBehaviour.InstantShow:
                    Show(true);
                    break;
                case EStartBehaviour.Hide:
                    Hide();
                    break;
                case EStartBehaviour.Show:
                    Show();
                    break;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var module in Modules.GetModulesWithInterface<IModuleDisposable>())
            {
                module.Dispose();
            }
        }

        [Button, ButtonGroup, GUIColor("cyan")]
        public void Show(bool instant = false)
        {
            if (_isClearingSelectedOnShow)
                EventSystem.current.SetSelectedGameObject(null);

            //if (!instant && (Visibility == EVisibility.Showing || Visibility == EVisibility.Shown))
            //{
            //    Hide(true);
            //}

            _workingAnimators = 0;
            Visibility = EVisibility.Showing;
            OnShow?.Invoke();

            gameObject.SetActive(true);
            if (_isDisablingCanvasOnHide)
                Canvas.enabled = true;
            if (Modules.TryGetModule(out ContainerModuleGraphicRaycaster grModule) &&
                grModule.IsDisablingGraphicRaycasterOnHide)
                grModule.Component.enabled = true;

            if (instant)
            {
                Visibility = EVisibility.Shown;
                OnShown?.Invoke();
            }
            else
            {
                if (Modules.HasInterface<IModuleAnimator>())
                {
                    foreach (var animator in Modules.GetModulesWithInterface<IModuleAnimator>())
                    {
                        _workingAnimators++;
                        animator.OnPlayFinished.AddListener(HandleAnimatorShowFinished);
                        animator.Play();
                    }
                }
                else
                {
                    Visibility = EVisibility.Shown;
                    OnShown?.Invoke();
                }
            }
        }

        [Button, ButtonGroup, GUIColor(1f, 0.7f, 0.7f)]
        public void Hide(bool instant = false)
        {
            //if (!instant && (Visibility == EVisibility.Hiding || Visibility == EVisibility.Hidden))
            //    return;

            if (_isClearingSelectedOnHide && EventSystem.current)
                EventSystem.current.SetSelectedGameObject(null);

            _workingAnimators = 0;
            Visibility = EVisibility.Hiding;
            OnHide?.Invoke();

            if (instant)
            {
                InstantHide();
                if (Modules.HasInterface<IModuleAnimator>())
                {
                    foreach (var animator in Modules.GetModulesWithInterface<IModuleAnimator>())
                        animator.ReverseInstant();
                }
            }
            else
            {
                if (Modules.HasInterface<IModuleAnimator>())
                {
                    foreach (var animator in Modules.GetModulesWithInterface<IModuleAnimator>())
                    {
                        _workingAnimators++;
                        animator.OnPlayReverseFinished.AddListener(HandleAnimatorHideFinished);
                        animator.PlayReverse();
                    }
                }
                else
                {
                    InstantHide();
                }
            }
        }

        private void InstantHide()
        {
            if (_isDisablingCanvasOnHide)
                Canvas.enabled = false;
            if (_isDisablingGameObjectOnHide)
                gameObject.SetActive(false);
            if (Modules.TryGetModule(out ContainerModuleGraphicRaycaster grModule) &&
                grModule.IsDisablingGraphicRaycasterOnHide)
                grModule.Component.enabled = false;

            Visibility = EVisibility.Hidden;
            OnHidden?.Invoke();
        }

        public void HandleAnimatorShowFinished(AnimatorModule module)
        {
            module.OnPlayFinished.RemoveListener(HandleAnimatorShowFinished);
            _workingAnimators--;
            if (_workingAnimators < 1)
            {
                Visibility = EVisibility.Shown;
                OnShown?.Invoke();
            }
        }

        public void HandleAnimatorHideFinished(AnimatorModule module)
        {
            module.OnPlayReverseFinished.RemoveListener(HandleAnimatorHideFinished);
            _workingAnimators--;
            if (_workingAnimators < 1)
            {
                InstantHide();
            }
        }
    }
}
