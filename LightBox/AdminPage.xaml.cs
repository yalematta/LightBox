using LightBox.DataModels;
using LightBox.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LightBox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminPage : Page
    {
        private readonly Random Rng = new Random();
        List<Box> offYale = new List<Box>();
        Windows.System.VirtualKey tempKey;
        //IEnumerable<int> myValues = Enumerable.Empty<int>();

        public AdminPage()
        {
            this.InitializeComponent();
            FillOffBoxes();
        }

        //Method for choosing the winner
        private async void NextNumber()
        {
            string chosenId = "";
            string chosenName = "";
            string chosenPhone = "";

            if (App.yale.Boxes.Box[0].state.Equals("off"))
                chosenId = App.yale.Boxes.Box[0].id;

            int max = offYale.Count();

            if (offYale.Count().Equals(0) || offYale.Count().Equals(1))
            {
                MessageDialog msgDialog = new MessageDialog("There is not enough boxes to launch the game!", "");

                UICommand yale = new UICommand("Close");
                yale.Invoked = CloseButtonClicked;
                msgDialog.Commands.Add(yale);

                await msgDialog.ShowAsync();
            }

            else
            {
                for (int ind = 1; ind < max; ind++)
                {
                    TimeSpan time1 = DateTimeOffset.Now - offYale[ind-1].timeOn;
                    TimeSpan time2 = DateTimeOffset.Now - offYale[ind].timeOn;

                    if (time2 > time1)
                    {
                        chosenId = offYale[ind].id;
                        chosenName = offYale[ind].owner.name;
                        chosenPhone = offYale[ind].owner.phone;
                    }
                }
            }

            for (int j = 1; j < 100; j++)
            {
                Random r = new Random();
                int i = Rng.Next(max) + 1;
                //IEnumerable<int> oneRandom = myValues.Shuffle().Take(1); 
                await Task.Delay(100);
                IdText.Text = i.ToString();
            }

            IdText.Text = chosenId;
            WinnerGrid.Visibility = Visibility.Visible;
            NameBlock.Text = chosenName;
            PhoneBlock.Text = chosenPhone;
        }

        //Grouping the full Boxes in a offYale list
        private void FillOffBoxes()
        {
            foreach (var item in App.yale.Boxes.Box)
            {
                if (item.state.Equals("off"))
                {
                    offYale.Add(item);     
                    //myValues = myValues.Concat(new[] { (int.Parse(item.id)) });
                }
            }               
        }

        private void CreateBoxes()
        {
            if (NumberBox.Text != "")
            {
                int max = int.Parse(NumberBox.Text);

                App.yale.Boxes.Box = new List<Box>();
                App.PhoneDict.Clear();

                for (int i = 0; i < max; i++)
                {
                    App.yale.Boxes.Box.Add(new Box
                    {
                        id = (i + 1) + "",
                        state = "on",
                        pin = "",
                        timeOn = DateTimeOffset.MinValue,
                        owner = new Owner { name = "", phone = "", icon = "ms-appx:///Assets/Images/Flat-Icons/default-account.png" }
                    });
                }
                App.SetJsonFile();
                this.Frame.GoBack();
            }

            else
                NumberBox.Focus(FocusState.Keyboard);
        }

        private void Reset1Box()
        {
            if (Number1Box.Text != "")
            {
                int num = int.Parse(Number1Box.Text);

                if (App.PhoneDict.ContainsKey(Number1Box.Text))
                {
                    App.yale.Boxes.Box[num - 1].state = "on";
                    App.yale.Boxes.Box[num - 1].pin = "";
                    App.yale.Boxes.Box[num - 1].timeOn = DateTimeOffset.MinValue;
                    App.yale.Boxes.Box[num - 1].owner.name = "";
                    App.yale.Boxes.Box[num - 1].owner.phone = "";
                    App.yale.Boxes.Box[num - 1].owner.icon = "ms-appx:///Assets/Images/Flat-Icons/default-account.png";

                    App.SetJsonFile();
                    this.Frame.GoBack();
                }
            }
            else
                NumberBox.Focus(FocusState.Keyboard);
        }

        #region KeyDownMethods

        private void NumberBox_KeyDown(object sender, KeyRoutedEventArgs e)
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
                CreateBoxes();
            }
        }

        private void Number1Box_KeyDown(object sender, KeyRoutedEventArgs e)
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
                Reset1Box();
            }
        }

        #endregion

        #region ClickingButtons

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseGrid.Visibility.Equals(Visibility.Visible))
                this.Frame.GoBack();

            else if (WinnerGrid.Visibility.Equals(Visibility.Visible) || GameGrid.Visibility.Equals(Visibility.Visible))
            {
                WinnerGrid.Visibility = Visibility.Collapsed;
                GameGrid.Visibility = Visibility.Collapsed;
                ChooseGrid.Visibility = Visibility.Visible;
            }

            else if (ManageGrid.Visibility.Equals(Visibility.Visible) || ResetAllGrid.Visibility.Equals(Visibility.Visible) || ResetBoxGrid.Visibility.Equals(Visibility.Visible))
            {
                ManageGrid.Visibility = Visibility.Collapsed;
                ResetAllGrid.Visibility = Visibility.Collapsed;
                ResetBoxGrid.Visibility = Visibility.Collapsed;
                ChooseGrid.Visibility = Visibility.Visible;
            }
        }

        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseGrid.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Visible;
            NextNumber();
        }

        private void CloseButtonClicked(IUICommand command)
        {
            WinnerGrid.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Collapsed;
            ChooseGrid.Visibility = Visibility.Visible;
        }

        private void ManageButton_Click(object sender, RoutedEventArgs e)
        {
            int OnBoxes = App.yale.Boxes.Box.Count() - offYale.Count();
            ChooseGrid.Visibility = Visibility.Collapsed;
            ManageGrid.Visibility = Visibility.Visible;
            TotalBlock.Text = "We have " + App.yale.Boxes.Box.Count() + " boxes.";
            FullBlock.Text = offYale.Count() + " boxes are full.";
            EmptyBlock.Text = OnBoxes + " boxes are empty.";
        }

        private void ResetAllButton_Click(object sender, RoutedEventArgs e)
        {
            ManageGrid.Visibility = Visibility.Collapsed;
            ResetAllGrid.Visibility = Visibility.Visible;
            SetBlock.Text = "Set the number of boxes";
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            CreateBoxes();
        }

        private void ResetBoxButton_Click(object sender, RoutedEventArgs e)
        {
            ManageGrid.Visibility = Visibility.Collapsed;
            ResetBoxGrid.Visibility = Visibility.Visible;
            Set1Block.Text = "Set the box number";
        }

        private void Done1Button_Click(object sender, RoutedEventArgs e)
        {
            Reset1Box();
        }

        private void Cancel1Button_Click(object sender, RoutedEventArgs e)
        {
            ManageGrid.Visibility = Visibility.Collapsed;
            ResetAllGrid.Visibility = Visibility.Collapsed;
            ResetBoxGrid.Visibility = Visibility.Collapsed;
            ChooseGrid.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ManageGrid.Visibility = Visibility.Collapsed;
            ResetAllGrid.Visibility = Visibility.Collapsed;
            ResetBoxGrid.Visibility = Visibility.Collapsed;
            ChooseGrid.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
