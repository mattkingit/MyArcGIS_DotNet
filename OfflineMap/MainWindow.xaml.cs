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
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();

            MyMapView.NavigationCompleted += (s, e) => { GenerateLocalTilesButton.IsEnabled = MyMapView.Scale < 6000000; };
            MyMapView.Loaded += MyMapView_Loaded;
        }

        private void MyMapView_Loaded(object sender, RoutedEventArgs e)
        {
            // try to load the online layers
            TryLoadOnlineLayers();
        }

        private async void GetTiles(object sender, RoutedEventArgs e)
        {
            try
            {
                // show the status controls
                StatusPanel.Visibility = Visibility.Visible;
                StatusMessageList.Items.Add("Requesting tile cache ...");

                // cancel if an earlier call was made
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                }

                // get a cancellation token for this task
                cancellationTokenSource = new CancellationTokenSource();
                var cancelToken = cancellationTokenSource.Token;

                // create a new ExportTileCachTask to generate the tiles
                var exportTilesTask = new ExportTileCacheTask(new Uri(basemapUrl));

                //define options for the new tiles (extent, scale levels, format)
                var generateOptions = new GenerateTileCacheParameters();
                generateOptions.Format = ExportTileCacheFormat.CompactCache;
                generateOptions.MinScale = 6000000.0;
                generateOptions.MaxScale = 1.0;

                // download the tile package to the app's local folder
                var outFolder = System.AppDomain.CurrentDomain.BaseDirectory;
                var downloadOptions = new DownloadTileCacheParameters(outFolder);

                //overwrite the file if it already exists
                downloadOptions.OverwriteExistingFiles = true;

                // check generation progress every two seconds
                var checkInterval = TimeSpan.FromSeconds(2);

                var creationProgress = new Progress<ExportTileCacheJob>(p =>
                {
                    StatusMessageList.Items.Clear();
                    foreach(var m in p.Messages)
                    {
                        // find messages with percent coplete
                        // "Finished:: 9 percent", e.g.
                        if (m.Description.Contains("Finished::"))
                        {
                            // parse out the percentage complete and update the progress bar
                            var numString = m.Description.Substring(m.Description.IndexOf("::") + 2, 3).Trim();
                            var pct = 0.0;
                            if (double.TryParse(numString,out pct))
                            {
                                StatusProgressBar.Value = pct;
                            }
                            else
                            {
                                // show other status message in the list
                                StatusMessageList.Items.Add(m.Description);
                            }
                        }
                    }
                });

                // dhow download progress
                var downloadProgress = new Progress<ExportTileCacheDownloadProgress>(p =>
                {
                    StatusProgressBar.Value = p.ProgressPercentage;
                });

                // generate the tiles and download them
                var result = await exportTilesTask.GenerateTileCacheAndDownloadAsync(generateOptions,
                    downloadOptions, checkInterval, cancelToken, creationProgress, downloadProgress);

                // when the task completes, store the path to the local tile cache
                this.localTileCachePath = result.OutputPath;
                LocalDataPathTextBlock.Text = this.localTileCachePath;
                LocalTilesPathTextBlock.ToolTip = this.localTileCachePath;

                //clear the wroking messages, report success
                StatusProgressBar.Value = 100;
                StatusMessageList.Items.Clear();
                StatusMessageList.Items.Add("Local tiles created at " + this.localTileCachePath);
            }
            catch (Exception exp)
            {
                StatusMessageList.Items.Clear();
                StatusMessageList.Items.Add("Unable to get local tiles: " + exp.Message);
            }
            finally
            {
                // reset the progress indicator
                StatusProgressBar.Value = 0;
            }
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
