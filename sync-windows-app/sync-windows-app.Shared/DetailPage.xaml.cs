using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using FHSDK.Sync;

namespace sync_windows_app
{
    /// <summary>
    /// Detail page that can be used to create new or edit existing items depending on wether a ShoppingItem was passed
    /// </summary>
    public sealed partial class DetailPage : Page
    {
        private ShoppingItem _shoppingItem;

        public DetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _shoppingItem = (ShoppingItem) e.Parameter;
            if (_shoppingItem != null)
            {
                ItemName.Text = _shoppingItem.Name;
            }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var client = FHSyncClient.GetInstance();
            if (_shoppingItem == null)
            {
                client.Create(MainPage.DatasetId, new ShoppingItem(ItemName.Text));
            }
            else if (!string.IsNullOrEmpty(ItemName.Text))
            {
                _shoppingItem.Name = ItemName.Text;
                client.Update(MainPage.DatasetId, _shoppingItem);
            }

            Frame.Navigate(typeof (MainPage));
        }

        private void ItemText_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                SaveButton_OnClick(null, null);
            }
        }
    }
}
