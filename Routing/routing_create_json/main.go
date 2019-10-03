package main

import (
	"github.com/aws/aws-lambda-go/lambda"
	"github.com/aws/aws-lambda-go/events"
	"encoding/json"
	"../routing"
	"io/ioutil"
	"strings"
	"os"
	"log"
	"fmt"
	"encoding/base64"
)

func HandleRequest(r events.APIGatewayProxyRequest) (events.APIGatewayProxyResponse, error) {
	bodyString  := string(r.Body)
	log.Printf("Received request for:\n%v\n ------", bodyString)
	request := strings.Split(bodyString, "\n")

	var filter map[string]interface{}
	json.Unmarshal([]byte(request[1]), &filter)

	createJSONRequest := routing.CreateJSONRequest{
		CollectionName: request[0],
		Filter: filter,
	}

	forest := routing.CreateJSON(createJSONRequest)
	var responseText []string

	for k, tree := range forest {
		responseText = append(responseText, fmt.Sprintf("\"%v\": \"%v\"", k, base64.StdEncoding.EncodeToString(tree)))
	}

	return events.APIGatewayProxyResponse{
		StatusCode: 200,
		Body: fmt.Sprintf("{%v}", strings.Join(responseText, ",")),
		Headers: map[string]string{"Content-Type": "application/json"},
	}, nil
}

func main() {
	if _, ok := os.LookupEnv("DISABLE_LOGGING"); ok {
		log.SetOutput(ioutil.Discard)
	}
	lambda.Start(HandleRequest)
}