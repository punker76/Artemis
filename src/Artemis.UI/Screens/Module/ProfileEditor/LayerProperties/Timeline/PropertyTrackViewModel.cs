﻿using System.Linq;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTrackViewModel : Screen
    {
        private readonly IPropertyTrackKeyframeViewModelFactory _propertyTrackKeyframeViewModelFactory;

        public PropertyTrackViewModel(PropertyTimelineViewModel propertyTimelineViewModel,
            LayerPropertyViewModel layerPropertyViewModel,
            IPropertyTrackKeyframeViewModelFactory propertyTrackKeyframeViewModelFactory)
        {
            _propertyTrackKeyframeViewModelFactory = propertyTrackKeyframeViewModelFactory;
            PropertyTimelineViewModel = propertyTimelineViewModel;
            LayerPropertyViewModel = layerPropertyViewModel;
            KeyframeViewModels = new BindableCollection<PropertyTrackKeyframeViewModel>();

            PopulateKeyframes();
            UpdateKeyframes(PropertyTimelineViewModel.LayerPropertiesViewModel.PixelsPerSecond);
        }

        public PropertyTimelineViewModel PropertyTimelineViewModel { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public BindableCollection<PropertyTrackKeyframeViewModel> KeyframeViewModels { get; set; }

        public void PopulateKeyframes()
        {
            // Remove old keyframes
            foreach (var viewModel in KeyframeViewModels.ToList())
            {
                if (!LayerPropertyViewModel.LayerProperty.UntypedKeyframes.Contains(viewModel.Keyframe))
                    KeyframeViewModels.Remove(viewModel);
            }

            // Add new keyframes
            foreach (var keyframe in LayerPropertyViewModel.LayerProperty.UntypedKeyframes)
            {
                if (KeyframeViewModels.Any(k => k.Keyframe == keyframe))
                    continue;
                KeyframeViewModels.Add(_propertyTrackKeyframeViewModelFactory.Create(this, keyframe));
            }

            UpdateKeyframes(PropertyTimelineViewModel.LayerPropertiesViewModel.PixelsPerSecond);
        }

        public void UpdateKeyframes(int pixelsPerSecond)
        {
            foreach (var keyframeViewModel in KeyframeViewModels)
            {
                keyframeViewModel.ParentView = View;
                keyframeViewModel.Update(pixelsPerSecond);
            }
        }

        protected override void OnViewLoaded()
        {
            foreach (var keyframeViewModel in KeyframeViewModels)
                keyframeViewModel.ParentView = View;
            base.OnViewLoaded();
        }
    }
}