using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Protocol;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Linq;
using System.Collections.Generic;
using NuGet.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using NuGet.Packaging;
using System.Xml.Linq;

namespace Server
{
    [Route("/v3/registrations")]
    public class RegistrationsController : ControllerBase
    {
        private static SourceCacheContext caching = new SourceCacheContext();

        [HttpGet("{lowerId}/index.json")]
        public async Task<ActionResult<RegistrationIndex>> GetRegistrations(string lowerId)
        {
             // Find all local packages
            var localRepDir = Directory.GetCurrentDirectory()+"/packages/";

            var packages = LocalFolderUtility.GetPackagesV3(localRepDir, lowerId, new CustomLogger()).ToList();

            if (packages.Count < 1)
            {
                var officialNuget = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json", FeedType.HttpV3);
                var metadataResource = officialNuget.GetResource<PackageMetadataResource>();
                var packageMetadatas = await metadataResource.GetMetadataAsync(lowerId, true, true, caching, new CustomLogger(), CancellationToken.None);

                packages = packageMetadatas.Select(_ => new LocalPackageInfo(
                    _.Identity,
                    "PATH",
                    new Lazy<NuspecReader>(() => new NuspecReader())
                ));

                foreach (var packageData in packageMetadata)
                {
                    System.Console.WriteLine($"PACKAGE {packageData.Identity}");
                }

                return NotFound();
            }

            NuGetVersion lowerVersion = null;
            NuGetVersion upperVersion = null;

            var currentFullUrl = HttpContext.Request.GetDisplayUrl();
            var currentPackageBaseUrl = currentFullUrl.Replace("/index.json","/");
            var contentUrl = currentFullUrl.Substring(0,currentFullUrl.IndexOf("/v3/registrations"))+"/v3/flatcontainer/";

            var pageItems = packages.Select(package => {
                lowerVersion = lowerVersion == null || lowerVersion > package.Identity.Version ? package.Identity.Version : lowerVersion;
                upperVersion = upperVersion == null || upperVersion < package.Identity.Version ? package.Identity.Version : upperVersion;

                var packageContentUrl = $"{contentUrl}{package.Identity.Id.ToLowerInvariant()}/{package.Identity.Version.ToNormalizedString()}/{package.Identity.ToString().ToLowerInvariant()}.nupkg";

                return new RegistrationIndexPageItem(
                    $"{currentPackageBaseUrl}{package.Identity.Version.ToNormalizedString()}.json",
                    new PackageMetadata(
                        "",
                        package.Nuspec.GetId(),
                        package.Nuspec.GetVersion(),
                        package.Nuspec.GetAuthors(),
                        package.Nuspec.GetDescription(),
                        0,
                        false,
                        package.Nuspec.GetIconUrl(),
                        package.Nuspec.GetLanguage(),
                        package.Nuspec.GetLicenseUrl(),
                        true,
                        package.Nuspec.GetMinClientVersion()?.ToNormalizedString() ?? "",
                        packageContentUrl,
                        package.Nuspec.GetProjectUrl(),
                        package.Nuspec.GetRepositoryMetadata().Url,
                        package.Nuspec.GetRepositoryMetadata().Type,
                        package.LastWriteTimeUtc,
                        package.Nuspec.GetRequireLicenseAcceptance(),
                        package.Nuspec.GetSummary(),
                        package.Nuspec.GetTags().Split(' '),
                        package.Nuspec.GetTitle(),
                        package.Nuspec.GetDependencyGroups().Select(group => new DependencyGroupItem(
                            $"",
                            group.TargetFramework.DotNetFrameworkName,
                            group.Packages.Select(dependency => new DependencyItem(
                                $"",
                                dependency.Id,
                                dependency.VersionRange.ToString()
                            )).ToList()
                        )).ToList()
                    ),
                    packageContentUrl
                );
            }).ToList();

            return new RegistrationIndex(1, 0, new List<RegistrationIndexPage> {
                new RegistrationIndexPage(
                    $"{HttpContext.Request.GetDisplayUrl()}#page",
                    pageItems.Count,
                    pageItems,
                    lowerVersion,
                    upperVersion
                )
            });
        }

        class NuspecReaderMock : NuspecReader
        {
            readonly PackageSearchMetadata _data;
            public NuspecReaderMock(PackageSearchMetadata data) : base(new XDocument())
            {
                _data = data;
            }
        }
    }
}