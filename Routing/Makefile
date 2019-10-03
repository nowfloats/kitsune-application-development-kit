ensure:
	go get github.com/aws/aws-sdk-go
	go get github.com/aws/aws-lambda-go/lambda
	go get github.com/mongodb/mongo-go-driver/mongo
	go get github.com/golang/protobuf/proto
	go get github.com/pkg/errors
	
build:
	make ensure
	CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -ldflags="-s -w" -o bin/routing_server *.go

build-all:
	make ensure
	CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -ldflags="-s -w" -o bin/routing routing/*.go
	env CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -ldflags="-s -w" -o bin/create_and_match routing_create_and_match/*.go
	env CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -ldflags="-s -w" -o bin/create_json routing_create_json/*.go
	env CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -ldflags="-s -w" -o bin/matcher routing_matcher/*.go
	env CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -ldflags="-s -w" -o bin/update_tree routing_update_tree/*.go

msbuild:
	make ensure
	env CGO_ENABLED=0 GOOS=windows GOARCH=amd64 go build -a -installsuffix cgo -ldflags="-s -w" -o bin/routing.exe routing/*.go

test:
	go test ./routing/...

image:
	make test
	docker build -t kitsune_routing .

ping:
	sls invoke -f matcher --data '{"ping_back": true}'

update:
	sls invoke -f update-tree --path events/update_tree.txt --log

match:
	sls invoke -f matcher --path events/request_match.txt --log

create-and-match:
	sls invoke -f create-and-match --path events/create_and_match.txt --log

deploy-dev:
	make build-all
	make test
	serverless deploy --stage dev

deploy-prod:
	make build-all
	make test
	serverless deploy --stage prod