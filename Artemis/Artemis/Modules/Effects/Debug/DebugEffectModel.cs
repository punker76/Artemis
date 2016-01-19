﻿using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Artemis.Models;
using Artemis.Utilities.Keyboard;

namespace Artemis.Modules.Effects.Debug
{
    internal class DebugEffectModel : EffectModel
    {
        public DebugEffectModel(DebugEffectSettings settings)
        {
            Name = "Debug Effect";
            Settings = settings;
            Scale = 4;

            KeyboardRectangle = new KeyboardRectangle(Scale, 0, 0, 21, 6,
                new List<Color>
                {
                    Color.Red,
                    Color.OrangeRed,
                    Color.Yellow,
                    Color.Green,
                    Color.Blue,
                    Color.Purple,
                    Color.DeepPink
                }, LinearGradientMode.Horizontal);
        }

        public int Scale { get; set; }

        public DebugEffectSettings Settings { get; set; }

        public KeyboardRectangle KeyboardRectangle { get; set; }

        public override void Dispose()
        {
        }

        public override void Enable()
        {
        }

        public override void Update()
        {
            KeyboardRectangle.Height = Settings.Height;
            KeyboardRectangle.Width = Settings.Width;
            //KeyboardRectangle.GradientMode = Settings.Type;
            KeyboardRectangle.Rotate = Settings.Rotate;
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(21*Scale, 6*Scale);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                KeyboardRectangle.Draw(g);
            }
            return bitmap;
        }
    }
}