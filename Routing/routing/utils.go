package routing

import (
	"strings"
	"time"
	"runtime"
	"regexp"
	"fmt"
	"log"
	"os"
)

func TrackTime(what string) func() {
	if value := os.Getenv("DISABLE_TIMING"); value == "" {
		start := time.Now()
		return func() {
			log.Printf("%s took %v\n", what, time.Since(start))
		}
	} else {
		return func() {}
	}
}

func TimeIt(start time.Time) {
	if value := os.Getenv("DISABLE_TIMING"); value == "" {
		elapsed := time.Since(start)
		pc, _, _, _ := runtime.Caller(1)
		funcObj := runtime.FuncForPC(pc)
		runtimeFunc := regexp.MustCompile(`^.*\.(.*)$`)
		name := runtimeFunc.ReplaceAllString(funcObj.Name(), "$1")
		log.Println(fmt.Sprintf("%s took %s", name, elapsed))
	} else {
		return
	}
}

func MaxInt(x int, y int) int {
	if x > y {
		return x
	}
	return y
}

func IndexWithMin(str, sub string, min int) int {
	return MaxInt(strings.Index(str, sub), min)
}

func RemoveLast(str string, sep string) string {
	s := strings.Split(str, sep)
	return strings.Join(s[:len(s)-1], sep)
}

func GetLast(str string, sep string) string {
	s := strings.Split(str, sep)
	return s[len(s)-1]
}

func stringInList(a string, list []string) bool {
	for _, b := range list {
		if b == a {
			return true
		}
	}
	return false
}

func stringSuffixInList(a string, list []string) bool {
	for _, b := range list {
		if strings.HasSuffix(a, b) {
			return true
		}
	}
	return false
}