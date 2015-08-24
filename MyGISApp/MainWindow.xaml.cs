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

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.WebMap;

namespace MyGISApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ArcGISPortal arcGISOnline;
        private ArcGISPortalItem selectedPortalItem;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            var sel = combo.SelectedItem as ComboBoxItem;
            if (sel.Tag == null) { return; }

            // Find and remove the current basemap layer from the map
            if (MyMap == null) { return; }
            var oldBasemap = MyMap.Layers["BaseMap"];
            MyMap.Layers.Remove(oldBasemap);

            // Create a new basemap layer
            var newBasemap = new Esri.ArcGISRuntime.Layers.ArcGISTiledMapServiceLayer();

            // Set the ServiceUri with the url defined for the ComboBoxItem's Tag
            newBasemap.ServiceUri = sel.Tag.ToString();

            // Give the layer the same ID so it can still be found with the code above
            newBasemap.ID = "BaseMap";

            // Insert the new basemap layer as the first (bottom) layer in the map
            MyMap.Layers.Insert(0, newBasemap);

        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if first run, get a ref to the portal (ArcGIS Online)
                if (this.arcGISOnline == null)
                {
                    //create the uri for the portal
                    var portalUri = new Uri("https://www.arcgis.com/sharing/rest");

                    //create the portal
                    this.arcGISOnline = await ArcGISPortal.CreateAsync(portalUri);
                }
                //create a variable to store search results (collection of portal items
                IEnumerable<ArcGISPortalItem> results = null;
                if(this.ItemTypeComboBox.SelectedValue.ToString() == "Basemap")
                {
                    //basemap search returns web maps that contain the basemap layer
                    var basemapSearch = await this.arcGISOnline.ArcGISPortalInfo.SearchBasemapGalleryAsync();
                    results = basemapSearch.Results;
                }
                else
                {
                    //get the search term and item type provided it he UI
                    var searchTerm = this.SearchTextBox.Text.Trim();
                    var searchItem = this.ItemTypeComboBox.SelectedValue.ToString();

                    // build a query that searches for the specified type
                    // ('web mapping application' is excluded from the search since 'web map' will match those item types too)
                    var queryString = string.Format("\"{0}\" type:(\"{1}\" NOT \"web mapping application\")", searchTerm, searchItem);

                    // create a SearchParameters object, set options
                    var searchParameters = new SearchParameters()
                    {
                        QueryString = queryString,
                        SortField = "avgrating",
                        SortOrder = QuerySortOrder.Descending,
                        Limit = 10
                    };

                    //execute the search
                    var itemSearch = await this.arcGISOnline.SearchItemsAsync(searchParameters);
                    results = itemSearch.Results;
                }
                //show the results in the list box
                this.ResultsLIstBox.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error searching portal");
            }
        }

        private void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // clear UI controls
            this.ResetUI();

            // store the currently selected portal item
            this.selectedPortalItem = this.ResultsLIstBox.SelectedItem as ArcGISPortalItem;
            if (this.selectedPortalItem == null) { return; }

            //show the portal item snippet (brief description) in the UI
            if (!string.IsNullOrEmpty(this.selectedPortalItem.Snippet))
            {
                this.SnippetTextBlock.Text = this.selectedPortalItem.Snippet;
            }

            //show a thumbnail for the selected portal item (if there is one)
            if (this.selectedPortalItem.ThumbnailUri != null)
            {
                var src = new BitmapImage(this.selectedPortalItem.ThumbnailUri);
                this.ThumbnailImage.Source = src;
            }

            // enable the show map button when a web map ortal item is chosen
            this.ShowMapButton.IsEnabled = (this.selectedPortalItem.Type == ItemType.WebMap);
        }

        private void ResetUI()
        {
            // clear UI controls
            this.SnippetTextBlock.Text = "";
            this.ThumbnailImage.Source = null;
            this.ShowMapButton.IsEnabled = false;
        }

        private async void ShowMapButton_Click(object sender, RoutedEventArgs e)
        {
            // create a web map for the selected portal item
            var webMap = await WebMap.FromPortalItemAsync(this.selectedPortalItem);

            // load the web map into a web map view model
            var webMapVM = await WebMapViewModel.LoadAsync(webMap, this.arcGISOnline);

            //show the web map view model's map in the pages map view control
            this.MyMapView.Map = webMapVM.Map;
        }
    }
}
