using LanguageTrainer.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LanguageTrainer
{
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
                       
            SizeChanged += (o, e) =>
            {
                Rect r = SystemParameters.WorkArea;
                Left = r.Right - ActualWidth - 10;
                Top = r.Bottom - ActualHeight - 10;
            };

            Loaded += (o, e) =>
            {
                Rect r = SystemParameters.WorkArea;

                DoubleAnimation FadeInAnimation = new DoubleAnimation();
                FadeInAnimation.From = r.Bottom;
                FadeInAnimation.To = r.Bottom - ActualHeight - 10;
                FadeInAnimation.Duration = TimeSpan.FromMilliseconds(500);
                FadeInAnimation.EasingFunction = new BackEase() { Amplitude = 0.7 };
                BeginAnimation(TopProperty, FadeInAnimation);
            };

            // StartPage
            LoadTheme("dark");
            Navigate(new MainMenu());
            InitializeComponent();
        }

        private void LoadTheme(string style)
        {
            var uri = new Uri("source/themes/" + style + ".xaml", UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
    }
}
