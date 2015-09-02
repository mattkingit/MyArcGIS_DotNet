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
                StatusMessagesList.Items.Add("Requesting tile cache ...");

                // cancel if an earlier call was made
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                }

                // get a cancellation token for this task
                cancellationTokenSource = new CancellationTokenSource();
                var cancelToken = cancellationTokenSource.Token;

                // create a new ExportTileCacheTask to generate the tiles
                var exportTilesTask = new ExportTileCacheTask(new Uri(basemapUrl));

                //define options for the new tiles (extent, scale levels, format)
                var generateOptions = new GenerateTileCacheParameters();
                generateOptions.Format = ExportTileCacheFormat.CompactCache;
                generateOptions.GeometryFilter = MyMapView.Extent;
                generateOptions.MinScale = 6000000.0;
                generateOptions.MaxScale = 1.0;

                // download the tile package to the app's local folder
                var outFolder = System.AppDomain.CurrentDomain.BaseDirectory;
                var downloadOptions = new DownloadTileCacheParameters(outFolder);

                // overwrite the file if it already exists
                downloadOptions.OverwriteExistingFiles = true;

                // check generation progress every two seconds
                var checkInterval = TimeSpan.FromSeconds(2);

                var creationProgress = new Progress<ExportTileCacheJob>(p =>
                {
                    StatusMessagesList.Items.Clear();
                    foreach (var m in p.Messages)
                    {
                        // find messages with percent complete
                        // "Finished:: 9 percent", e.g.
                        if (m.Description.Contains("Finished::"))
                        {
                            // parse out the percentage complete and update the progress bar
                            var numString = m.Description.Substring(m.Description.IndexOf("::") + 2, 3).Trim();
                            var pct = 0.0;
                            if (double.TryParse(numString, out pct))
                            {
                                StatusProgressBar.Value = pct;
                            }
                        }
                        else
                        {
                            // show other status messages in the list
                            StatusMessagesList.Items.Add(m.Description);
                        }
                    }
                });

                // show download progress 
                var downloadProgress = new Progress<ExportTileCacheDownloadProgress>(p =>
                {
                    StatusProgressBar.Value = p.ProgressPercentage;
                });

                // generate the tiles and download them 
                var result = await exportTilesTask.GenerateTileCacheAndDownloadAsync(generateOptions,
                                                                                     downloadOptions,
                                                                                     checkInterval,
                                                                                     cancelToken,
                                                                                     creationProgress,
                                                                                     downloadProgress);

                // when complete, store the path to the new local tile cache
                this.localTileCachePath = result.OutputPath;
                LocalTilesPathTextBlock.Text = this.localTileCachePath;
                LocalTilesPathTextBlock.ToolTip = this.localTileCachePath;


                // clear the working messages, report success
                StatusProgressBar.Value = 100;
                StatusMessagesList.Items.Clear();
                StatusMessagesList.Items.Add("Local tiles created at " + this.localTileCachePath);
            }
            catch (Exception exp)
            {
                StatusMessagesList.Items.Clear();
                StatusMessagesList.Items.Add("Unable to get local tiles: " + exp.Message);
            }
            finally
            {
                // reset the progress indicator
                StatusProgressBar.Value = 0;
            }
        }

        private void DataOptionChecked(object sender, RoutedEventArgs e)
        {
            MyMapView.Map.Layers.Clear();

            if (UseOnlineDataOption.IsChecked == true)
            {
                TryLoadOnlineLayers();
            }
            else // offline
            {
                TryLoadLocalLayers();
            }
        }

        private async void GetFeatures(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusMessagesList.Items.Add("Submitting generate geodatabase job ...");
                StatusProgressBar.IsIndeterminate = true;
                StatusProgressBar.IsEnabled = true;

                // cancel if an erliear call was made
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                }

                // get a cancellation token
                cancellationTokenSource = new CancellationTokenSource();
                var cancelToken = cancellationTokenSource.Token;

                // create a new GeodatabaseSncTask with the uri of the feature service to pull from
                var serverUrl = operationalUrl.Substring(0, operationalUrl.LastIndexOf('/'));
                var uri = new Uri(serverUrl);
                var getFeaturesTask = new GeodatabaseSyncTask(uri);

                // crate parameters for the task: layers and extent to include, out spation reference and sync model
                var layers = new List<int>(new int[1] { 0 }); //just get the first layer
                var extent = MyMapView.Extent;
                var getFeaturesParams = new GenerateGeodatabaseParameters(layers, extent)
                {
                    OutSpatialReference = MyMapView.SpatialReference,
                    SyncModel = SyncModel.PerLayer
                };

                // check progress every two seconds
                var checkInterval = TimeSpan.FromSeconds(2);
                var creationProgress = new Progress<GeodatabaseStatusInfo>(p =>
                {
                    this.StatusMessagesList.Items.Add(DateTime.Now.ToShortTimeString() + ": " + p.Status);
                });

                // call GenerateGeodatabaseAsync, the GenerateFeautresCompleteCallback callback will execute when it's complete
                var gdbResults = await getFeaturesTask.GenerateGeodatabaseAsync(getFeaturesParams,
                                                                                GenerateFeatuersCompleteCallback,
                                                                                checkInterval,
                                                                                creationProgress,
                                                                                cancelToken);

            }
            catch (Exception ex)
            {
                StatusMessagesList.Items.Add("Unable to create offline database: " + ex.Message);
                StatusProgressBar.IsIndeterminate = false;
            }
        }

        private async void TryLoadOnlineLayers()
        {
            try
            {
                // create an online tiled map service layer, an online feature layer
                var basemapLayer = new ArcGISTiledMapServiceLayer(new Uri(basemapUrl));
                var operationalLayer = new FeatureLayer(new Uri(operationalUrl));

                // give the feature layer an ID so it can be found later
                operationalLayer.ID = "Sightings";

                // initialize the layers
                await basemapLayer.InitializeAsync();
                await operationalLayer.InitializeAsync();

                // see if there was an exception when initializing the layers, if so throw an exception
                if (basemapLayer.InitializationException != null ||
                    operationalLayer.InitializationException != null)
                {
                    // unable to load one or more of the layers, throw an exception
                    throw new Exception("Could not initialize layers");
                }

                // add layers
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

        private async void TryLoadLocalLayers()
        {
            try
            {
                if (string.IsNullOrEmpty(this.localGeodatabasePath))
                {
                    throw new Exception("Local features do not yet exist. Please generate them first.");
                }

                if (string.IsNullOrEmpty(this.localTileCachePath))
                {
                    throw new Exception("Local tiles do not yet exist. Please generate them first.");
                }

                // create a local tiled layer
                var basemapLayer = new ArcGISLocalTiledLayer(this.localTileCachePath);

                // open the local geodatabase, get the first (only) table, create a FeatureLayer to display it
                var localGdb = await Geodatabase.OpenAsync(this.localGeodatabasePath);
                var gdbTable = localGdb.FeatureTables.FirstOrDefault();
                var operationalLayer = new FeatureLayer(gdbTable);

                // give the feature layer an ID so it can be found later
                operationalLayer.ID = "Sightings";

                await basemapLayer.InitializeAsync();
                await operationalLayer.InitializeAsync();

                // see if there was an exception when initializing the layer
                if (basemapLayer.InitializationException != null || operationalLayer.InitializationException != null)
                {
                    // unable to load one or more of the layers, throw an exception
                    throw new Exception("Could not initialize local layers");
                }

                // add the local layers to the map
                MyMapView.Map.Layers.Add(basemapLayer);
                MyMapView.Map.Layers.Add(operationalLayer);

            }
            catch (Exception exp)
            {
                MessageBox.Show("Unable to load local layers: " + exp.Message, "Load Layers");
            }
        }

        private async void GenerateFeatuersCompleteCallback(GeodatabaseStatusInfo statusInfo, Exception ex)
        {
            if(ex != null)
            {
                this.Dispatcher.Invoke(() => StatusMessagesList.Items.Add("An exception has occured: " + ex.Message));
                return;
            }

            // if successful, download the generated geodatabase from the server
            var client = new ArcGISHttpClient();
            var geodatabaseStream = client.GetOrPostAsync(statusInfo.ResultUri, null);

            // create a path for the local geodatabse
            var outFolder = System.AppDomain.CurrentDomain.BaseDirectory;
            var geodatabasePath = System.IO.Path.Combine(outFolder, "Wildlife.geodatabase");

            await Task.Factory.StartNew(async delegate
            {
                using (var stream = System.IO.File.Create(geodatabasePath))
                {
                    await geodatabaseStream.Result.Content.CopyToAsync(stream);
                }

                this.localGeodatabasePath = geodatabasePath;
                this.Dispatcher.Invoke(() => LocalDataPathTextBlock.Text = geodatabasePath);
                this.Dispatcher.Invoke(() => LocalDataPathTextBlock.ToolTip = geodatabasePath);
                this.Dispatcher.Invoke(() => StatusMessagesList.Items.Add("Features downloaded to " + geodatabasePath));
                this.Dispatcher.Invoke(() => StatusProgressBar.IsIndeterminate = false);
            });

        }
    }
}
