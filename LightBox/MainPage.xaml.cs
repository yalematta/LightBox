using LightBox.DataModels;
using LightBox.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LightBox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        int itemIndex;
        string yourPin;
        string jsonData;
        string fileName;
        DateTimeOffset yourTime;
        StorageFile file;
        DispatcherTimer dt = new DispatcherTimer();
        Uri dataUri = new Uri("ms-appx:///DataFiles/DataBoxes.txt");

        public MainPage()
        {
            this.InitializeComponent();

            App.main = this;

            dt.Interval = new TimeSpan(0, 0, 0, 1);

            ReadJsonFile();
        }

        //Reading JSON file (inside the app) when !isExist
        private async void ReadJsonFile()
        {
            bool isExist = false;
            try
            {
                App.newfile = await ApplicationData.Current.LocalFolder.GetFileAsync("DataBoxes.json");
                jsonData = await FileIO.ReadTextAsync(App.newfile);
                isExist = true;
            }

            catch (Exception)
            {
                isExist = false;
            }

            if (!isExist)
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                jsonData = await FileIO.ReadTextAsync(file);
            }
            App.WriteJsonFile(jsonData);
        }

        //Load the Flat-Icons GridView
        void Load_icons()
        {
            List<string> icons = new List<string>();
            for (int i = 1; i < 19; i++)
            {
                icons.Add("/Assets/Images/Flat-Icons/" + i + ".png");
            }
            IconsGridView.ItemsSource = icons;

            IconsGridView.Visibility = Visibility.Visible;
            CaptureButton.Visibility = Visibility.Visible;
        }

        //Load the BoxesGridView and fill the PhoneDict with contacts that exist in the Boxes
        public void Load_boxes()
        {
            BoxesGridView.ItemsSource = null;
            BoxesGridView.ItemsSource = App.yale.Boxes.Box;

            foreach (var item in App.yale.Boxes.Box)
            {
                if (App.PhoneDict.ContainsKey(item.id))
                {
                    App.PhoneDict[item.id] = item.owner.phone;
                }
                else
                {
                    App.PhoneDict.Add(item.id, item.owner.phone);
                }
            }
        }

        //Show the right command grids for each Box state
        private async void ShowGrids(string state)
        {
            try
            {
                dt.Stop();
            }
            catch (Exception e)
            {

            }

            ResetCommandGrid();

            if (state == "off")
            {
                OffCommandGrid.Visibility = Visibility.Visible;
                NotYouBlock.Text = "Not " + App.yale.Boxes.Box[itemIndex].owner.name + " ?";
                yourTime = App.yale.Boxes.Box[itemIndex].timeOn;

                if (App.yale.Boxes.Box[itemIndex].owner.icon.Contains("Assets"))
                {
                    IconImageOff.Source = new BitmapImage(new Uri(App.yale.Boxes.Box[itemIndex].owner.icon, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    //Code to get the Captured Saved image
                    var csi = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName + ".jpg");
                    IRandomAccessStream str = await csi.OpenAsync(FileAccessMode.Read);
                    BitmapImage b = new BitmapImage();

                    b.SetSource(str);

                    IconImageOff.Source = b;
                }

                dt.Tick += dt_Tick;
                dt.Start();
            }

            else if (state == "on")
            {
                App.yale.Boxes.Box[itemIndex].locker = new Uri("ms-appx:///Assets/Images/locker_opened.PNG", UriKind.Absolute);
                App.yale.Boxes.Box[itemIndex].lightVis = Visibility.Collapsed;
                OnCommandGrid.Visibility = Visibility.Visible;
                IconImage.Source = new BitmapImage(new Uri(App.yale.Boxes.Box[itemIndex].owner.icon, UriKind.RelativeOrAbsolute));

            }

            App.SetJsonFile();
        }

        private void ResetCommandGrid()
        {
            WrongBlock.Visibility = Visibility.Collapsed;
            ForgotGrid.Visibility = Visibility.Collapsed;
            WrongPhBlock.Visibility = Visibility.Collapsed;
            OffCommandGrid.Visibility = Visibility.Collapsed;
            PinText.Password = "";
            PinBox.Text = "";
            PhoneBox.Text = "";

            WrongName.Visibility = Visibility.Collapsed;
            WrongPhone.Visibility = Visibility.Collapsed;
            OnCommandGrid.Visibility = Visibility.Collapsed;
            NameBox.Text = "";
            OnPhoneBox.Text = "";
            PinBlock.Text = "";
            yourPin = "";

            foreach (var item in App.yale.Boxes.Box)
            {
                item.locker = new Uri("ms-appx:///Assets/Images/locker_closed.PNG", UriKind.Absolute);
                item.lightVis = Visibility.Visible;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (jsonData != null)
            {
                App.WriteJsonFile(jsonData);
            }
        }

        #region TimeMethods

        //Substract the time each phone has spend in his Box
        private void dt_Tick(object sender, object e)
        {
            PrepareTime(DateTimeOffset.Now - yourTime);
        }

        //Display the time each phone has spend in his Box
        private void PrepareTime(TimeSpan yourTime)
        {
            TimeBlock.Text = "You've been here since " + TimeSpanFormattingExtensions.ToReadableString(yourTime);
        }

        #endregion

        #region TextBoxesManipulation

        Windows.System.VirtualKey tempKey;
        private void PinText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (tempKey != null && tempKey == Windows.System.VirtualKey.Shift)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = !e.Key.ToString().Contains("Number");
            }

            tempKey = e.Key;

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                RetreiveMethod();
            }
        }

        private void PinText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PinText.Password.Equals(""))
            {
                PinBox.Text = "PIN";

                PinBox.Visibility = Visibility.Visible;
                PinText.Visibility = Visibility.Collapsed;
            }
        }

        private void PinBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PinBox.Visibility = Visibility.Collapsed;
            PinText.Visibility = Visibility.Visible;

            PinText.Focus(FocusState.Keyboard);
        }

        private void PhoneBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PhoneBox.Text.Equals("Phone"))
            {
                PhoneBox.Text = "";
            }
            PhoneBox.Focus(FocusState.Keyboard);
        }

        private void PhoneBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PhoneBox.Text.Equals(""))
            {
                PhoneBox.Text = "Phone";
            }
        }

        private void PhoneBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (tempKey != null && tempKey == Windows.System.VirtualKey.Shift)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = !e.Key.ToString().Contains("Number");
            }

            tempKey = e.Key;

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                RetreiveMethod();
            }
        }

        private void NameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NameBox.Focus(FocusState.Keyboard);
        }

        private void NameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text != "" && OnPhoneBox.Text != "")
            {
                if (yourPin.Equals(""))
                    yourPin = GeneratePin(App.yale.Boxes.Box[itemIndex].id);
                PinBlock.Text = "Your PIN     " + yourPin;
            }
        }

        private void OnPhoneBox_GotFocus(object sender, RoutedEventArgs e)
        {
            OnPhoneBox.Focus(FocusState.Keyboard);
        }

        private async void OnPhoneBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (tempKey != null && tempKey == Windows.System.VirtualKey.Shift)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = !e.Key.ToString().Contains("Number");
            }

            tempKey = e.Key;

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ShowPin();
                await Task.Delay(2000);
                InsertMethod();
            }
        }

        private void OnPhoneBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (OnPhoneBox.Text != "")
                ShowPin();
        }

        #endregion

        #region BoxManipulation

        private void RetreivePhone()
        {
            App.yale.Boxes.Box[itemIndex].state = "on";
            App.yale.Boxes.Box[itemIndex].pin = "";
            App.yale.Boxes.Box[itemIndex].owner.name = "";
            App.yale.Boxes.Box[itemIndex].owner.phone = "";
            App.yale.Boxes.Box[itemIndex].owner.icon = "ms-appx:///Assets/Images/Flat-Icons/default-account.png";
            App.yale.Boxes.Box[itemIndex].timeOn = DateTimeOffset.MinValue;

            App.SetJsonFile();
            ResetCommandGrid();
        }

        private void InsertPhone()
        {
            App.yale.Boxes.Box[itemIndex].pin = yourPin;
            App.yale.Boxes.Box[itemIndex].state = "off";
            App.yale.Boxes.Box[itemIndex].timeOn = DateTimeOffset.Now;
            App.yale.Boxes.Box[itemIndex].owner.name = NameBox.Text;
            App.yale.Boxes.Box[itemIndex].owner.phone = OnPhoneBox.Text;
            //Code to get the AbsolutUri of an image (with ms-appx://...)
            //App.yale.Boxes.Box[itemIndex].owner.icon = (IconImage.Source as BitmapImage).UriSource.AbsoluteUri.ToString();

            ////Code to get the Captured Saved image
            //var csi = await ApplicationData.Current.LocalFolder.GetFileAsync("whatever.jpg");
            ////Stream str = await csi.OpenStreamForReadAsync();
            //IRandomAccessStream str = await csi.OpenAsync(FileAccessMode.Read);
            //BitmapImage b = new BitmapImage();

            //b.SetSource(str);
          App.yale.Boxes.Box[itemIndex].owner.icon = fileName + ".jpg";

            App.SetJsonFile();
            ResetCommandGrid();
        }

        private void InsertMethod()
        {
            if (NameBox.Text.Equals(""))
                WrongName.Visibility = Visibility.Visible;

            if (OnPhoneBox.Text.Equals("") || App.PhoneDict.ContainsValue(OnPhoneBox.Text))
                WrongPhone.Visibility = Visibility.Visible;

            else if (!NameBox.Text.Equals("") && !OnPhoneBox.Text.Equals("") && !App.PhoneDict.ContainsValue(OnPhoneBox.Text))
            {
                WrongName.Visibility = Visibility.Collapsed;
                WrongPhone.Visibility = Visibility.Collapsed;
                App.yale.Boxes.Box[itemIndex].locker = new Uri("ms-appx:///Assets/Images/locker_closed.PNG", UriKind.Absolute);
                App.yale.Boxes.Box[itemIndex].lightVis = Visibility.Visible;

                BoxesGridView.ItemsSource = App.yale.Boxes.Box;
                InsertPhone();
            }
        }

        private void RetreiveMethod()
        {
            if (!PinText.Password.Equals(""))
            {
                if (PinText.Password.Equals(App.yale.Boxes.Box[itemIndex].pin))
                {
                    RetreivePhone();
                }
                else
                {
                    WrongBlock.Visibility = Visibility.Visible;
                    ForgotGrid.Visibility = Visibility.Visible;
                    PhoneBox.Focus(FocusState.Keyboard);
                    PinText.Password = "";
                }
            }

            else if (!PhoneBox.Text.Equals(""))
            {
                if (PhoneBox.Text.Equals(App.yale.Boxes.Box[itemIndex].owner.phone))
                {
                    RetreivePhone();
                }
                else
                {
                    WrongPhBlock.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region PinMethods

        private string GeneratePin(string id)
        {
            string number = "";
            Random rng = new Random();
            number = (rng.Next(100, 999)).ToString();
            return id + number;
        }

        private void ShowPin()
        {
            if (App.PhoneDict.ContainsValue(OnPhoneBox.Text))
            {
                WrongPhone.Visibility = Visibility.Visible;
                PinBlock.Text = "This phone already exists.";
            }

            else if (NameBox.Text != "" && OnPhoneBox.Text != "")
            {
                if (yourPin.Equals(""))
                    yourPin = GeneratePin(App.yale.Boxes.Box[itemIndex].id);
                PinBlock.Text = "Your PIN     " + yourPin;
                WrongPhone.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region SelectionChangedEvents

        private void BoxesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                itemIndex = BoxesGridView.Items.IndexOf((BoxesGridView.SelectedItem));

                ShowGrids(App.yale.Boxes.Box[itemIndex].state);

                BoxesGridView.SelectedItem = null;
                BoxesGridView.SelectedIndex = -1;
            }
        }

        //When choosing a profile picture from the IconsGridView
        private void IconsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string str = IconsGridView.SelectedItem as string;
                IconImage.Source = new BitmapImage(new Uri("ms-appx://" + str, UriKind.RelativeOrAbsolute));
                IconsGridView.Visibility = Visibility.Collapsed;
                CaptureButton.Visibility = Visibility.Collapsed;
                BoxesGridView.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region ClickingButtons

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCommandGrid();
            BoxesGridView.Visibility = Visibility.Collapsed;
            IconsGridView.Visibility = Visibility.Collapsed;
            CaptureButton.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Collapsed;
            UserGrid.Visibility = Visibility.Visible;
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdminPage));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCommandGrid();
        }

        private void CustButton_Click(object sender, RoutedEventArgs e)
        {
            UserGrid.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Visible;
            BoxesGridView.Visibility = Visibility.Visible;
        }

        private void OnCancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.yale.Boxes.Box[itemIndex].locker = new Uri("ms-appx:///Assets/Images/locker_closed.PNG", UriKind.Absolute);
            App.yale.Boxes.Box[itemIndex].lightVis = Visibility.Visible;
            IconsGridView.Visibility = Visibility.Collapsed;
            CaptureButton.Visibility = Visibility.Collapsed;

            BoxesGridView.ItemsSource = App.yale.Boxes.Box;
            BoxesGridView.Visibility = Visibility.Visible;
            App.SetJsonFile();
            ResetCommandGrid();
        }

        private void RetreiveButton_Click(object sender, RoutedEventArgs e)
        {
            RetreiveMethod();
        }

        private async void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnPhoneBox.Text != "")
            {
                ShowPin();
                await Task.Delay(2000);
                InsertMethod();
            }
        }

        private void NotYouLink_Click(object sender, RoutedEventArgs e)
        {
            OffCommandGrid.Visibility = Visibility.Collapsed;
            PinText.Password = "";
            PinBox.Text = "";
        }

        //When tapping on the default profile picture
        private void IconImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BoxesGridView.Visibility = Visibility.Collapsed;
            Load_icons();
        }

        #endregion

        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Using Windows.Media.Capture.CameraCaptureUI API to capture a photo
                CameraCaptureUI dialog = new CameraCaptureUI();
                Size aspectRatio = new Size(100, 100);
                dialog.PhotoSettings.CroppedAspectRatio = aspectRatio;

                StorageFile photo = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
                if (photo != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    using (IRandomAccessStream fileStream = await photo.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        bitmapImage.SetSource(fileStream);
                    }
                    IconImage.Source = bitmapImage;

                    fileName = Path.GetRandomFileName();

                    var targetFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName + ".jpg");
                    if (targetFile != null)
                    {
                        await photo.MoveAndReplaceAsync(targetFile);
                    }

                    IconsGridView.Visibility = Visibility.Collapsed;
                    CaptureButton.Visibility = Visibility.Collapsed;
                    BoxesGridView.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageDialog msgDialog = new MessageDialog("No photo captured.", "StatusMessage");
                }
            }
            catch (Exception ex)
            {
                MessageDialog msgDialog = new MessageDialog(ex.Message, "ErrorMessage");
            }
        }
    }
}
