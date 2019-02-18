package server

import (
	"archive/zip"
	"encoding/xml"
	"io"
	"log"
	"net/http"
	"net/url"
	"os"
	"strings"

	"github.com/dolittle-entropy/NuGetFeedTreeServer/feed"
)

/*
   <dependencies>
     <group targetFramework=".NETStandard2.0">
       <dependency id="Dolittle.Assemblies" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Dolittle.Collections" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Dolittle.DependencyInversion" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Dolittle.Execution" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Dolittle.IO" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Dolittle.Logging" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Dolittle.Scheduling" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Dolittle.Time" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Dolittle.Types" version="0.0.3" exclude="Build,Analyzers" />
       <dependency id="Microsoft.Extensions.Logging" version="2.2.0" exclude="Build,Analyzers" />
     </group>
   </dependencies>
*/

type nugetPackageType struct {
	Name    string `xml:"name,attr"`
	Version string `xml:"version,attr"`
}

type nugetDependency struct {
	ID      string `xml:"id,attr"`
	Version string `xml:"version,attr"`
	Include string `xml:"include,attr"`
	Exclude string `xml:"exclude,attr"`
}

type nugetDependencyGroup struct {
	Dependenies []nugetDependency `xml:"dependency"`
}

type nugetDependencies struct {
	Dependenies []nugetDependency      `xml:"dependency"`
	Groups      []nugetDependencyGroup `xml:"group"`
}

type nugetFrameworkAssembly struct {
	AssemblyName    string `xml:"assemblyName,attr"`
	TargetFramework string `xml:"targetFramework,attr"`
}

type nugetReferenceGroup struct {
	TargetFramework string   `xml:"targetFramework,attr"`
	References      []string `xml:"reference>file"`
}

type nugetReferences struct {
	References []string              `xml:"reference>file"`
	Groups     []nugetReferenceGroup `xml:"group"`
}

type nuspecPackage struct {
	ID          string `xml:"metadata>id"`
	Version     string `xml:"metadata>version"`
	Description string `xml:"metadata>description"`
	Authors     string `xml:"metadata>authors"`
	Title       string `xml:"metadata>title"`
	Owners      string `xml:"metadata>owners"`
	ProjectURL  string `xml:"metadata>projectUrl"`
	LicenseURL  string `xml:"metadata>licenseUrl"`
	License     string `xml:"metadata>license"`
	//License type
	IconURL                  string                   `xml:"metadata>iconUrl"`
	RequireLicenseAcceptance bool                     `xml:"metadata>requireLicenseAcceptance"`
	DevelopmentDependency    bool                     `xml:"metadata>developmentDependency"`
	Summary                  string                   `xml:"metadata>summary"`
	ReleaseNotes             string                   `xml:"metadata>releaseNotes"`
	Copyright                string                   `xml:"metadata>copyright"`
	Language                 string                   `xml:"metadata>language"`
	Tags                     string                   `xml:"metadata>tags"`
	MinClientVersion         string                   `xml:"metadata>minClientVersion,attr"`
	Repository               string                   `xml:"metadata>repository"`
	PackageTypes             []nugetPackageType       `xml:"metadata>packageTypes"`
	Dependencies             nugetDependencies        `xml:"metadata>dependencies"`
	FrameworkAssemblies      []nugetFrameworkAssembly `xml:"metadata>frameworkAssemblies"`
	References               nugetReferences          `xml:"metadata>references"`
}

type publishHandler struct {
}

func (s *publishHandler) ServeHTTP(res http.ResponseWriter, req *http.Request) {
	log.Println("[INFO] Handling Publish request")

	if reader, err := req.MultipartReader(); err == nil {
		if part, err := reader.NextPart(); err == nil {
			if file, err := os.Create(part.FileName()); err == nil {
				log.Println("[INFO] Part name", part.FileName())
				if length, err := io.Copy(file, part); err != nil {
					log.Println("[ERROR] Wrtiting file ", err)
				} else {
					_, err = file.Seek(0, 0)
					if err != nil {
						log.Println("[ERROR] Wrtiting file ", err)
					}
					if archreader, err := zip.NewReader(file, length); err != nil {
						log.Println("[ERROR] Looking in archive.", err)
					} else {
						for _, file := range archreader.File {
							if strings.HasSuffix(file.Name, ".nuspec") {
								log.Println("[INFO] File", file.Name)
								if freader, err := file.Open(); err == nil {
									decoder := xml.NewDecoder(freader)
									pkg := nuspecPackage{}
									if err = decoder.Decode(&pkg); err == nil {
										log.Println("[INFO] Package", pkg)
									} else {
										log.Println("[ERROR] Decoding package.", err)
									}
								}
							}
						}
					}
				}
			}
		}
	}
}

func newPublishHandler(base url.URL, feed feed.NuGetFeed) (http.Handler, error) {
	return &publishHandler{}, nil
}
