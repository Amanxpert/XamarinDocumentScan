using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ScanDoc.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanFilter : ContentPage
    {
        public ScanFilter()
        {
            InitializeComponent();
            ImageShow.Source = App.PassImage;
        }
    }
}