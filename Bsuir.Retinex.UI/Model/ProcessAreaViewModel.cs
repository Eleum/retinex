using Bsuir.Retinex.UI.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Bsuir.Retinex.UI.Model
{
    public class ProcessAreaViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _imagePath;
        private ICommand _addImageCommand;

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                OnPropertyChanged(() => ImagePath);
            }
        }

        public List<string> ProcessTypes { get; set; }

        public string SelectedProcessType { get; set; }

        public ICommand AddImageCommand => _addImageCommand ?? (_addImageCommand = new RelayCommand(AddImage));

        public ProcessAreaViewModel()
        {
            ProcessTypes = new List<string> { "SSR", "MSR", "MSRCR" };
            SelectedProcessType = ProcessTypes.First();
        }

        private void AddImage(object obj)
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Images|*.jpg;*.png;*.gif";

            if (dlg.ShowDialog() != true)
                return;
            try
            {
                ImagePath = dlg.FileName;
            }
            catch (NotSupportedException)
            {
                Debug.WriteLine("not supported conversion");
            }
        }

        public void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.Name));
        }
    }
}
