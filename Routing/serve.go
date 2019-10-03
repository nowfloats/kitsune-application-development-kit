package main

import (
	"net/http"
	"log"
	"strings"
	"io/ioutil"
	"strconv"
	"os"
	"./routing"
	"fmt"
	"encoding/json"
	"github.com/joho/godotenv"
)

func matchHandler(w http.ResponseWriter, r *http.Request) {
	// TODO: Send Content-Type
	defer r.Body.Close()
	bodyBytes, err := ioutil.ReadAll(r.Body)
	bodyString  := string(bodyBytes)
	log.Printf("Received request for:\n%v\n ------", bodyString)

	if err == nil {
		request := strings.Split(bodyString, "\n")

		stage, err := strconv.Atoi(request[0])
		if err == nil {
			matchRequest := routing.RouteMatchRequest{
				Stage:    stage,
				PrefixId: request[1],
				Url:      request[2],
			}

			response := routing.MatchRouteFromCache(matchRequest)
			log.Printf("Response: %v", response)
			fmt.Fprintf(w, "%v\n%v\n%v\n%v\n%v\n%v", response.Status, response.ResourceId, response.File,
				response.RedirectPath,  response.IsStatic, response.Message)
		} else {
			http.Error(w, http.StatusText(http.StatusBadRequest), http.StatusBadRequest)
		}
	} else {
		http.Error(w, http.StatusText(http.StatusBadRequest), http.StatusBadRequest)
	}
}


func updateTreeHandler(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	bodyBytes, err := ioutil.ReadAll(r.Body)
	bodyString  := string(bodyBytes)
	log.Printf("Received request for:\n%v\n ------", bodyString)
	if err == nil {
		request := strings.Split(bodyString, "\n")

		stage, err := strconv.Atoi(request[0])
		var filter map[string]interface{}
		json.Unmarshal([]byte(request[3]), &filter)

		if err == nil {
			updateTreeRequest := routing.UpdateTreeRequest{
				Stage:    stage,
				PrefixId: request[1],
				CollectionName: request[2],
				Filter: filter,
			}

			response := routing.UpdateTreeToCache(updateTreeRequest)
			log.Printf("Response: %v", response)
			fmt.Fprintf(w, "%v\n%v", response.Status, response.Message)
		} else {
			http.Error(w, http.StatusText(http.StatusBadRequest), http.StatusBadRequest)
		}
	} else {
		http.Error(w, http.StatusText(http.StatusBadRequest), http.StatusBadRequest)
	}
}

func createAndMatchHandler(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	bodyBytes, err := ioutil.ReadAll(r.Body)
	bodyString  := string(bodyBytes)
	log.Printf("Received request for:\n%v\n ------", bodyString)
	if err == nil {
		request := strings.Split(bodyString, "\n")

		var filter map[string]interface{}
		json.Unmarshal([]byte(request[1]), &filter)

		if err == nil {
			createAndMatchRequest := routing.CreateAndMatchRequest{
				CollectionName: request[0],
				Filter: filter,
				Url: request[2],
			}

			response := routing.CreateAndMatch(createAndMatchRequest)
			log.Printf("Response: %v", response)
			fmt.Fprintf(w, "%v\n%v\n%v\n%v\n%v\n%v", response.Status, response.ResourceId, response.File,
				response.RedirectPath, response.IsStatic, response.Message)
		} else {
			http.Error(w, http.StatusText(http.StatusBadRequest), http.StatusBadRequest)
		}
	} else {
		http.Error(w, http.StatusText(http.StatusBadRequest), http.StatusBadRequest)
	}
}

func createJSON(w http.ResponseWriter, r *http.Request) {
	defer r.Body.Close()
	bodyBytes, err := ioutil.ReadAll(r.Body)
	bodyString  := string(bodyBytes)
	log.Printf("Received request for:\n%v\n ------", bodyString)
	if err == nil {
		request := strings.Split(bodyString, "\n")

		var filter map[string]interface{}
		json.Unmarshal([]byte(request[1]), &filter)

		if err == nil {
			createJSONRequest := routing.CreateJSONRequest{
				CollectionName: request[0],
				Filter: filter,
			}

			forest := routing.CreateJSON(createJSONRequest)
			w.Header().Set("Content-Type", "application/json")
			json.NewEncoder(w).Encode(forest)
		} else {
			http.Error(w, http.StatusText(http.StatusBadRequest), http.StatusBadRequest)
		}
	} else {
		http.Error(w, http.StatusText(http.StatusBadRequest), http.StatusBadRequest)
	}
}

func healthCheckHandler(w http.ResponseWriter, r *http.Request) {
	fmt.Fprintf(w, "OK")
}

func main() {
	godotenv.Load()

	if _, ok := os.LookupEnv("DISABLE_LOGGING"); ok {
		log.SetOutput(ioutil.Discard)
	}
	http.HandleFunc("/", healthCheckHandler)
	http.HandleFunc("/match", matchHandler)
	http.HandleFunc("/update", updateTreeHandler)
	http.HandleFunc("/createAndMatch", createAndMatchHandler)
	http.HandleFunc("/createJSON", createJSON)
	log.Println("Starting to listen at", os.Getenv("HTTP_LISTENER_ADDRESS"))
	log.Print(http.ListenAndServe(os.Getenv("HTTP_LISTENER_ADDRESS"), nil))
}
