using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using LanguageTrainer.Source.Controls;

using LanguageTrainer.Source.Tools;

namespace LanguageTrainer.Pages
{
    public partial class Vocabulary : Page, INotifyPropertyChanged
    {
        public Vocabulary()
        {
            InitializeComponent();
        }

        public string answer;
        public string Answer
        {
            get => answer;
            set
            {
                answer = value;
                OnPropertyChanged("Answer");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
