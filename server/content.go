package server

import (
	"log"
	"net/http"
	"net/url"

	"github.com/dolittle-entropy/NuGetFeedTreeServer/feed"
)

type contentHandler struct {
}

func (s *contentHandler) ServeHTTP(res http.ResponseWriter, req *http.Request) {
	log.Println("[INFO] Handling Content request")
	log.Println(req.URL.String())
}

func newContentHandler(base url.URL, feed feed.NuGetFeed) (http.Handler, error) {
	return &contentHandler{}, nil
}
