package routing

import (
	"os"
	"time"
	"fmt"
	"encoding/csv"
	"bufio"
	"io"
	"strings"
	"log"
	"github.com/mongodb/mongo-go-driver/bson"
	"github.com/mongodb/mongo-go-driver/bson/objectid"
	"encoding/json"
	"strconv"
	"github.com/golang/protobuf/proto"
)

type ResourcesProduction struct {
	ResourceId      objectid.ObjectID `bson:"_id"`
	FilePath        string `bson:"OptimizedPath"`
	UrlPatternRegex string `bson:"UrlPatternRegex"`
	PageType        string `bson:"PageType"`
	IsDefault		bool `bson:"IsDefault"`
	IsStatic		bool `bson:"IsStatic"`
}

type RouteMatchRequest struct {
	PrefixId string `json:"prefix_id"`
	Url      string `json:"url"`
	Stage    int    `json:"stage"`
	PingBack bool   `json:"ping_back"`
}

type RouteMatchResponse struct {
	Status			int		`json:"status"`
	Message			string	`json:"message"`
	File			string	`json:"file"`
	ResourceId		string	`json:"resource_id"`
	RedirectPath	string	`json:"redirect_path"`
	IsStatic		string 	`json:"is_static"`
}

type UpdateTreeRequest struct {
	PrefixId string					`json:"prefix_id"`
	Stage    int					`json:"stage"`
	CollectionName string			`json:"collection_name"`
	Filter map[string]interface{}	`json:"filter"`
}

type UpdateTreeResponse struct {
	Status	int		`json:"status"`
	Message	string	`json:"message"`
}

type CreateAndMatchRequest struct {
	CollectionName	string					`json:"collection_name"`
	Filter			map[string]interface{}	`json:"filter"`
	Url				string					`json:"string"`
}

type CreateJSONRequest struct {
	CollectionName string					`json:"collection_name"`
	Filter 			map[string]interface{}	`json:"filter"`
}

func ReadFromFile(filename string) ([]Route) {
	f, _ := os.Open(filename)
	r := csv.NewReader(bufio.NewReader(f))
	_, _ = r.Read() // Remove header
	var records []Route
	for {
		record, err := r.Read()
		if err == io.EOF {
			break
		}
		route := Route{Pattern:record[1], Filepath: record[0], Filetype:record[2]}
		if route.Pattern == "" {
			route.Pattern = route.Filepath
		}

		// Escape hyphen
		if strings.Contains(route.Pattern, StringRegexpStart) {
			route.Pattern = strings.Replace(route.Pattern, "\\-", "\\\\-", -1)
		}

		StringRegexpFullEscapedForFile := "([a-zA-Z0-9\\\\\\-\\\\.\\\\%]+)"

		if route.Filetype == "LIST" || route.Filetype == "SEARCH" {
			// Only if the entire last segment is a regex
			if GetLast(route.Pattern, "/") == StringRegexpFullEscapedForFile {
				routeWithOptional :=
					Route {
						Filetype: route.Filetype,
						Filepath: route.Filepath,
						Pattern:  RemoveLast(route.Pattern, "/"),
					}

				// Try to make last segment optional
				if routeWithOptional.Pattern != "" {
					records = append(records, routeWithOptional)
				}
			}

		}
		records = append(records, route)
	}
	return records
}

func ReadFromDatabase(resourceCollectionName string, filter interface{}) ([]Route, string) {
	var records []Route
	var resources []ResourcesProduction
	resources, err := GetFromMongoDB(resourceCollectionName, filter)
	if err != nil { return records, fmt.Sprintf("Database error: %v", err) }

	for _, resource := range resources {
		route := Route{
			ResourceId: resource.ResourceId.Hex(),
			Filepath: resource.FilePath,
			Pattern:  resource.UrlPatternRegex,
			Filetype: resource.PageType,
			IsStatic: resource.IsStatic,
		}

		if route.Pattern == "" {
			route.Pattern = route.Filepath
		}

		//// Escape hyphen
		//if strings.Contains(Route.Pattern, StringRegexp) {
		//	Route.Pattern = strings.Replace(Route.Pattern, "\\-", "\\\\-", -1)
		//}

		records = append(records, route)

		if route.Filetype == "LIST" || route.Filetype == "SEARCH" {
			// Only if the entire last segment is a regex
			lastSegment := GetLast(route.Pattern, "/")
			if strings.HasPrefix(lastSegment, StringRegexpStart) && strings.HasSuffix(lastSegment, StringRegexpEnd) {
				routeWithOptional :=
					Route {
						ResourceId: route.ResourceId,
						Filetype: route.Filetype,
						Filepath: route.Filepath,
						Pattern:  RemoveLast(route.Pattern, "/"),
						IsStatic: route.IsStatic,
					}

				// Try to make last segment optional
				if routeWithOptional.Pattern != "" {
					records = append(records, routeWithOptional)
				}
			}
		}

		if resource.IsDefault == true {
			records = append(records,
				Route{
				ResourceId: route.ResourceId,
				Filetype: route.Filetype,
				Filepath: route.Filepath,
				Pattern:  "/",
				IsStatic: route.IsStatic,
			})
		}
	}
	return records, ""
}

func ForestToCache(prefix string, collectionName string, filter interface{}) (err string) {
	defer func() {
		if r := recover(); r != nil {
			err = r.(string)
		}
	}()

	records, err := ReadFromDatabase(collectionName, filter)

	if err != "" {
		return fmt.Sprintf("Error in getting records due to: %v", err)
	} else if len(records) == 0 {
		log.Println("No records found with the filter")
		return fmt.Sprintf("No records found for PrefixId: %v in Collection %s, Filter: %v" , prefix, collectionName, filter)
	}

	forest := MakeForest(records)
	if forest == nil { return "Couldn't make forest from records" }

	log.Printf("Done making Forest")
	err = MemorizeForest(forest, prefix)
	return err
}

func MatchRouteFromCache(request RouteMatchRequest) RouteMatchResponse {
	defer TimeIt(time.Now())
	prefix := fmt.Sprintf("%s-%d", request.PrefixId, request.Stage)
	matchResult, err := MatchWithCache(prefix, request.Url)
	if matchResult.Matched {
		return RouteMatchResponse{
			200,
			"OK",
			matchResult.Result.Route.Filepath,
			matchResult.Result.Route.ResourceId,
			matchResult.RedirectPath,
			strconv.FormatBool(matchResult.Result.Route.IsStatic),
		}
	} else if err == "" {
		return RouteMatchResponse{404, "Not Found", "", "", "", ""}
	}
	return RouteMatchResponse{500, string(err), "", "", "", ""}
}

func UpdateTreeToCache(request UpdateTreeRequest) UpdateTreeResponse {
	log.Printf("Got update request: %v", request)
	prefix := fmt.Sprintf("%s-%d", request.PrefixId, request.Stage)
	jsonFilter, _ := json.Marshal(request.Filter)
	filter, _ := bson.ParseExtJSONObject(string(jsonFilter))
	log.Printf("Filter Query: %v", filter)

	err := ForestToCache(prefix, request.CollectionName, filter)
	if err == "" {
		return UpdateTreeResponse{200, fmt.Sprintf("Updated tree for ... %v", request.PrefixId)}
	} else {
		return UpdateTreeResponse{500, fmt.Sprintf("Error in Updating Tree due to %v", err)}
	}
}

func CreateAndMatch(request CreateAndMatchRequest) (matchResponse RouteMatchResponse) {
	defer func() {
		if r := recover(); r != nil {
			matchResponse.Status = 500
			matchResponse.Message = r.(string)
		}
	}()

	jsonFilter, _ := json.Marshal(request.Filter)
	filter, _ := bson.ParseExtJSONObject(string(jsonFilter))
	log.Printf("Filter Query: %v", filter)

	records, err := ReadFromDatabase(request.CollectionName, filter)
	if err != "" {
		return RouteMatchResponse{
			Status: 500,
			Message: fmt.Sprintf("Couldn't get records from Database due to: %v", err),
		}
	}
	forest := MakeForest(records)
	matchResult, err := MatchWithForest(forest, request.Url)
	if matchResult.Matched {
		return RouteMatchResponse{
			200,
			"OK",
			matchResult.Result.Route.Filepath,
			matchResult.Result.Route.ResourceId,
			matchResult.RedirectPath,
			strconv.FormatBool(matchResult.Result.Route.IsStatic),
		}
	} else if err == "" {
		return RouteMatchResponse{404, "Not Found", "", "", "", ""}
	}
	return RouteMatchResponse{500, string(err), "", "", "",""}
}

func CreateJSON(request CreateJSONRequest) map[int][]byte {
	defer func() {
		if r := recover(); r != nil {
			log.Printf("Unable to create JSON due to: %v", r.(string))
		}
	}()

	jsonFilter, _ := json.Marshal(request.Filter)
	filter, _ := bson.ParseExtJSONObject(string(jsonFilter))
	log.Printf("Filter Query: %v", filter)

	records, err := ReadFromDatabase(request.CollectionName, filter)
	if err != "" {
		log.Panicf("Error in reading from database due to: %v", err)
		return nil
	}
	forest := MakeForest(records)

	response := make(map[int][]byte)

	for k, tree := range forest {
		response[k], _ = proto.Marshal(tree)
	}

	return response
}

//func createAndMatchRoute(rm RouteMatchRequest) RouteMatchResponse {
//	//matchResult, err = CreateAndMatch(rm.PrefixId, rm.Url)
//}