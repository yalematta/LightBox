using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    public sealed class Box
    {
        public string id { get; set; }
        public string state { get; set; }
        public Owner owner { get; set; }
        public string pin { get; set; }
        public DateTimeOffset timeOn { get; set; }

        public Uri lightstate
        {
            get
            {
                if (state.Equals("on"))
                    return (new Uri("ms-appx:///Assets/onswtch.png", UriKind.RelativeOrAbsolute));
                else
                    return (new Uri("ms-appx:///Assets/offswtch.png", UriKind.RelativeOrAbsolute));
            }
        }
    }

    public sealed class Boxes
    {
        public List<Box> Box { get; set; }
    }
}
