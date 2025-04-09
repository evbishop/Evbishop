using TheraBytes.BetterUi;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Evbishop.Runtime.UI
{
    [RequireComponent(typeof(BetterToggle))]
    public class UIToggle : UIBehaviour, IPointerUpHandler
    {
        [SerializeField] private bool _deselectAfterPress;

        private BetterToggle _toggle;

        protected override void Awake()
        {
            base.Awake();

            _toggle = GetComponent<BetterToggle>();
            _toggle.onValueChanged.AddListener((value) =>
            {
                if (_deselectAfterPress)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            });
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_deselectAfterPress && !eventData.hovered.Contains(gameObject))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}