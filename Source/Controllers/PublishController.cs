using IOFile = System.IO.File;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IO;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Common;
using NuGet.Configuration;
using System.Threading;
using NuGet.Packaging.Core;
using System;

namespace Server
{
    [Route("/v2/packages")]
    public class PublishController : ControllerBase
    {
        [HttpPut]
        public async Task<IActionResult> Publish(IFormFile package)
        {
            // Copy the package to a temporary file, and read package info
            try
            {
                PackageIdentity packageIdentity;
                string packageHash;
                string nuspecContent;

                var tmpPackagePath = Path.GetTempFileName();
                using (var tmpFile = IOFile.Open(tmpPackagePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    await package.CopyToAsync(tmpFile);
                    tmpFile.Seek(0, SeekOrigin.Begin);

                    var reader = new PackageArchiveReader(tmpFile, true);

                    packageIdentity = reader.GetIdentity();

                    var nuspecReader = new StreamReader(reader.GetNuspec());
                    nuspecContent = await nuspecReader.ReadToEndAsync();

                    tmpFile.Seek(0, SeekOrigin.Begin);
                    packageHash = Convert.ToBase64String(new CryptoHashProvider("SHA512").CalculateHash(tmpFile));
                }

                // Repository
                var localRepDir = Directory.GetCurrentDirectory()+"/packages/";
                var pathResolver = new VersionFolderPathResolver(localRepDir, true);

                // Check if package already exists
                if (IOFile.Exists(pathResolver.GetPackageFilePath(packageIdentity.Id, packageIdentity.Version)))
                {
                    Console.WriteLine($"[WARNING] Package already exists");
                    return Conflict();
                }

                // Create the package/version folder, and write files
                Directory.CreateDirectory(pathResolver.GetInstallPath(packageIdentity.Id, packageIdentity.Version));
            
                IOFile.Move(tmpPackagePath, pathResolver.GetPackageFilePath(packageIdentity.Id, packageIdentity.Version));

                using (var hashFile = IOFile.CreateText(pathResolver.GetHashPath(packageIdentity.Id, packageIdentity.Version)))
                {
                    await hashFile.WriteAsync(packageHash);
                }

                using (var nuspecFile = IOFile.CreateText(pathResolver.GetManifestFilePath(packageIdentity.Id, packageIdentity.Version)))
                {
                    await nuspecFile.WriteAsync(nuspecContent);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Processing package. {ex.Message}");
                return BadRequest();
            }
        }
    }
}