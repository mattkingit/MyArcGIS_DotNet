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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Layers;

namespace MyGeocodeApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void FindAddressButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");
            var token = String.Empty;
            var locator = new OnlineLocatorTask(uri, token);

            var findParams = new OnlineLocatorFindParameters(AddressTextBox.Text);
            findParams.OutSpatialReference = MyMapView.SpatialReference;
            findParams.SourceCountry = "US";

            var results = await locator.FindAsync(findParams, new System.Threading.CancellationToken());

            if(results.Count > 0)
            {
                var firstMatch = results[0].Feature;
                var matchLocation = firstMatch.Geometry as MapPoint;

                var matchSym = new PictureMarkerSymbol();
                var pictureURI = new Uri("http://static.arcgis.com/images/Symbols/Basic/GreenStickpin.png");
                await matchSym.SetSourceAsync(pictureURI);

                var matchGraphic = new Graphic(matchLocation, matchSym);

                var graphicsLayer = MyMap.Layers["GeocodeResults"] as GraphicsLayer;
                graphicsLayer.Graphics.Add(matchGraphic);

                var matchExtent = new Envelope(matchLocation.X - 100,
                                               matchLocation.Y - 100,
                                               matchLocation.X + 100,
                                               matchLocation.Y + 100);
                await MyMapView.SetViewAsync(matchExtent);
            }
        }
    }
}
