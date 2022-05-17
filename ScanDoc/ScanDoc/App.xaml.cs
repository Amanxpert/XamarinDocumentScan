using ScanDoc.Services;
using ScanDoc.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ScanDoc
{
    public partial class App : Application
    {
        public static ImageSource PassImage { get; set; }
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new ScanPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
