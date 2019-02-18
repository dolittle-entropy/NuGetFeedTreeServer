package main

import (
	"log"
	"net/http"
	"net/url"

	"github.com/dolittle-entropy/NuGetFeedTreeServer/server"

	"github.com/gorilla/mux"
)

func main() {
	router := mux.NewRouter()

	base := url.URL{
		Scheme: "http",
		Host:   "localhost:8080",
		Path:   "/",
	}
	server.AddNuget(router, base, nil)

	httpServer := &http.Server{
		Handler: router,
		Addr:    base.Host,
		// TODO: Add timeouts
	}
	log.Fatal(httpServer.ListenAndServe())
}
