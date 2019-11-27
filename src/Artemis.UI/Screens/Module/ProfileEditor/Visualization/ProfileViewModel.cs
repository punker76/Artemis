﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.SurfaceEditor.Visualization;
using RGB.NET.Core;
using Stylet;
using Point = System.Windows.Point;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileViewModel : ProfileEditorPanelViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly TimerUpdateTrigger _updateTrigger;

        public ProfileViewModel(ISurfaceService surfaceService, ISettingsService settingsService)
        {
            _settingsService = settingsService;
            Devices = new ObservableCollection<ProfileDeviceViewModel>();
            Execute.PostToUIThread(() =>
            {
                SelectionRectangle = new RectangleGeometry();
                PanZoomViewModel = new PanZoomViewModel();
            });
            Cursor = null;

            ApplySurfaceConfiguration(surfaceService.ActiveSurface);

            // Borrow RGB.NET's update trigger but limit the FPS
            var targetFpsSetting = settingsService.GetSetting("Core.TargetFrameRate", 25);
            var editorTargetFpsSetting = settingsService.GetSetting("ProfileEditor.TargetFrameRate", 15);
            var targetFps = Math.Min(targetFpsSetting.Value, editorTargetFpsSetting.Value);
            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / targetFps};
            _updateTrigger.Update += UpdateLeds;

            surfaceService.ActiveSurfaceConfigurationChanged += OnActiveSurfaceConfigurationChanged;
        }

        public bool IsInitializing { get; private set; }
        public ObservableCollection<ProfileDeviceViewModel> Devices { get; set; }
        public RectangleGeometry SelectionRectangle { get; set; }
        public PanZoomViewModel PanZoomViewModel { get; set; }
        public PluginSetting<bool> HighlightSelectedLayer { get; set; }
        public PluginSetting<bool> PauseRenderingOnFocusLoss { get; set; }

        public Cursor Cursor { get; set; }

        private void OnActiveSurfaceConfigurationChanged(object sender, SurfaceConfigurationEventArgs e)
        {
            ApplySurfaceConfiguration(e.Surface);
        }

        private void ApplySurfaceConfiguration(Surface surface)
        {
            // Make sure all devices have an up-to-date VM
            foreach (var surfaceDeviceConfiguration in surface.Devices)
            {
                // Create VMs for missing devices
                var viewModel = Devices.FirstOrDefault(vm => vm.Device.RgbDevice == surfaceDeviceConfiguration.RgbDevice);
                if (viewModel == null)
                {
                    // Create outside the UI thread to avoid slowdowns as much as possible
                    var profileDeviceViewModel = new ProfileDeviceViewModel(surfaceDeviceConfiguration);
                    Execute.PostToUIThread(() =>
                    {
                        // Gotta call IsInitializing on the UI thread or its never gets picked up
                        IsInitializing = true;
                        lock (Devices)
                        {
                            Devices.Add(profileDeviceViewModel);
                        }
                    });
                }
                // Update existing devices
                else
                    viewModel.Device = surfaceDeviceConfiguration;
            }

            // Sort the devices by ZIndex
            Execute.PostToUIThread(() =>
            {
                lock (Devices)
                {
                    foreach (var device in Devices.OrderBy(d => d.ZIndex).ToList())
                        Devices.Move(Devices.IndexOf(device), device.ZIndex - 1);
                }
            });
        }

        private void UpdateLeds(object sender, CustomUpdateData customUpdateData)
        {
            lock (Devices)
            {
                if (IsInitializing)
                    IsInitializing = Devices.Any(d => !d.AddedLeds);

                foreach (var profileDeviceViewModel in Devices)
                    profileDeviceViewModel.Update();
            }
        }

        protected override void OnActivate()
        {
            HighlightSelectedLayer = _settingsService.GetSetting("ProfileEditor.HighlightSelectedLayer", true);
            PauseRenderingOnFocusLoss = _settingsService.GetSetting("ProfileEditor.PauseRenderingOnFocusLoss", true);

            _updateTrigger.Start();
            base.OnActivate();
        }
        
        protected override void OnDeactivate()
        {
            HighlightSelectedLayer.Save();
            PauseRenderingOnFocusLoss.Save();

            _updateTrigger.Stop();
            base.OnDeactivate();
        }

        #region Selection

        private MouseDragStatus _mouseDragStatus;
        private Point _mouseDragStartPoint;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (IsPanKeyDown() || e.ChangedButton == MouseButton.Right)
                return;

            var position = e.GetPosition((IInputElement) sender);
            var relative = PanZoomViewModel.GetRelativeMousePosition(sender, e);
            if (e.LeftButton == MouseButtonState.Pressed)
                StartMouseDrag(position, relative);
            else
                StopMouseDrag(position);
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void EditorGridMouseMove(object sender, MouseEventArgs e)
        {
            // If holding down Ctrl, pan instead of move/select
            if (IsPanKeyDown())
            {
                Pan(sender, e);
                return;
            }

            var position = e.GetPosition((IInputElement) sender);
            if (_mouseDragStatus == MouseDragStatus.Selecting)
                UpdateSelection(position);
        }

        private void StartMouseDrag(Point position, Point relative)
        {
            _mouseDragStatus = MouseDragStatus.Selecting;
            _mouseDragStartPoint = position;

            // Any time dragging starts, start with a new rect
            SelectionRectangle.Rect = new Rect();
        }

        private void StopMouseDrag(Point position)
        {
            var selectedRect = new Rect(_mouseDragStartPoint, position);
            foreach (var device in Devices)
            {
                foreach (var profileLedViewModel in device.Leds)
                {
                    if (PanZoomViewModel.TransformContainingRect(profileLedViewModel.Led.AbsoluteLedRectangle.ToWindowsRect(1)).IntersectsWith(selectedRect))
                        profileLedViewModel.SelectionStatus = SelectionStatus.Selected;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        profileLedViewModel.SelectionStatus = SelectionStatus.None;
                }
            }

            _mouseDragStatus = MouseDragStatus.None;
        }

        private void UpdateSelection(Point position)
        {
            if (IsPanKeyDown())
                return;

            var selectedRect = new Rect(_mouseDragStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            foreach (var device in Devices)
            {
                foreach (var profileLedViewModel in device.Leds)
                {
                    if (PanZoomViewModel.TransformContainingRect(profileLedViewModel.Led.AbsoluteLedRectangle.ToWindowsRect(1)).IntersectsWith(selectedRect))
                        profileLedViewModel.SelectionStatus = SelectionStatus.Selected;
                    else if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                        profileLedViewModel.SelectionStatus = SelectionStatus.None;
                }
            }
        }

        #endregion

        #region Panning and zooming

        public void EditorGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            PanZoomViewModel.ProcessMouseScroll(sender, e);
        }

        public void EditorGridKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && e.IsDown)
                Cursor = Cursors.ScrollAll;
        }

        public void EditorGridKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && e.IsUp)
                Cursor = null;
        }

        public void Pan(object sender, MouseEventArgs e)
        {
            PanZoomViewModel.ProcessMouseMove(sender, e);

            // Empty the selection rect since it's shown while mouse is down
            SelectionRectangle.Rect = Rect.Empty;
        }

        public void ResetZoomAndPan()
        {
            PanZoomViewModel.Reset();
        }

        private bool IsPanKeyDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }

        #endregion
    }
}