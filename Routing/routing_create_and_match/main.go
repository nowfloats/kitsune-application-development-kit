package main

import (
	"github.com/aws/aws-lambda-go/lambda"
	"../routing"
	"log"
	"strings"
	"encoding/json"
	"fmt"
	"github.com/aws/aws-lambda-go/events"
	"os"
	"io/ioutil"
)

func HandleRequest(r events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	bodyString  := string(r.Body)
	log.Printf("Received request for:\n%v\n ------", bodyString)
	request := strings.Split(bodyString, "\n")

	var filter map[string]interface{}
	json.Unmarshal([]byte(request[1]), &filter)

	createAndMatchRequest := routing.CreateAndMatchRequest{
		CollectionName: request[0],
		Filter: filter,
		Url: request[2],
	}

	response := routing.CreateAndMatch(createAndMatchRequest)
	log.Printf("Response: %v", response)

	responseBody := fmt.Sprintf("%v\n%v\n%v\n%v\n%v\n%v", response.Status, response.ResourceId, response.File,
		response.RedirectPath, response.IsStatic, response.Message)

	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Body: responseBody,
		Headers: map[string]string{"Content-Type": "text/plain"},
	}, nil
}

func main() {
	if _, ok := os.LookupEnv("DISABLE_LOGGING"); ok {
		log.SetOutput(ioutil.Discard)
	}
	lambda.Start(HandleRequest)
}