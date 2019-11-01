using Bsuir.Retinex.UI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bsuir.Retinex.UI.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ProcessAreaViewModel> Areas { get; set; }
         
        public MainWindowViewModel()
        {
            var areas = new List<ProcessAreaViewModel>
            {
                new ProcessAreaViewModel(),
                new ProcessAreaViewModel(),
                new ProcessAreaViewModel(),
                new ProcessAreaViewModel(),
                new ProcessAreaViewModel(),
                new ProcessAreaViewModel()
            };

            Areas = new ObservableCollection<ProcessAreaViewModel>(areas);
        }

        public void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.Name));
        }
    }
}
