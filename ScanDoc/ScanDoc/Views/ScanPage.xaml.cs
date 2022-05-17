using Imaging.Library;
using Imaging.Library.Entities;
using Imaging.Library.Enums;
using Imaging.Library.Filters.BasicFilters;
using Imaging.Library.Filters.ComplexFilters;
using Imaging.Library.Maths;
using Plugin.Media;
using ScanDoc.ViewModels;
using ScanDocument.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Point = Imaging.Library.Entities.Point;

namespace ScanDoc.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanPage : ContentPage
    {
        public ScanPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();
        }
        public PixelMap Source { get; set; }
        
        private void CaptureImage(object sender, EventArgs e)
        {
            //GetPixelMapFromGallery();
            xctCameraView.Shutter();
        }
        private void RecordVideo(object sender, EventArgs e)
        {
            xctCameraView.Shutter();
        }
        private void StopVideo(object sender, EventArgs e)
        {
            xctCameraView.Shutter();
        }

        private void MediaCaptured(object sender, MediaCapturedEventArgs e)
        {
            //Get Pixel Map from Caputered Image 

            Source = AndroidHelper.GetPixelMap(new MemoryStream(e.ImageData));// e.ImageData .Image file.GetStream());

            // Scanning Document
            var imaging = new ImagingManager(Source);

            var scale = 0.4;

            imaging.AddFilter(new BicubicFilter(scale)); //Downscaling
            imaging.Render();

            imaging.AddFilter(new CannyEdgeDetector());
            imaging.Render();

            var blobCounter = new BlobCounter
            {
                ObjectsOrder = ObjectsOrder.Size
            };
            imaging.AddFilter(blobCounter);

            imaging.Render();

            List<Point> corners = null;
            var blobs = blobCounter.GetObjectsInformation();
            foreach (var blob in blobs)
            {
                var points = blobCounter.GetBlobsEdgePoints(blob);

                var shapeChecker = new SimpleShapeChecker();

                if (shapeChecker.IsQuadrilateral(points, out corners))
                    break;
            }
            var edgePoints = new EdgePoints();
            edgePoints.SetPoints(corners.ToArray());

            imaging.Render();
            imaging.UndoAll();

            edgePoints = edgePoints.ZoomIn(scale);
            imaging.AddFilter(new QuadrilateralTransformation(edgePoints, true));
            imaging.Render();


            App.PassImage = ImageSource.FromStream(() => AndroidHelper.LoadImageFromPixelMap(imaging.Output));
            NavigationPage page = new NavigationPage(new ScanFilter());
            Navigation.PushModalAsync(page);
        }

        private void CloseImageView(object sender, EventArgs e)
        {
            imgViewPanel.IsVisible = false;
        }

        private async void GetPixelMapFromGallery()
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return;
            }
            var file = await CrossMedia.Current.PickPhotoAsync();

            if (file == null)
                return;


            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    Source = AndroidHelper.GetPixelMap(file.GetStream());
                    break;

                    //case Device.iOS:
                    //    Source = iOSHelper.GetPixelMap(file.Path);
                    //    break;
            }

            // Scanning Document
            var imaging = new ImagingManager(Source);

            var scale = 0.4;

            imaging.AddFilter(new BicubicFilter(scale)); //Downscaling
            imaging.Render();

            imaging.AddFilter(new CannyEdgeDetector());
            imaging.Render();

            var blobCounter = new BlobCounter
            {
                ObjectsOrder = ObjectsOrder.Size
            };
            imaging.AddFilter(blobCounter);

            imaging.Render();

            List<Point> corners = null;
            var blobs = blobCounter.GetObjectsInformation();
            foreach (var blob in blobs)
            {
                var points = blobCounter.GetBlobsEdgePoints(blob);

                var shapeChecker = new SimpleShapeChecker();

                if (shapeChecker.IsQuadrilateral(points, out corners))
                    break;
            }

            var edgePoints = new EdgePoints();
            edgePoints.SetPoints(corners.ToArray());

            imaging.Render();
            imaging.UndoAll();

            edgePoints = edgePoints.ZoomIn(scale);
            imaging.AddFilter(new QuadrilateralTransformation(edgePoints, true));

            imaging.Render();

            //imgView.Source = ImageSource.FromStream(() => AndroidHelper.LoadImageFromPixelMap(imaging.Output)); //LoadFromPixel(imaging.Output);
            //imgViewPanel.IsVisible = true;
            //Settings.Filepath = file.Path;

            App.PassImage = ImageSource.FromStream(() => AndroidHelper.LoadImageFromPixelMap(imaging.Output));
            NavigationPage page = new NavigationPage(new ScanFilter());
            await Navigation.PushModalAsync(page);
        }

        private ImageSource LoadFromPixel(PixelMap pixelMap)
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    return ImageSource.FromStream(() => AndroidHelper.LoadImageFromPixelMap(pixelMap));

                //case Device.iOS:
                //    return iOSHelper.LoadImageFromPixelMap(pixelMap);

                default:
                    return null;
            }
        }

        private void uploadBtn_Clicked(object sender, EventArgs e)
        {
            GetPixelMapFromGallery();
        }
    }
}