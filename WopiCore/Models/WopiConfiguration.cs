﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WopiCore.Models
{
    public class WopiConfiguration
    {
        static WopiConfiguration()
        {
            WopiHostCapabilities = new WopiHostCapabilities()
            {
                SupportsCobalt = false,
                SupportsContainers = false,
                SupportsDeleteFile = false,
                SupportsEcosystem = false,
                SupportsExtendedLockLength = true,
                SupportsFolders = false,
                SupportsGetLock = true,
                SupportsLocks = true,
                SupportsRename = false,
                SupportsUpdate = true,
                SupportsUserInfo = false
            };

            WopiHostProperties = new WopiHostProperties()
            {
                AllowExternalMarketplace = true,
                // CloseButtonClosesWindow = true
            };
            PostMessageProperties = new PostMessageProperties()
            {
                ClosePostMessage = true,
            };


        }

        public static string HostEndpoint { get; set; }
        public static string MachineName { get; set; }
        public static string ServerVersion { get; set; }

        public static WopiHostCapabilities WopiHostCapabilities { get; private set; }
        public static WopiHostProperties WopiHostProperties { get; private set; }
        public static PostMessageProperties PostMessageProperties { get; private set; }
        public static BreadcrumbProperties BreadcrumbProperties { get; private set; }
        //public static WopiFileServiceEvents WopiFileServiceEvents { get; private set; }
    }
}
