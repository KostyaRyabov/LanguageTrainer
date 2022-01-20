using LanguageTrainer.Source.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace LanguageTrainer.Source.Controls
{
    public partial class Timer : UserControl, INotifyPropertyChanged
    {
        private readonly DispatcherTimer timer;

        private DoubleAnimationUsingKeyFrames waveScaleAnimation;
        private DoubleAnimation explosionAnimation, idleAnimation;

        private SolidColorBrush mixedBrush;
        private ColorAnimation mixedBrushAnimation;

        private int counter;

        public Action<Timer> OnFinish;
        public bool IsFinished
        {
            get => currentStatus == Statuses.Finsihed;
            set => throw new Exception("An attempt ot modify Read-Only property");
        }


        public static readonly DependencyProperty InitTextProperty;
        public static readonly DependencyProperty EndTextProperty;

        public static readonly DependencyProperty SecondsProperty;
        public static readonly DependencyProperty AlarmColorProperty;
        public static readonly DependencyProperty StrokeProperty;
        public static readonly DependencyProperty StrokeThicknessProperty;
        public static readonly DependencyProperty ButtonTextProperty;
        public static readonly DependencyProperty ThresholdProperty;
        public static new readonly DependencyProperty ForegroundProperty;
        public static new readonly DependencyProperty BackgroundProperty;

        public string InitText
        {
            get => (string)GetValue(InitTextProperty);
            set => SetValue(InitTextProperty, value);
        }
        public string EndText
        {
            get => (string)GetValue(EndTextProperty);
            set => SetValue(EndTextProperty, value);
        }

        public int Seconds
        {
            get => (int)GetValue(SecondsProperty);
            set => SetValue(SecondsProperty, value);
        }
        public int Threshold
        {
            get => (int)GetValue(ThresholdProperty);
            set => SetValue(ThresholdProperty, value);
        }
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }
        public SolidColorBrush AlarmColor
        {
            get => (SolidColorBrush)GetValue(AlarmColorProperty);
            set => SetValue(AlarmColorProperty, value);
        }
        public new SolidColorBrush Foreground
        {
            get => (SolidColorBrush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }
        public new SolidColorBrush Background
        {
            get => (SolidColorBrush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        private enum Statuses
        {
            Idle = 0,
            Run,
            Finsihed
        }
        private Statuses currentStatus = Statuses.Idle;
        private Statuses CurrentStatus {
            get => currentStatus;
            set
            {
                currentStatus = value;
                OnPropertyChanged("IsFinished");
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            counter = Seconds;
            TextArea.Text = InitText;
            TextArea.Foreground = Foreground;
        }

        static Timer()
        {
            InitTextProperty = DependencyProperty.Register(
                "InitText",
                typeof(string),
                typeof(Timer),
                new FrameworkPropertyMetadata(
                    "START",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                )
            );

            EndTextProperty = DependencyProperty.Register(
                "EndText",
                typeof(string),
                typeof(Timer),
                new FrameworkPropertyMetadata(
                    "FINISH",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                )
            );

            ForegroundProperty = DependencyProperty.Register(
                "Foreground",
                typeof(SolidColorBrush),
                typeof(Timer),
                new FrameworkPropertyMetadata(
                    Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender
                )
            );

            BackgroundProperty = DependencyProperty.Register(
                "Background",
                typeof(SolidColorBrush),
                typeof(Timer),
                new FrameworkPropertyMetadata(
                    Brushes.Transparent,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnMixedColorChanged)
                )
            );

            AlarmColorProperty = DependencyProperty.Register(
                "AlarmColor",
                typeof(SolidColorBrush),
                typeof(Timer),
                new FrameworkPropertyMetadata(
                    Brushes.Red,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnMixedColorChanged)
                )
            );

            SecondsProperty = DependencyProperty.Register(
                "Seconds",
                typeof(int),
                typeof(Timer),
                new FrameworkPropertyMetadata(20)
            );

            ThresholdProperty = DependencyProperty.Register(
                "Threshold",
                typeof(int),
                typeof(Timer),
                new FrameworkPropertyMetadata(10)
            );

            StrokeThicknessProperty = DependencyProperty.Register(
                "StrokeThickness",
                typeof(double),
                typeof(Timer),
                new FrameworkPropertyMetadata(
                    2.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                )
            );

            StrokeProperty = DependencyProperty.Register(
                "Stroke",
                typeof(Brush),
                typeof(Timer),
                new FrameworkPropertyMetadata(
                    Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender
                )
            );
        }
        public Timer()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += OnLoad;

            mainWave.Command = new RelayCommand(
                _ => Reload(),
                _ => CurrentStatus != Statuses.Run
            );

            mainWave.RenderTransform = new ScaleTransform();
            minorWave.RenderTransform = new ScaleTransform();
            
            minorWave.RenderTransformOrigin = mainWave.RenderTransformOrigin = new Point(0.5, 0.5);
            minorWave.Fill = mainWave.Background = mixedBrush = Background.Clone();

            idleAnimation = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase()
            };
            waveScaleAnimation = new DoubleAnimationUsingKeyFrames()
            {
                KeyFrames = new()
                {
                    new EasingDoubleKeyFrame(1, TimeSpan.FromMilliseconds(500), new ElasticEase()),
                    new EasingDoubleKeyFrame(1, TimeSpan.FromMilliseconds(800), new PowerEase()),
                }
            };
            explosionAnimation = new DoubleAnimation()
            {
                To = 20,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase()
            };
            mixedBrushAnimation = new ColorAnimation(Background.Color, TimeSpan.FromMilliseconds(250));

            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += delegate(object sender, EventArgs e)
            {
                counter -= 1;
                TextArea.Text = counter.ToString();
                
                if (counter > 0)
                {
                    if (counter <= Threshold)
                    {
                        var progress = 1 - (counter / (float)Threshold);
                        waveScaleAnimation.KeyFrames[0].Value = 1 + progress / 2;

                        UpdateWavesColor(progress);
                        RunWaveAnimation(waveScaleAnimation, minorWave);
                        RunWaveAnimation(waveScaleAnimation, mainWave, 200);
                    }
                }
                else
                {
                    timer.Stop();
                    UpdateWavesColor(1);
                    RunWaveAnimation(explosionAnimation, minorWave);

                    TextArea.Text = EndText;

                    CurrentStatus = Statuses.Finsihed;
                    OnFinish?.Invoke(this);

                    CommandManager.InvalidateRequerySuggested();
                }
            };
        }

        private static void OnMixedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (Timer)d;
            sender.UpdateWavesColor();
        }
        private Color MixColors(Color color1, Color color2, double scale)
        {
            return new Color()
            {
                R = (byte)(color1.R + ((color2.R - color1.R) * scale)),
                G = (byte)(color1.G + ((color2.G - color1.G) * scale)),
                B = (byte)(color1.B + ((color2.B - color1.B) * scale)),
                A = (byte)(color1.A + ((color2.A - color1.A) * scale))
            };
        }

        private async void RunWaveAnimation(AnimationTimeline animation, FrameworkElement wave, int delay = 0)
        {
            await Task.Delay(delay);
            
            wave.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            wave.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }
        private void UpdateWavesColor(int delay = 0)
        {
            UpdateWavesColor(1 - (counter / (float)Threshold), delay);
        }
        private async void UpdateWavesColor(double scale, int delay = 0)
        {
            await Task.Delay(delay);

            Dispatcher.Invoke(() =>
            {
                mixedBrushAnimation.To = MixColors(Background.Color, AlarmColor.Color, scale);
                mixedBrush.BeginAnimation(SolidColorBrush.ColorProperty, mixedBrushAnimation);
            });
        }

        private void InitAnimation()
        {
            counter = Seconds;
            TextArea.Text = counter.ToString();
            
            RunWaveAnimation(idleAnimation, minorWave);
            RunWaveAnimation(idleAnimation, mainWave, 200);
            UpdateWavesColor(0, 400);
        }
        public void Reload()
        {
            CurrentStatus = Statuses.Run;

            InitAnimation();

            if (timer.IsEnabled)
            {
                timer.Stop();
            }

            timer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
