﻿using System.Collections.ObjectModel;
using System.Linq;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class LayerProperty<T> : BaseLayerProperty
    {
        public LayerProperty(Layer layer, BaseLayerProperty parent, string id, string name, string description) : base(layer, parent, id, name, description, typeof(T))
        {
        }

        /// <summary>
        ///     Gets or sets the value of the property without any keyframes applied
        /// </summary>
        public T Value
        {
            get => BaseValue != null ? (T) BaseValue : default;
            set => BaseValue = value;
        }

        /// <summary>
        ///     Gets the value of the property with keyframes applied
        /// </summary>
        public T CurrentValue
        {
            get
            {
                var currentValue = GetCurrentValue();
                return currentValue == null ? default : (T) currentValue;
            }
        }

        /// <summary>
        ///     Gets a list of keyframes defining different values of the property in time, this list contains the strongly typed
        ///     <see cref="Keyframe{T}" />
        /// </summary>
        public ReadOnlyCollection<Keyframe<T>> Keyframes => BaseKeyframes.Cast<Keyframe<T>>().ToList().AsReadOnly();

        /// <summary>
        ///     Adds a keyframe to the property.
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void AddKeyframe(Keyframe<T> keyframe)
        {
            base.AddKeyframe(keyframe);
        }

        /// <summary>
        ///     Removes a keyframe from the property.
        /// </summary>
        /// <param name="keyframe">The keyframe to remove</param>
        public void RemoveKeyframe(Keyframe<T> keyframe)
        {
            base.RemoveKeyframe(keyframe);
        }
    }
}