using System;

namespace WopiCore.Models
{
    public class BreadcrumbProperties : IBreadcrumbProperties
    {
        public string BreadcrumbBrandName { get; set; }
        public Uri BreadcrumbBrandUrl { get; set; }
        public string BreadcrumbDocName { get; set; }
        public string BreadcrumbFolderName { get; set; }
        public Uri BreadcrumbFolderUrl { get; set; }
        internal BreadcrumbProperties()
        {
        }
        internal BreadcrumbProperties Clone()
        {
            return (BreadcrumbProperties)MemberwiseClone();
        }
    }
}
