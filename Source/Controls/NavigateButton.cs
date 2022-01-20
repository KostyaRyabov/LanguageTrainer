using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;

using LanguageTrainer.Source.Tools;


namespace LanguageTrainer.Source.Controls
{
    public class NavigateButton : Button
    {
        public Uri NavigateUri { get; set; }
        private NavigationService navigationService;

        public NavigateButton()
        {
            this.Command = new RelayCommand(
                _ => navigationService.Navigate(NavigateUri),
                _ => (navigationService ??= NavigationService.GetNavigationService(this)) != null
            );
        }
    }
}
