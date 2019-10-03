FROM golang:1.9 as builder
WORKDIR /go/src/code.kitsune.tools/routing/

RUN go get github.com/pkg/errors

COPY Makefile /go/src/code.kitsune.tools/routing/
RUN make ensure

COPY . /go/src/code.kitsune.tools/routing/
RUN env CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -ldflags="-s -w" -o bin/routing_server *.go

FROM scratch
WORKDIR /root/
COPY --from=builder /go/src/code.kitsune.tools/routing/bin/routing_server /
CMD ["/routing_server"]
