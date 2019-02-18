package server

import (
	"log"
	"net/http"
	"net/url"

	"github.com/dolittle-entropy/NuGetFeedTreeServer/feed"
)

type metadataHandler struct {
}

func (s *metadataHandler) ServeHTTP(res http.ResponseWriter, req *http.Request) {
	log.Println("[INFO] Handling Metadata request")
	log.Println(req.URL.String())
}

func newMetadataHandler(base url.URL, feed feed.NuGetFeed) (http.Handler, error) {
	return &metadataHandler{}, nil
}
