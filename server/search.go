package server

import (
	"log"
	"net/http"
	"net/url"

	"github.com/dolittle-entropy/NuGetFeedTreeServer/feed"
)

type searchHandler struct {
}

func (s *searchHandler) ServeHTTP(res http.ResponseWriter, req *http.Request) {
	log.Println("[INFO] Handling Search request")
	log.Println(req.URL.String())
}

func newSearchHandler(base url.URL, feed feed.NuGetFeed) (http.Handler, error) {
	return &searchHandler{}, nil
}
