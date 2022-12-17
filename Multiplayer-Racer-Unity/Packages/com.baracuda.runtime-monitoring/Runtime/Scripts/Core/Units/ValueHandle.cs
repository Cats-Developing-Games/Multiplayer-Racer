// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Profiles;
using Baracuda.Monitoring.Types;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baracuda.Monitoring.Units
{
    /// <summary>
    /// Base type for units that monitor a value <br/>
    /// <see cref="FieldHandle{TTarget,TValue}"/><br/>
    /// <see cref="PropertyHandle{TTarget,TValue}"/>
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal abstract class ValueHandle<TTarget, TValue> : MonitorHandle, ISettableValue<TValue>, IGettableValue<TValue> where TTarget : class
    {
        #region Fields

        protected readonly StringDelegate ProcessValue;

        private readonly TTarget _target;
        private readonly Func<TTarget, TValue> _getValue;
        private readonly Action<TTarget, TValue> _setValue;

        private readonly Action _validationTick;
        private readonly Func<bool> _validateFunc;
        private readonly ValidationEvent _validationEvent;

        private readonly ValueProfile<TTarget, TValue>.IsDirtyDelegate _checkIsDirty;

        private TValue _lastValue = default;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Ctor

        internal ValueHandle(TTarget target,
            Func<TTarget, TValue> getValue,
            Action<TTarget, TValue> setValue,
            Func<TValue, string> valueProcessor,
            MulticastDelegate validationFunc,
            ValidationEvent validationEvent,
            ValueProfile<TTarget, TValue> profile) : base(target, profile)
        {
            _target = target;
#if DEBUG
            _getValue = (value) =>
            {
                try
                {
                    return getValue(value);
                }
                catch (Exception exception)
                {
                    Monitor.Logger.Log($"Exception when calling {nameof(GetValue)} in {this}\n(see next log for more information)", LogType.Warning, false);
                    Monitor.Logger.LogException(exception);
                    Enabled = false;
                    return default;
                }
            };

            if (setValue != null)
            {
                _setValue = (targetArg, valueArg) =>
                {
                    try
                    {
                        setValue(targetArg, valueArg);
                    }
                    catch (Exception exception)
                    {
                        Monitor.Logger.Log($"Exception when calling {nameof(SetValue)} in {this}\n(see next log for more information)", LogType.Warning, false);
                        Monitor.Logger.LogException(exception);
                        Enabled = false;
                    }
                };
            }
#else
            _getValue = getValue;
            _setValue = setValue;
#endif

            ProcessValue = CompileValueProcessor(valueProcessor);

            _checkIsDirty = profile.IsDirtyFunc;

            if (profile.CustomUpdateEventAvailable)
            {
                if (!profile.TrySubscribeToUpdateEvent(target, Refresh, SetValue))
                {
                    Debug.LogWarning($"Could not subscribe {Name} to update event!");
                }
            }

            if (validationEvent != null)
            {
                _validationEvent = validationEvent;
                _validationEvent.AddMethod(SetEnabled);
            }

            // Prefer event based validation
            else if (validationFunc != null)
            {
                switch (validationFunc)
                {
                    case Func<TTarget, bool> instanceValidator:
                        _validateFunc = () => instanceValidator(_target);
                        break;

                    case Func<bool> simpleValidator:
                        _validateFunc = simpleValidator;
                        break;

                    case Func<TValue, bool> conditionalValidator:
                        _validateFunc = () => conditionalValidator(GetValue());
                        break;
                }

                _validationTick = () => Enabled = _validateFunc();
                Monitor.Ticker.AddValidationTicker(_validationTick);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Value Processor

        private StringDelegate CompileValueProcessor(Func<TValue, string> func)
        {
            return () => func(_getValue(_target)) ?? Null;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Update

        public override void Refresh()
        {
            var current = GetValue();

            if (_checkIsDirty(ref current, ref _lastValue))
            {
                var state = GetState();
                RaiseValueChanged(state);
            }

            _lastValue = current;
        }

        #endregion

        #region Validation

        private void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Get

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string GetState()
        {
            return ProcessValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetValue()
        {
            return _getValue(_target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValueAs<T>()
        {
            return _getValue(_target).ConvertFast<TValue, T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValueAsObject()
        {
            return _getValue(_target);
        }

        #endregion

        #region Set

        public void SetValue(TValue value)
        {
            _setValue?.Invoke(_target, value);
            _lastValue = value;
            var state = GetState();
            RaiseValueChanged(state);
        }

        public void SetValue(object value)
        {
            _setValue?.Invoke(_target, (TValue) value);
            _lastValue = (TValue) value;
            var state = GetState();
            RaiseValueChanged(state);
        }

        public void SetValueStruct<TStruct>(TStruct value) where TStruct : struct
        {
            var converted = value.ConvertFast<TStruct, TValue>();
            _setValue?.Invoke(_target, converted);
            _lastValue = converted;
            var state = GetState();
            RaiseValueChanged(state);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region IDisosable

        public override void Dispose()
        {
            base.Dispose();

            ((ValueProfile<TTarget, TValue>) Profile).TryUnsubscribeFromUpdateEvent(_target, Refresh, SetValue);

            _validationEvent?.RemoveMethod(SetEnabled);

            if (_validationTick != null)
            {
                Monitor.Ticker.RemoveValidationTicker(_validationTick);
            }
        }

        #endregion
    }
}