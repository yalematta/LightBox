using LightBox.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
        YaleBoxes yale;
        string jsonData;
        StorageFile file, newfile;
        Uri dataUri = new Uri("ms-appx:///DataFiles/DataBoxes.txt");

        public MainPage()
        {
            this.InitializeComponent();
            ReadJsonFile();
        }

        private async void ReadJsonFile()
        {            
            file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            jsonData = await FileIO.ReadTextAsync(file);                      

            WriteJsonFile(jsonData);
        }

        private async void WriteJsonFile(string data)
        {
            if (newfile == null)
                newfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("DataBoxes.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(newfile, data);
            ParseJsonString(data);
        }


        private void ParseJsonString(string jsonData)
        {
            if (jsonData != null)
                yale = JsonConvert.DeserializeObject<YaleBoxes>(jsonData);

            Load_boxes();
        }

        private void Load_boxes()
        {
            BoxesGridView.ItemsSource = yale.Boxes.Box;
        }

        private void BoxesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            itemIndex = BoxesGridView.Items.IndexOf((e.ClickedItem));
            if (yale.Boxes.Box[itemIndex].state.Equals("off"))
            {
                OffCommandGrid.Visibility = Visibility.Visible;
                NotYouBlock.Text = "Not " + yale.Boxes.Box[itemIndex].owner.name + " ?";
                var yourTime = DateTimeOffset.Now - yale.Boxes.Box[itemIndex].timeOn;
                TimeBlock.Text = "You've been here for " + yourTime.Days.ToString() + " hours " + yourTime.Minutes.ToString() + " minutes.";
            }   
        }

        private void PinText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (PinText.Password.Equals(yale.Boxes.Box[itemIndex].pin))
                {
                    RetreivePhone();
                    
                }
                else
                {
                    WrongBlock.Visibility = Visibility.Visible;
                    ForgotGrid.Visibility = Visibility.Visible;
                }
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

        private void SetJsonFile()
        {
            string jsonContents = JsonConvert.SerializeObject(yale);
            WriteJsonFile(jsonContents);
        }

        private void RetreivePhone()
        {
            yale.Boxes.Box[itemIndex].state = ("on");
            yale.Boxes.Box[itemIndex].pin = ("");
            yale.Boxes.Box[itemIndex].owner.name = ("");
            yale.Boxes.Box[itemIndex].owner.phone = ("");
            yale.Boxes.Box[itemIndex].timeOn = DateTimeOffset.MinValue;

            SetJsonFile();
            ResetCommandGrid();
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (jsonData != null)
                WriteJsonFile(jsonData);
        }

        private void NotYouLink_Click(object sender, RoutedEventArgs e)
        {
            OffCommandGrid.Visibility = Visibility.Collapsed;
            PinText.Password = "";
            PinBox.Text = "";
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

        private async void PhoneBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (PhoneBox.Text.Equals(yale.Boxes.Box[itemIndex].owner.phone))
                {
                    RetreivePhone();
                }
                else
                {
                    WrongPhBlock.Visibility = Visibility.Visible;
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    ResetCommandGrid();
                }
            }
        }
    }
}
