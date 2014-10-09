using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LightBox.DataModels
{
    public sealed class YaleBoxes
    {
        public Boxes Boxes { get; set; }
    }

    public sealed class Owner
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string icon { get; set; }

    }

    public sealed class Box
    {
        public string id { get; set; }
        public string state { get; set; }
        public Owner owner { get; set; }
        public string pin { get; set; }
        public DateTimeOffset timeOn { get; set; }


        private Visibility _lightVis = Visibility.Visible;

        public Visibility lightVis
        {
            get
            {
                return _lightVis;
            }
            set
            {
                _lightVis = value;
            }
        }

        public Uri lightstate
        {
            get
            {
                if (state.Equals("on"))
                    return (new Uri("ms-appx:///Assets/Images/onswtch.png", UriKind.RelativeOrAbsolute));
                else
                    return (new Uri("ms-appx:///Assets/Images/offswtch.png", UriKind.RelativeOrAbsolute));
            }
        }

        private Uri _locker = new Uri("ms-appx:///Assets/Images/locker_closed.PNG", UriKind.RelativeOrAbsolute);

        public Uri locker
        {
            get
            {
                return _locker;
            }
            set
            {
                _locker = value;
            }
        }
    }

    public sealed class Boxes
    {
        public List<Box> Box { get; set; }
    }
}
