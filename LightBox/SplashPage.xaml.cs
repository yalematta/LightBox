using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LightBox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class SplashPage : Page
    {
        int navcount = 1;
        int loaderCount = 2;
        DispatcherTimer dt = new DispatcherTimer();

        public SplashPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            dt.Interval = new TimeSpan (0, 0, 0, 0, 400); //(0, 0, 0, 0, 180);
            dt.Tick += dt_Tick;
            dt.Start();
        }

        void dt_Tick(object sender, object e)
        {
            Uri uri = new Uri("ms-appx:/Assets/yale splash/" + loaderCount + ".png", UriKind.RelativeOrAbsolute);
            imageSplash.Source = new BitmapImage(uri);
            sound.Play();
           
            loaderCount++;

            if (loaderCount > 3)
            {
                loaderCount = 1;
                navcount++;
            }

            if(navcount==4)
            {
                Frame.Navigate(typeof(MainPage));
                dt.Stop();
            }
        }
    }
}
