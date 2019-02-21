using System.Collections.Generic;
using NuGet.Packaging;
using NuGet.Versioning;

namespace Server.Metadata
{
    public class CatalogEntry
    {
        public string ID { get; set; }
        public string Authors { get; set; }
        public IEnumerable<PackageDependencyGroup> DependencyGroups { get; set; }
        public string Description { get; set; }
        public string IconURL { get; set; }
        public string PackageID { get; set; }
        public string LicenseURL { get; set; }
        public string LicenseExpressions { get; set; }
        public bool Listed { get; set; }
        public string MinClientVersion { get; set; }
        public string ProjectURL { get; set; }
        public string Published { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Summary { get; set; }
        public string Tags { get; set; }
        public string Title { get; set; }
        public NuGetVersion Version { get; set; }
    }
}