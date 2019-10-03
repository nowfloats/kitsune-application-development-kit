package main

import (
	"github.com/aws/aws-lambda-go/lambda"
	"../routing"
	"strings"
	"strconv"
	"encoding/json"
	"fmt"
	"github.com/aws/aws-lambda-go/events"
	"log"
)

func HandleRequest(r events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	bodyString  := string(r.Body)
	log.Printf("Received request for:\n%v\n ------", bodyString)
	request := strings.Split(bodyString, "\n")

	stage, err := strconv.Atoi(request[0])

	if err == nil {
		var filter map[string]interface{}
		json.Unmarshal([]byte(request[3]), &filter)

		updateTreeRequest := routing.UpdateTreeRequest{
			Stage:    stage,
			PrefixId: request[1],
			CollectionName: request[2],
			Filter: filter,
		}

		response := routing.UpdateTreeToCache(updateTreeRequest)
		log.Printf("Response: %v", response)
		responseBody := fmt.Sprintf("%v\n%v", response.Status, response.Message)
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
	lambda.Start(HandleRequest)
}