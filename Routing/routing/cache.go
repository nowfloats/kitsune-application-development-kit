package routing

import (
	"strconv"
	"github.com/golang/protobuf/proto"
	"github.com/mediocregopher/radix.v2/cluster"
	"time"
	"os"
	"log"
	"fmt"
)

var redisAddress = os.Getenv("REDIS_ADDRESS")

func MemorizeForest(forest map[int]*Node, prefix string) (e string) {
	c, err := cluster.New(redisAddress)
	if err != nil {
		log.Println("Unable to connect to redis using " + redisAddress)
		return err.Error()
	}
	log.Println("Connected to Redis")
	defer c.Close()
	defer TimeIt(time.Now())

	for k, tree := range forest {
		key := prefix + ":" + strconv.Itoa(k)
		//fmt.Printf("%v = %v\n", key, tree)
		serialized, err := proto.Marshal(tree)
		if err != nil {
			log.Printf("Unable to parse redis value due to: %v", err.Error())
			return err.Error()
		}

		err = c.Cmd("SET", key, serialized).Err
		if err != nil {
			log.Println("Unable to SET Key: " + key + " to redis")
			return err.Error()
		}
		//fmt.Printf("%v = %T | %v\n", key, serialized,  serialized)
	}

	return ""
}

func RecallTree(prefix string, segment int) (*Node) {
	c, err := cluster.New(redisAddress)
	defer c.Close()
	//defer TimeIt(time.Now())
	if err == nil {
		key := fmt.Sprintf("%s:%d", prefix, segment)
		//fmt.Println("\nSearching for: " + key)

		value, err := c.Cmd("GET", key).Bytes()

		if err == nil && value != nil {
			tree := &Node{}
			defer TrackTime("Routing Tree Deserialization Time")()
			proto.Unmarshal(value, tree)
			return tree
		} else {
			log.Println("Unable to GET Key: "+ key + " on redis")
		}
	} else {
		log.Println("Unable to connect to redis using " + redisAddress)
	}

	return nil
}

func TreeCount(prefix string) (int, string) {
	c, err := cluster.New(redisAddress)
	defer c.Close()
	defer TimeIt(time.Now())
	if err == nil {
		keyPrefix := fmt.Sprintf("*%s*", prefix)

		value, err := c.Cmd("KEYS", keyPrefix).List()
		if err == nil {
			return len(value), ""
		}

		return -1, fmt.Sprintf("Unable to Count KEYS for prefix: %s on redis", keyPrefix)
	}
	return -1, fmt.Sprintf("Unable to connect to redis using %s", redisAddress)
}

//func main()  {
//	projectId := "5a4ced817a25030518487e05"
//	//forest := MakeForest(ReadFromDatabase(projectId))
//	//MemorizeForest(forest, projectId)
//	result, matched := MatchWithCache(projectId, "/index.html")
//	fmt.Printf("%v %v\n", matched, result)
//}