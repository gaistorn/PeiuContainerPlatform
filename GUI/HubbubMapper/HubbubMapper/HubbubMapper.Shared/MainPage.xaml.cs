using HubbubMapper.Shared;
using HubbubMapper.Shared.Dialogs;
using HubbubMapper.Shared.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HubbubMapper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            SelectSiteDialog dialog = new SelectSiteDialog();
            var result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                //GlobalProperty.Common. = dialog.SelectedHubbub;
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {

        }
        

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            FrameNavigationOptions navOptions = new FrameNavigationOptions();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            Type pageType = null;
            if (args.InvokedItemContainer.Tag?.ToString() == "HubbubConfiguration")
                pageType = typeof(ModbusHubbubInfoPage);

            
            if (pageType != null)
            {
                ContentFrame.NavigateToType(pageType, null, navOptions);

                btnUpload.IsEnabled = ContentFrame.Content is ISaveContents && (ContentFrame.Content as ISaveContents).IsSaveEnabled;
                
            }
        }
    }
}
