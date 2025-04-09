using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Evbishop.Runtime.UI
{
    public class UIStepper : UIBehaviour
    {
        private const float TOLERANCE = 0.0001f;
        private const float DRAG_DISTANCE = 20f;
        private const float WAIT_BEFORE_STARTING = 0.6f;
        private const float WAIT_TIME = 0.4f;
        private const float WAIT_TIME_MIN = 0.04f;
        private const float WAIT_TIME_REDUCTION = 0.4f;

        [SerializeField] private UIButton _minusButton;
        public UIButton MinusButton
        {
            get => _minusButton;
            private set
            {
                if (_minusButton != null)
                {
                    _minusButton.RemoveStateHandler(ESelectableState.Pressed, HandleMinusButtonClicked);
                    _minusButton.PointerDownActions.RemoveListener(HandleMinusButtonDown);
                    _minusButton.PointerUpActions.RemoveListener(HandleMinusButtonUp);
                }
                if (value != null)
                {
                    value.AddStateHandler(ESelectableState.Pressed, HandleMinusButtonClicked);
                    value.PointerDownActions.AddListener(HandleMinusButtonDown);
                    value.PointerUpActions.AddListener(HandleMinusButtonUp);
                }
                _minusButton = value;
            }
        }

        [SerializeField] private UIButton _plusButton;
        public UIButton PlusButton
        {
            get => _plusButton;
            private set
            {
                if (_plusButton != null)
                {
                    _plusButton.RemoveStateHandler(ESelectableState.Pressed, HandlePlusButtonClicked);
                    _plusButton.PointerDownActions.RemoveListener(HandlePlusButtonDown);
                    _plusButton.PointerUpActions.RemoveListener(HandlePlusButtonUp);
                }
                if (value != null)
                {
                    value.AddStateHandler(ESelectableState.Pressed, HandlePlusButtonClicked);
                    value.PointerDownActions.AddListener(HandlePlusButtonDown);
                    value.PointerUpActions.AddListener(HandlePlusButtonUp);
                }
                _plusButton = value;
            }
        }

        [SerializeField] private UIButton _resetButton;
        public UIButton ResetButton
        {
            get => _resetButton;
            private set
            {
                if (_resetButton != null)
                {
                    _resetButton.RemoveStateHandler(ESelectableState.Pressed, HandleResetButtonClicked);
                }
                if (value != null)
                {
                    value.AddStateHandler(ESelectableState.Pressed, HandleResetButtonClicked);
                }
                _resetButton = value;
            }
        }

        [SerializeField] private TMP_Text _targetLabel;
        public TMP_Text TargetLabel
        {
            get => _targetLabel;
            private set
            {
                _targetLabel = value;
                UpdateValueLabel();
            }
        }

        [SerializeField] private float _minValue;
        public float MinValue
        {
            get => _minValue;
            set
            {
                _minValue = (float)Math.Round(value, ValuePrecision);
                _maxValue = _minValue > _maxValue ? _minValue : _maxValue;

                if (_value < _minValue)
                {
                    SetValue(_minValue);
                }
            }
        }

        [SerializeField] private float _maxValue = 1f;
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = (float)Math.Round(value, ValuePrecision);
                _minValue = _maxValue < _minValue ? _maxValue : _minValue;

                if (_value > _maxValue)
                {
                    SetValue(_maxValue);
                }
            }
        }

        [SerializeField] private float _value;
        public float Value
        {
            get => _value;
            set => SetValue(value);
        }

        [SerializeField] private float _defaultValue;
        public float DefaultValue
        {
            get => _defaultValue;
            set => _defaultValue = Mathf.Clamp(value, MinValue, MaxValue);
        }

        [SerializeField] private float _step = 0.1f;
        public float Step
        {
            get => _step;
            private set
            {
                _step = value;
                SetValue(NearestStep(value));
                _isStepValueChanged = true;
            }
        }

        public bool ResetValueOnEnable = true;
        public int ValuePrecision = 2;

        /// <summary>
        /// The initial and maximum time in seconds to wait before the value starts to auto repeat (increment/decrement)
        /// </summary>
        [FoldoutGroup("Auto Repeat")] public float AutoRepeatWaitTime = WAIT_TIME;
        /// <summary>
        /// When the stepper is auto-repeating, the wait time between each increase/decrease will be reduced by multiplying the remaining wait time with this value until it reaches AutoRepeatMinWaitTime limit.
        /// This reduction makes the stepper feel more responsive and less laggy.
        /// </summary>
        [FoldoutGroup("Auto Repeat")] public float AutoRepeatWaitTimeReduction = WAIT_TIME_REDUCTION;
        /// <summary> The minimum wait time between each increase/decrease when the stepper is auto-repeating </summary>
        [FoldoutGroup("Auto Repeat")] public float AutoRepeatMinWaitTime = WAIT_TIME_MIN;

        /// <summary>
        /// Coroutine called when the user is holding down the plus button.
        /// It's used to auto-repeat the increment action.
        /// </summary>
        private Coroutine _autoIncrementCoroutine { get; set; }
        /// <summary>
        /// Coroutine called when the user is holding down the minus button.
        /// It's used to auto-repeat the decrement action.
        /// </summary>
        private Coroutine _autoDecrementCoroutine { get; set; }
        private bool _isAutoIncrementing;
        private bool _isAutoDecrementing;
        private bool _isStepValueChanged;

        /// <summary>
        /// Fired when the value changed.
        /// Returns the new value.
        /// </summary>
        [FoldoutGroup(CALLBACKS)] public UnityEvent<float> OnValueChanged = new();
        /// <summary>
        /// Fired when the value increases.
        /// Returns the difference between the new and old value.
        /// <para/> Example: if the previous value was 0.5 and the new value is 0.7, the returned value will be 0.2
        /// </summary>
        [FoldoutGroup(CALLBACKS)] public UnityEvent<float> OnValueIncremented = new();
        /// <summary>
        /// Fired when the value decreases.
        /// Returns the difference between the new and old value.
        /// <para/> Example: if the previous value was 10 and the new value is 5, the returned value will be -5
        /// </summary>
        [FoldoutGroup(CALLBACKS)] public UnityEvent<float> OnValueDecremented = new();
        /// <summary> Fired when the value was reset </summary>
        [FoldoutGroup(CALLBACKS)] public UnityEvent OnValueReset = new();
        /// <summary> Fired when the value has reached the minimum value </summary>
        [FoldoutGroup(CALLBACKS)] public UnityEvent OnValueReachedMin = new();
        /// <summary> Fired when the value has reached the maximum value </summary>
        [FoldoutGroup(CALLBACKS)] public UnityEvent OnValueReachedMax = new();

        protected virtual void OnValidate()
        {
            SetValue(Value);
        }

        protected override void Start()
        {
            base.Start();

            UpdateValueLabel();
        }

        protected virtual void OnEnable()
        {
            if (!Application.isPlaying)
                return;

            if (MinusButton != null)
            {
                MinusButton.AddStateHandler(ESelectableState.Pressed, HandleMinusButtonClicked);
                MinusButton.PointerDownActions.AddListener(HandleMinusButtonDown);
                MinusButton.PointerUpActions.AddListener(HandleMinusButtonUp);
            }
            if (PlusButton != null)
            {
                PlusButton.AddStateHandler(ESelectableState.Pressed, HandlePlusButtonClicked);
                PlusButton.PointerDownActions.AddListener(HandlePlusButtonDown);
                PlusButton.PointerUpActions.AddListener(HandlePlusButtonUp);
            }
            if (ResetButton != null)
            {
                ResetButton.AddStateHandler(ESelectableState.Pressed, HandleResetButtonClicked);
            }
            if (ResetValueOnEnable)
            {
                ResetValue();
            }
        }

        protected virtual void OnDisable()
        {
            if (!Application.isPlaying)
                return;

            if (PlusButton != null)
            {
                PlusButton.RemoveStateHandler(ESelectableState.Pressed, HandlePlusButtonClicked);
            }
            if (MinusButton != null)
            {
                MinusButton.RemoveStateHandler(ESelectableState.Pressed, HandleMinusButtonClicked);
            }
            if (ResetButton != null)
            {
                ResetButton.RemoveStateHandler(ESelectableState.Pressed, HandleResetButtonClicked);
            }
            _isAutoIncrementing = false;
            if (_autoIncrementCoroutine != null)
            {
                StopCoroutine(_autoIncrementCoroutine);
            }
            _isAutoDecrementing = false;
            if (_autoDecrementCoroutine != null)
            {
                StopCoroutine(_autoDecrementCoroutine);
            }
        }

        private void LateUpdate()
        {
            if (_isAutoIncrementing) StopAutoIncrement();
            if (_isAutoDecrementing) StopAutoDecrement();
        }

        protected void HandleMinusButtonClicked()
        {
            DecrementValue();
        }

        protected void HandlePlusButtonClicked()
        {
            IncrementValue();
        }

        protected void HandlePlusButtonDown()
        {
            StopAutoIncrement();
            StartAutoIncrement();
        }

        protected void HandlePlusButtonUp()
        {
            StopAutoIncrement();
        }

        protected void HandleMinusButtonDown()
        {
            StopAutoDecrement();
            StartAutoDecrement();
        }

        protected void HandleMinusButtonUp()
        {
            StopAutoDecrement();
        }

        protected void HandleResetButtonClicked()
        {
            ResetValue();
        }

        public void ResetValue()
        {
            SetValue(DefaultValue);
            OnValueReset?.Invoke();
        }

        public void SetValue(float newValue)
        {
            bool valueChanged = Math.Abs(_value - newValue) > TOLERANCE;
            _value = (float)Math.Round(Mathf.Clamp(newValue, MinValue, MaxValue), ValuePrecision);
            if (_isStepValueChanged)
            {
                _value = NearestStep(_value);
                _isStepValueChanged = false;
            }
            UpdateValueLabel();
            if (valueChanged)
            {
                OnValueChanged.Invoke(_value);
            }
            if (_value <= MinValue)
            {
                OnValueReachedMin?.Invoke();
                if (MinusButton != null && MinusButton.IsInteractable)
                {
                    MinusButton.IsInteractable = false;
                }
            }
            else
            {
                if (MinusButton != null && !MinusButton.IsInteractable)
                {
                    MinusButton.IsInteractable = true;
                }
            }
            if (_value >= MaxValue)
            {
                OnValueReachedMax?.Invoke();
                if (PlusButton != null && PlusButton.IsInteractable)
                {
                    PlusButton.IsInteractable = false;
                }
            }
            else
            {
                if (PlusButton != null && !PlusButton.IsInteractable)
                {
                    PlusButton.IsInteractable = true;
                }
            }
        }

        /// <summary> Increment the value by the step value </summary>
        public void IncrementValue()
        {
            IncrementValue(Step);
        }

        public void IncrementValue(float increment)
        {
            bool currentValueIsMax = Math.Abs(Value - MaxValue) < TOLERANCE;
            if (!currentValueIsMax)
            {
                OnValueIncremented?.Invoke(increment);
            }
            SetValue(Value + increment);
        }

        public void DecrementValue()
        {
            DecrementValue(Step);
        }

        public void DecrementValue(float decrement)
        {
            bool currentValueIsMin = Math.Abs(Value - MinValue) < TOLERANCE;
            if (!currentValueIsMin)
            {
                OnValueDecremented?.Invoke(-decrement);
            }
            SetValue(Value - decrement);
        }

        public void UpdateValueLabel()
        {
            if (TargetLabel == null) return;
            TargetLabel.text = Value.ToString(CultureInfo.InvariantCulture);
        }

        #region Increment

        private bool CanIncrementValue() =>
            Value < MaxValue;

        private void StartAutoIncrement()
        {
            StopAutoIncrement();
            _autoIncrementCoroutine = StartCoroutine(AutoIncrementValue());
        }

        private void StopAutoIncrement()
        {
            _isAutoIncrementing = false;
            if (_autoIncrementCoroutine == null) return;
            StopCoroutine(_autoIncrementCoroutine);
        }

        private IEnumerator AutoIncrementValue()
        {
            _isAutoIncrementing = true;
            yield return new WaitForSecondsRealtime(WAIT_BEFORE_STARTING);
            float waitTime = AutoRepeatWaitTime;
            while (CanIncrementValue())
            {
                IncrementValue();
                yield return new WaitForSecondsRealtime(waitTime);
                waitTime = Mathf.Clamp(waitTime * AutoRepeatWaitTimeReduction, AutoRepeatMinWaitTime, AutoRepeatWaitTime);
            }
            _isAutoIncrementing = false;
            _autoIncrementCoroutine = null;
        }

        #endregion

        #region Decrement

        private bool CanDecrementValue() =>
            Value > MinValue;

        private void StartAutoDecrement()
        {
            StopAutoDecrement();
            _autoDecrementCoroutine = StartCoroutine(AutoDecrementValue());
        }

        private void StopAutoDecrement()
        {
            _isAutoDecrementing = false;
            if (_autoDecrementCoroutine == null) return;
            StopCoroutine(_autoDecrementCoroutine);
        }

        private IEnumerator AutoDecrementValue()
        {
            _isAutoDecrementing = true;
            yield return new WaitForSecondsRealtime(WAIT_BEFORE_STARTING);
            float waitTime = AutoRepeatWaitTime;
            while (CanDecrementValue())
            {
                DecrementValue();
                yield return new WaitForSecondsRealtime(waitTime);
                waitTime = Mathf.Clamp(waitTime * AutoRepeatWaitTimeReduction, AutoRepeatMinWaitTime, AutoRepeatWaitTime);
            }
            _isAutoDecrementing = false;
            _autoDecrementCoroutine = null;
        }

        #endregion

        /// <summary> Get the nearest value to the given value that is a multiple of the step size. </summary>
        /// <param name="uncorrectedValue"> Value that has not been corrected. </param>
        private float NearestStep(float uncorrectedValue)
        {
            if (_step == 0) return uncorrectedValue;
            return (int)Math.Round(uncorrectedValue / (double)Step, MidpointRounding.AwayFromZero) * Step;
        }
    }
}