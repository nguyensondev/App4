using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App4
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            PickPhotoAction();
        }
        private async void PickPhotoAction()
        {
            var result = await CrossMultiMediaChooserPicker.Current.PickMultiImage(true, 10);
            if (result == null) return;
            if (result.Any()) { }
                //ImageSources = new ObservableCollection<ImageSource>(result.Select(c => c.Source).ToList());
        }
    }
}
