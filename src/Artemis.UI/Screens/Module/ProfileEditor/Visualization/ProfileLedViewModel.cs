﻿using System;
using System.Windows;
using System.Windows.Media;
using Artemis.Core.Models.Surface;
using Artemis.UI.Extensions;
using RGB.NET.Core;
using Stylet;
using Color = System.Windows.Media.Color;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileLedViewModel : PropertyChangedBase
    {
        public ProfileLedViewModel(ArtemisLed led)
        {
            Led = led;

            // Don't want ActualLocation here since rotation is done in XAML
            X = led.RgbLed.Location.X * led.RgbLed.Device.Scale.Horizontal;
            Y = led.RgbLed.Location.Y * led.RgbLed.Device.Scale.Vertical;
            Width = led.RgbLed.ActualSize.Width;
            Height = led.RgbLed.ActualSize.Height;

            Execute.PostToUIThread(CreateLedGeometry);
        }

        public ArtemisLed Led { get; }

        public bool IsSelected { get; set; }
        public bool IsDimmed { get; set; }

        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public Geometry DisplayGeometry { get; private set; }
        public Geometry StrokeGeometry { get; private set; }
        public Color DisplayColor { get; private set; }

        private void CreateLedGeometry()
        {
            switch (Led.RgbLed.Shape)
            {
                case Shape.Custom:
                    if (Led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || Led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
                        CreateCustomGeometry(2.0);
                    else
                        CreateCustomGeometry(1.0);
                    break;
                case Shape.Rectangle:
                    if (Led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || Led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
                        CreateKeyCapGeometry();
                    else
                        CreateRectangleGeometry();
                    break;
                case Shape.Circle:
                    CreateCircleGeometry();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Stroke geometry is the display geometry excluding the inner geometry
            StrokeGeometry = DisplayGeometry.GetWidenedPathGeometry(new Pen(null, 1.0), 0.1, ToleranceType.Absolute);
            DisplayGeometry.Freeze();
            StrokeGeometry.Freeze();
        }

        private void CreateRectangleGeometry()
        {
            DisplayGeometry = new RectangleGeometry(new Rect(0.5, 0.5, Width - 1, Height - 1));
        }

        private void CreateCircleGeometry()
        {
            DisplayGeometry = new EllipseGeometry(new Rect(0.5, 0.5, Width - 1, Height - 1));
        }

        private void CreateKeyCapGeometry()
        {
            DisplayGeometry = new RectangleGeometry(new Rect(1, 1, Width - 2, Height - 2), 1.6, 1.6);
        }

        private void CreateCustomGeometry(double deflateAmount)
        {
            try
            {
                DisplayGeometry = Geometry.Combine(
                    Geometry.Empty,
                    Geometry.Parse(Led.RgbLed.ShapeData),
                    GeometryCombineMode.Union,
                    new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                            new ScaleTransform(Width - deflateAmount, Height - deflateAmount),
                            new TranslateTransform(deflateAmount / 2, deflateAmount / 2)
                        }
                    }
                );
            }
            catch (Exception)
            {
                CreateRectangleGeometry();
            }
        }

        public void Update()
        {
            var newColor = Led.RgbLed.Color.ToMediaColor();
            Execute.PostToUIThread(() =>
            {
                if (!DisplayColor.Equals(newColor))
                    DisplayColor = newColor;
            });
        }
    }
}