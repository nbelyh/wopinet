using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WopiCore.Models
{
    internal interface IPostMessageProperties
    {
        bool ClosePostMessage { get; set; }
        bool EditModePostMessage { get; set; }
        bool EditNotificationPostMessage { get; set; }
        bool FileSharingPostMessage { get; set; }
        string PostMessageOrigin { get; set; }

    }
}
