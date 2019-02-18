package server

import (
	"encoding/json"
	"log"
	"net/http"
	"net/url"

	"github.com/dolittle-entropy/NuGetFeedTreeServer/feed"
	"github.com/gorilla/mux"
)

type serviceResource struct {
	ID   string `json:"@id"`
	Type string `json:"@type"`
}

type serviceIndex struct {
	Version    string            `json:"version"`
	Resources  []serviceResource `json:"resources"`
	marshalled []byte
}

func (si *serviceIndex) ServeHTTP(res http.ResponseWriter, _ *http.Request) {
	log.Println("[INFO] Handling ServiceIndex request")
	res.Write(si.marshalled)
}

func newServiceIndex(base url.URL) (http.Handler, error) {
	index := serviceIndex{
		Version: "3.0.0",
		Resources: []serviceResource{
			serviceResource{
				Type: "SearchQueryService",
				ID:   base.String() + searchPath,
			},
			serviceResource{
				Type: "PackagePublish/2.0.0",
				ID:   base.String() + publishPath,
			},
			serviceResource{
				Type: "RegistrationsBaseUrl/3.4.0",
				ID:   base.String() + metadataPath,
			},
			serviceResource{
				Type: "PackageBaseAddress/3.0.0",
				ID:   base.String() + contentPath,
			},
		},
	}
	if marshalled, err := json.Marshal(index); err != nil {
		log.Println("[ERROR] Marshalling index.", err)
		return nil, err
	} else {
		index.marshalled = marshalled
		return &index, nil
	}
}

func AddNuget(router *mux.Router, base url.URL, feed feed.NuGetFeed) error {
	if index, err := newServiceIndex(base); err == nil {
		router.Handle(base.Path+"index.json", index)
	} else {
		return err
	}
	if search, err := newSearchHandler(base, feed); err == nil {
		router.Handle(base.Path+searchPath, search)
	} else {
		return err
	}
	if publish, err := newPublishHandler(base, feed); err == nil {
		router.Path(base.Path + publishPath).Methods("PUT").Handler(publish)
		router.Path(base.Path + publishPath + "/").Methods("PUT").Handler(publish)
		//router.PathPrefix(base.Path + publishPath).Handler(publish)
		//router.Handle(base.Path+publishPath, publish)
	} else {
		return err
	}
	if metadata, err := newMetadataHandler(base, feed); err == nil {
		router.PathPrefix(base.Path + metadataPath).Handler(metadata)
		//router.Handle(base.Path+metadataPath, metadata)
	} else {
		return err
	}
	if content, err := newContentHandler(base, feed); err == nil {
		router.Handle(base.Path+contentPath, content)
	} else {
		return err
	}
	log.Printf("[INFO] Registered feed at %s\n", base.String())
	return nil
}
