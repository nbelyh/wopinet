using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WopiCore.Models
{
    public class WopiHostProperties : IWopiHostProperties
    {
        internal WopiHostProperties()
        {
        }

        public bool AllowExternalMarketplace
        {
            get; set;
        }
        //public bool CloseButtonClosesWindow
        //{
        //    get; set;
        //}
        public int FileNameMaxLength
        {
            get; set;
        }
        internal WopiHostProperties Clone()
        {
            return (WopiHostProperties)MemberwiseClone();
        }
    }
}
