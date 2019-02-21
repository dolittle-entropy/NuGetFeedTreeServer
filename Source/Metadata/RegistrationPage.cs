using System.Collections.Generic;
using NuGet.Versioning;

namespace Server.Metadata
{
    public class RegistrationPage
    {
        public string ID { get; set; }
        public string Parent { get; set; }
        public int Count { get; set; }
        public IEnumerable<RegistrationLeaf> Items { get; set; }
        public NuGetVersion Lower { get; set; }
        public NuGetVersion Upper { get; set; }
    }
}