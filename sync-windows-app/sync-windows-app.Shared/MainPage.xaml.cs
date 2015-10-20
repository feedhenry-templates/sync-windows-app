using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using FHSDK.Sync;
using FHSDKPortable;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace sync_windows_app
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public const string DatasetId = "myShoppingList";

        public MainPage()
        {
            InitializeComponent();
            ShoppingItems = new ObservableCollection<ShoppingItem>();
            DataContext = this;

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        public ObservableCollection<ShoppingItem> ShoppingItems { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        ///     Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">
        ///     Event data that describes how this page was reached.
        ///     This parameter is typically used to configure the page.
        /// </param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var shoppingItem = e.Parameter as ShoppingItem;
            if (shoppingItem != null)
            {

                var index = ShoppingItems.IndexOf(shoppingItem);
                if (index != -1)
                {
                    ShoppingItems[index] = shoppingItem;
                }
                else
                {
                    ShoppingItems.Insert(0, shoppingItem);
                }

                FirePropertyChanged("ShoppingItems");
            }
            else
            {
                await FHClient.Init();
                var client = FHSyncClient.GetInstance();
                var config = new FHSyncConfig();
                client.Initialise(config);
                client.SyncCompleted += async (sender, args) =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ShoppingItems.Clear();
                        foreach (var item in client.List<ShoppingItem>(DatasetId).OrderByDescending(i => i.Created))
                        {
                            ShoppingItems.Add(item);
                        }
                        FirePropertyChanged("ShoppingItems");
                    });
                };

                ShoppingItems = new ObservableCollection<ShoppingItem>();
                client.Manage<ShoppingItem>(DatasetId, config, null);
            }
        }

        private void ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof (DetailPage), e.ClickedItem);
        }

        private void NewButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (DetailPage));
        }

        private void ListView_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListViewItem_Holding(sender, null);
        }

        private void ListViewItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            FlyoutBase.GetAttachedFlyout(senderElement).ShowAt(senderElement);
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShoppingItem shoppingItem;
            var menuFlyoutItem = sender as MenuFlyoutItem;
            if (menuFlyoutItem != null)
            {
                shoppingItem = menuFlyoutItem.DataContext as ShoppingItem;
                if (shoppingItem == null) return;
            }
            else
            {
                if (ListView.SelectedIndex == -1) return;
                shoppingItem = ShoppingItems[ListView.SelectedIndex];                
            }
            FHSyncClient.GetInstance().Delete<ShoppingItem>(DatasetId, shoppingItem.UID);
            ShoppingItems.Remove(shoppingItem);
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class ShoppingItem : IFHSyncModel
    {
        public ShoppingItem(string name)
        {
            Name = name;
            Created = DateTime.Now;
        }

        [JsonProperty("name")]
        public string Name { set; get; }

        [JsonProperty("created")]
        public DateTime Created { set; get; }

        [JsonIgnore]
        public string UID { set; get; }

        public override string ToString()
        {
            return string.Format("[ShoppingItem: UID={0}, Name={1}, Created={2}]", UID, Name, Created);
        }

        private bool Equals(IFHSyncModel other)
        {
            return string.Equals(UID, other.UID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ShoppingItem) obj);
        }

        public override int GetHashCode()
        {
            return (UID != null ? UID.GetHashCode() : 0);
        }
    }

    public sealed class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return value == null || (int) value > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return new NotImplementedException();
        }
    }
}