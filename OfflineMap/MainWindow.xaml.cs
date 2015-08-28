using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Offline;


namespace OfflineMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const string basemapUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer";
        private const string operationalUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer/0";

        private string localTileCachePath;
        private string localGeodatabasePath;

        public MainWindow()
        {
            InitializeComponent();

            MyMapView.Loaded += MyMapView_Loaded;
        }

        private void MyMapView_Loaded(object sender, RoutedEventArgs e)
        {
            // try to load the online layers
            TryLoadOnlineLayers();
        }

        private void GetTiles(object sender, RoutedEventArgs e)
        {

        }

        private void DataOptionChecked(object sender, RoutedEventArgs e)
        {

        }

        private void GetFeatures(object sender, RoutedEventArgs e)
        {

        }

        private async void TryLoadOnlineLayers()
        {
            try
            {
                //handle a variety of possible exceptions
                var basemapLayer = new ArcGISTiledMapServiceLayer(new Uri(basemapUrl));
                var operationalLayer = new FeatureLayer(new Uri(operationalUrl));

                // give the feature layer an ID so it can be found later
                operationalLayer.ID = "Sightings";

                // initialize the layers
                await basemapLayer.InitializeAsync();
                await operationalLayer.InitializeAsync();

                // see if there was an exception when initializing the layers, if so throw an exception
                if (basemapLayer.InitializationException != null || operationalLayer.InitializationException != null)
                {
                    // unable to load one or more of the layers, throw an exception
                    throw new Exception("Could not initialize layers");
                }
                //add layers
                MyMapView.Map.Layers.Add(basemapLayer);
                MyMapView.Map.Layers.Add(operationalLayer);
            }
            catch (ArcGISWebException arcGISExp)
            {
                // token required?
                MessageBox.Show("Unable to load online layers: credentials may be required", "Load error");

            }
            catch (System.Net.Http.HttpRequestException httpExp)
            {
                //not connected? server down? wrong URI?
                MessageBox.Show("Unable to load online layers: check your connection and verify service URLs", "Load Error");
            }
            catch (Exception exp)
            {
                //other problems ...
                MessageBox.Show("Unable to load online layers: " + exp.Message, "Load Error");
            }
        }
    }
}
