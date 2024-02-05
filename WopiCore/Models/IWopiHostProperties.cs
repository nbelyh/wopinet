using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WopiCore.Models
{
    public interface IWopiHostProperties
    {
        bool AllowExternalMarketplace { get; set; }
        // bool CloseButtonClosesWindow { get; set; }
        int FileNameMaxLength { get; set; }
    }
}
