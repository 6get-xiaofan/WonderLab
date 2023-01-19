﻿using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia;
using FluentAvalonia.UI.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonderLab.Modules.Media
{
    public class EntranceNavigationTransitionInfoX : NavigationTransitionInfo
    {
        /// <summary>
        /// Gets or sets the Horizontal Offset used when animating
        /// </summary>
        public double FromHorizontalOffset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the Vertical Offset used when animating
        /// </summary>
        public double FromVerticalOffset { get; set; } = 28;

        public EntranceNavigationTransitionInfoX(double fromHorizontalOffset)
        {
            FromVerticalOffset = fromHorizontalOffset;
        }



        //SlideUp and FadeIn
        public async override void RunAnimation(Animatable ctrl)
        {
            var animation = new Avalonia.Animation.Animation
            {
                Easing = new SplineEasing(0.1, 0.9, 0.2, 1.0),
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Visual.OpacityProperty, 0.0),
                            new Setter(TranslateTransform.XProperty,FromHorizontalOffset),
                            new Setter(TranslateTransform.YProperty, FromVerticalOffset)
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Visual.OpacityProperty, 1d),
                            new Setter(TranslateTransform.XProperty,0.0),
                            new Setter(TranslateTransform.YProperty, 0.0)
                        },
                        Cue = new Cue(1d)
                        }
                    },
                    Duration = TimeSpan.FromSeconds(0.67),
                    FillMode = FillMode.Forward
                };

            await animation.RunAsync(ctrl, null);

            (ctrl as IVisual).Opacity = 1;
        }
    }
}
