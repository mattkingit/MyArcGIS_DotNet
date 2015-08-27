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

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;

namespace DrivingDirections
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

        private async void GetDirectionsButton_Click(object sender, RoutedEventArgs e)
        {
            var from = await this.FindAddress(this.FromTextBox.Text);
            var to = await FindAddress(ToTextBox.Text);

            try
            {
                if (from == null)
                {
                    throw new Exception("Unable to find a match for '" + this.FromTextBox.Text + "'.");
                }

                if (to == null)
                {
                    throw new Exception("Unable to find a match for '" + this.ToTextBox.Text + "'.");
                }

                // get the RouteResults graphics layer; add the from/to graphics
                var routeGraphics = MyMap.Layers["RouteResults"] as GraphicsLayer;
                if (routeGraphics == null)
                {
                    throw new Exception("A graphics layer named 'RouteResults' was not found in the map.");
                }

                // code here to show address locations on the map
                var fromMapPoint = GeometryEngine.Project(from.Geometry, new SpatialReference(102100));
                var toMapPoint = GeometryEngine.Project(to.Geometry, new SpatialReference(102100));

                var fromSym = new SimpleMarkerSymbol { Style = SimpleMarkerStyle.Circle, Size = 16, Color = Colors.Green };
                var toSym = new SimpleMarkerSymbol { Style = SimpleMarkerStyle.Circle, Size = 16, Color = Colors.Red };

                var fromMapGraphic = new Graphic { Geometry = fromMapPoint, Symbol = fromSym };
                var toMapGraphic = new Graphic { Geometry = toMapPoint, Symbol = toSym };

                routeGraphics.Graphics.Add(fromMapGraphic);
                routeGraphics.Graphics.Add(toMapGraphic);

            }
            catch (Exception exp)
            {
                this.DirectionsTextBlock.Text = exp.Message;
            }
        }

        private async System.Threading.Tasks.Task<Graphic> FindAddress(string address)
        {
            var uri = new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");
            var locator = new OnlineLocatorTask(uri, string.Empty);

            var findParams = new OnlineLocatorFindParameters(address);
            findParams.OutSpatialReference = new SpatialReference(4326);

            Graphic matchGraphic = null;
            var results = await locator.FindAsync(findParams, new System.Threading.CancellationToken());
            if (results.Count > 0)
            {
                var firstMatch = results[0].Feature;
                var matchLocation = firstMatch.Geometry as MapPoint;
                matchGraphic = new Graphic();
                matchGraphic.Geometry = matchLocation;
                matchGraphic.Attributes.Add("Name", address);
            }
            return matchGraphic;
        }
    }
}
