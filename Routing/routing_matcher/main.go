package main

import (
	"github.com/aws/aws-lambda-go/lambda"
	"../routing"
	"log"
	"io/ioutil"
	"os"
	"github.com/aws/aws-lambda-go/events"
	"strings"
	"strconv"
	"fmt"
)

func HandleRequest(r events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	bodyString  := string(r.Body)
	log.Printf("Received request for:\n%v\n ------", bodyString)

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
		responseBody := fmt.Sprintf("%v\n%v\n%v\n%v\n%v\n%v", response.Status, response.ResourceId, response.File,
			response.RedirectPath, response.IsStatic, response.Message)
		return events.APIGatewayProxyResponse{
			StatusCode:200,
			Body: responseBody,
			Headers: map[string]string{"Content-Type": "text/plain"},
		}, nil
	} else {
		return events.APIGatewayProxyResponse{StatusCode:400, Body: err.Error()}, nil
	}
}

func main() {
	if _, ok := os.LookupEnv("DISABLE_LOGGING"); ok {
		log.SetOutput(ioutil.Discard)
	}
	lambda.Start(HandleRequest)
}