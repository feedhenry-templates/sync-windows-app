using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FHSDK.Sync;
using FHSDKPortable;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace sync_windows_app
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private const string DatasetId = "myShoppingList";
        public event PropertyChangedEventHandler PropertyChanged;
        private FHSyncDataset<ShoppingItem> _dataset;
        public ObservableCollection<ShoppingItem> ShoppingItems { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;

            Item.TextChanged += (sender, args) =>
            {
                if (string.IsNullOrEmpty(Item.Text))
                {
                    ListView.SelectedIndex = -1;
                    Button.Content = "Add";
                }
            };

            ListView.SelectionChanged += (sender, args) =>
            {
                if (ListView.SelectedItem == null) return;

                Item.Text = ((ShoppingItem)ListView.SelectedItem).Name;
                Button.Content = "Save";
            };

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (ListView.SelectedIndex != -1)
            {
                var shoppingItem = ShoppingItems[ListView.SelectedIndex];
                shoppingItem.Name = Item.Text;
                _dataset.Update(shoppingItem);

                //update item in observable list
                ShoppingItems.Remove(shoppingItem);
                ShoppingItems.Add(shoppingItem);
            }
            else if (!string.IsNullOrEmpty(Item.Text))
            {
                var shoppingItem = new ShoppingItem(Item.Text);
                _dataset.Create(shoppingItem);
                ShoppingItems.Add(shoppingItem);
            }

            Item.Text = "";
            PropertyChanged(this, new PropertyChangedEventArgs("ShoppingItems"));
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ListView.SelectedIndex == -1) return;
            var shoppingItem = ShoppingItems[ListView.SelectedIndex];
            ShoppingItems.Remove(shoppingItem);
            _dataset.Delete(shoppingItem.UID);
            PropertyChanged(this, new PropertyChangedEventArgs("ShoppingItems"));
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await FHClient.Init();
            _dataset = FHSyncDataset<ShoppingItem>.Build<ShoppingItem>(DatasetId, new FHSyncConfig(), null, null);

            _dataset.SyncNotificationHandler += (sender, args) =>
            {
                ShoppingItems = new ObservableCollection<ShoppingItem>(_dataset.List());
                PropertyChanged(this, new PropertyChangedEventArgs("ShoppingItems"));

            };

            await _dataset.StartSyncLoop();
        }
    }

    public class ShoppingItem : IFHSyncModel
    {
        public ShoppingItem(string name)
        {
            Name = name;
            Created = DateTime.Now;
        }

        [JsonProperty]
        public string Name { set; get; }

        [JsonProperty]
        public DateTime Created { set; get; }

        [JsonIgnore]
        public string UID { set; get; }

        public override string ToString()
        {
            return string.Format("[ShoppingItem: UID={0}, Name={1}, Created={2}]", UID, Name, Created);
        }
    }
}
