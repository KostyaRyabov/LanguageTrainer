using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

using LanguageTrainer.Source.Tools;


/* 
 * ################################################################################################
 * 
 * Source  : https://gist.github.com/badamczewski/06d9c86e6d78fc79905f943cb3545f51
 * 
 *              Some parts of the code have been removed or rewritten
 *           
 * ################################################################################################
 */


namespace LanguageTrainer.Source.Controls
{
    public class MorphTextBox : Canvas
    {
        public Path Path { get; set; }

        private readonly Dictionary<((string, string, double), (string, string, double), double), List<PathGeometry>> cache;
        private TextAnimation textAnim;


        public MorphTextBox()
        {
            cache = new Dictionary<((string, string, double), (string, string, double), double), List<PathGeometry>>();

            this.Path = new Path() {
                Data                = new PathGeometry(),
                Fill                = Foreground,
                Stroke              = StrokeColor,
                StrokeThickness     = 0,
                StrokeDashArray     = null,
                StrokeDashOffset    = 0
            };

            this.Children.Add(this.Path);

            BindingOperations.SetBinding(
                target: this,
                dp: MorphTextBox.WidthProperty,
                binding: new Binding()
                {
                    Source = this.Path,
                    Path = new PropertyPath("ActualWidth")
                }
            );

            BindingOperations.SetBinding(
                target: this,
                dp: MorphTextBox.HeightProperty,
                binding: new Binding()
                {
                    Source = this.Path,
                    Path = new PropertyPath("ActualHeight")
                }
            );
        }

        public void Show(double delay = 250)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                this.BeginAnimation(
                    Canvas.OpacityProperty,
                    new DoubleAnimation() { To = 1, Duration = TimeSpan.FromMilliseconds(delay) });
            });
        }
        public void Hide(double delay = 250)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                this.BeginAnimation(
                    Canvas.OpacityProperty,
                    new DoubleAnimation() { To = 0, Duration = TimeSpan.FromMilliseconds(delay) });
            });
        }
        public void Update()
        {
            if (PreviousState == CurrentState) return;

            var textAnimationNotFound = true;
            var transition = (PreviousState, CurrentState, AnimationSpeed);

            if (!IsCached || (IsCached && (textAnimationNotFound = !cache.ContainsKey(transition))))
            {
                textAnim = new TextAnimation(
                    from    : (PathGeometry)Path.Data,
                    to      : Text.ToPathGeometry(FontName, FontSize),
                    speed   : AnimationSpeed,
                    isCached: IsCached
                );

                if (IsCached)
                {
                    cache[transition] = textAnim.Cache;
                }
            }

            if (!textAnimationNotFound)
            {
                textAnim = new TextAnimation(cache[transition]);
            }

            CompositionTarget.Rendering -= RenderMorph;
            CompositionTarget.Rendering += RenderMorph;
        }

        private (string, string, double) CurrentState   => (Text, FontName, FontSize);
        private (string, string, double) PreviousState  => (previousText, previousFontName, previousFontSize);


        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty FontNameProperty;
        public static readonly DependencyProperty FontSizeProperty;
        public static readonly DependencyProperty ForegroundProperty;
        public static readonly DependencyProperty IsCachedProperty;
        public static readonly DependencyProperty IsEditableProperty;
        public static readonly DependencyProperty StrokeColorProperty;
        public static readonly DependencyProperty StrokeThicknessProperty;
        public static readonly DependencyProperty AnimationSpeedProperty;


        private PathGeometry targetPath;

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public string FontName
        {
            get => (string)GetValue(FontNameProperty);
            set => SetValue(FontNameProperty, value);
        }
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }
        public bool IsCached
        {
            get => (bool)GetValue(IsCachedProperty);
            set => SetValue(IsCachedProperty, value);
        }
        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }
        public Brush StrokeColor
        {
            get => (Brush)GetValue(StrokeColorProperty);
            set => SetValue(StrokeColorProperty, value);
        }
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        public double AnimationSpeed
        {
            get => (double)GetValue(AnimationSpeedProperty);
            set => SetValue(AnimationSpeedProperty, value);
        }

        static MorphTextBox()
        {
            TextProperty = DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(MorphTextBox),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnTextChanged)));

            FontNameProperty = DependencyProperty.Register(
                "FontName",
                typeof(string),
                typeof(MorphTextBox),
                new FrameworkPropertyMetadata(
                    "Nexa Bold",
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnFontNameChanged)));

            FontSizeProperty = DependencyProperty.Register(
                "FontSize",
                typeof(double),
                typeof(MorphTextBox),
                new FrameworkPropertyMetadata(
                    44.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnFontSizeChanged)));

            ForegroundProperty = DependencyProperty.Register(
                "Foreground",
                typeof(Brush),
                typeof(MorphTextBox),
                new FrameworkPropertyMetadata(
                    Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender));

            IsCachedProperty = DependencyProperty.Register(
                "IsCached",
                typeof(bool),
                typeof(MorphTextBox),
                new FrameworkPropertyMetadata(
                    false,
                    new PropertyChangedCallback(OnIsCachedChanged)));

            StrokeColorProperty = DependencyProperty.Register(
                "StrokeColor",
                typeof(Brush),
                typeof(MorphTextBox),
                new FrameworkPropertyMetadata(
                    Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender));

            StrokeThicknessProperty = DependencyProperty.Register(
                "StrokeThickness",
                typeof(double),
                typeof(MorphTextBox),
                new FrameworkPropertyMetadata(
                    1.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnStrokeThicknessChanged)));

            AnimationSpeedProperty = DependencyProperty.Register(
                "AnimationSpeed",
                typeof(double),
                typeof(MorphTextBox),
                new FrameworkPropertyMetadata(
                    0.05,
                    new PropertyChangedCallback(OnAnimationSpeedChanged)));
        }

        private string previousText;
        private string previousFontName;
        private double previousFontSize;

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (MorphTextBox)d;
            sender.previousText = (string)e.OldValue;

            if ((string)e.NewValue == "") sender.Hide();
            else
            {
                if (sender.previousText == "") sender.Show();
                sender.Update();
            }
        }
        private static void OnFontNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (MorphTextBox)d;
            sender.previousFontName = (string)e.OldValue;
            
            sender.Update();
        }
        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (MorphTextBox)d;
            sender.previousFontSize = (double)e.OldValue;

            sender.Update();
        }
        private static void OnIsCachedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (MorphTextBox)d;
            sender.IsCached = (bool)e.NewValue;
        }
        private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (MorphTextBox)d;

            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                To          = (double)e.NewValue,
                Duration    = TimeSpan.FromMilliseconds(300)
            };

            sender.Path.BeginAnimation(Path.StrokeThicknessProperty, doubleAnimation);
        }
        private static void OnAnimationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MorphTextBox sender = (MorphTextBox)d;

            if (sender.IsCached)
            {
                sender.cache.Clear();
            }
        }

        private void RenderMorph(object target, EventArgs e)
        {
            if (textAnim.Move())
            {
                Path.Data = textAnim.GetFrame();
            }
            else
            {
                CompositionTarget.Rendering -= RenderMorph;
            }
        }
    }
}
