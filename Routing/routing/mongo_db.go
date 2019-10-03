package routing

import (
	"os"
	"fmt"
	"context"
	"github.com/mongodb/mongo-go-driver/bson"
	"github.com/mongodb/mongo-go-driver/mongo"
	"github.com/pkg/errors"
		)

func GetFromMongoDB(collectionName string, filter interface{}) ([]ResourcesProduction, error) {
	var resources []ResourcesProduction
	//log.Printf("filter: %v", filter)

	//log.Printf("Connecting to: %v", os.Getenv("MONGO_DB_URI"))
	client, err := mongo.NewClient(os.Getenv("MONGO_DB_URI"))
	if err != nil { return nil, errors.New(fmt.Sprintf("Error in get database client due to: %v", err)) }

	err = client.Connect(context.TODO())
	if err != nil { return nil, errors.New(fmt.Sprintf("Error in connecting to DB due to: %v", err)) }

	collection := client.Database(os.Getenv("MONGO_DB_NAME")).Collection(collectionName)
	cursor, err := collection.Find(context.Background(), filter)
	if err != nil { return nil, errors.New(fmt.Sprintf("Error in getting records from DB due to: %v", err)) }
	defer cursor.Close(context.Background())

	for cursor.Next(context.Background()) {
		document := new(ResourcesProduction)
		elem, err := cursor.DecodeBytes()
		if err != nil { return nil, errors.New(fmt.Sprintf("Error in decoding a document due to: %v", err)) }
		err = bson.Unmarshal(elem, document)
		if err != nil { return nil, errors.New(fmt.Sprintf("Error in decoding a document due to: %v", err)) }
		//log.Printf("%v", document)
		resources = append(resources, *document)
	}
	//log.Printf("Got %v resources.", len(resources))

	if err := cursor.Err(); err != nil {
		return nil, errors.New(fmt.Sprintf("Error from cursor: %v", err))
	}

	return resources, nil
}