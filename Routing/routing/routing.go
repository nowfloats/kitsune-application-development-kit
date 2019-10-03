package routing

import (
	"strings"
	"fmt"
	"log"
)

const StringRegexpStart = "([a-z"
const StringRegexpEnd = "]+)"
const stringRegexpLen = 24

type MatchResponse struct {
	Result *Node
	Matched bool
	RedirectPath string
}

func (r *Route) IsCatchAllRoute() bool {
	return strings.Count(r.Pattern, StringRegexpStart) == strings.Count(r.Pattern, "/")
}

type Forest interface {
	GetTree(int) *Node
	CountTrees() (int, string)
}

type ForestInMemory struct {
	forest map[int] *Node
}

func (f ForestInMemory) GetTree(segmentCount int) *Node {
	return f.forest[segmentCount]
}

func (f ForestInMemory) CountTrees() (int, string) {
	return len(f.forest), ""
}

type ForestInCache struct {
	prefix string
}

func (f ForestInCache) GetTree(segmentCount int) *Node {
	return RecallTree(f.prefix, segmentCount)
}

func (f ForestInCache) CountTrees() (int, string) {
	return TreeCount(fmt.Sprintf("%s:", f.prefix))
}

const indexDotHtml = "/index.html"

// Return a score to comparator function to ensure following order of routes:
//
// /aa/-/zz   More literals first
// /aa/-/z	  Only literals next
// /aa/-/zz*  Descending order by Start Index of Regex
// /aa/-/z*
// /aa/-/*zz  Descending order by Index from Last of Regex
// /aa/-/*z
// /aa/-/*
// /aa*/-/zz* Descending order by Start Index of Regex weighted by Segment Number
// /aa*/-/z*
// /aa*/-/*zz
// /aa*/-/*z
// /aa*/-/*
func WeightedSegmentScoreFromStart(pattern string) int {
	score := 0
	segments := strings.Split(pattern, "/")[1:]
	totalSegments := len(segments)
	for i, segment := range segments {
		indexFromStart := strings.Index(segment, StringRegexpStart)
		if indexFromStart == -1 { // only non-complex (just literals) segments
			literalLength := len(segment) + stringRegexpLen + 1
			score += (totalSegments - i) * literalLength
		} else { // complex (with pattern) segments
			score += (totalSegments - i) * indexFromStart
		}
	}
	return score
}

func WeightedSegmentScoreFromLast(pattern string) int {
	score := 0
	segments := strings.Split(pattern, "/")
	totalSegments := len(segments)
	for i, segment := range segments {
		lastIndex := strings.LastIndex(segment, StringRegexpEnd)
		if lastIndex == -1 {
			literalLength := len(segment) + stringRegexpLen + 1
			score += (totalSegments - i) * literalLength
		} else {
			indexFromLast := len(segment) - lastIndex + len(StringRegexpEnd) - 1
			score += (totalSegments - i) * indexFromLast
		}
	}
	return score
}

func CleanseRoute(routePattern string) string {
	defer func() {
		if r := recover(); r != nil {
			if len(routePattern) == 0 {
				panic("Route pattern is Blank")
			} else {
				log.Panicf("Failed cleaning route: %v due to %v", routePattern, r.(string))
			}
		}
	}()

	if routePattern[0] == 32 || routePattern[len(routePattern)-1] == 32 {  // 32 == ' ' (space)
		routePattern = strings.Trim(routePattern, " ")
	}
	if routePattern[0] == 47 || routePattern[len(routePattern)-1] == 47 {  // 47 == '/'
		routePattern = strings.Trim(routePattern, "/")  // Remove all extra trailing and prefix slashes
	}
	routePattern = fmt.Sprintf("/%s", routePattern)
	routePattern = strings.Split(routePattern, "?")[0]
	routePattern = strings.Split(routePattern, "#")[0]

	if routePattern == "/" {
		return ""  // Necessary for default route
	}

	return routePattern
}

func routeWithFilepath(routes []Route, filepath string) Route {
	for _, route := range routes {
		if route.Filepath == filepath {
			return route
		}
	}
	return Route{}
}

var DefaultPages = []string{"/index.html", "/index.htm", "/default.html", "/default.htm"}

func MakeForest(routes []Route) (map[int] *Node) {
	defer TrackTime("Making returnForest")()
	defer func() {
		if r := recover(); r != nil {
			log.Panicf("Error in generating forest due to %v", r.(string))
		}
	}()

	routes = CleanAndSortRoutes(routes)
	//printBySegment(routes)

	forest := make(map[int] *Node)
	for _, route := range routes {
		segmentSize := strings.Count(route.Pattern, "/")

		segmentRoot := forest[segmentSize]

		if segmentRoot == nil {
			segmentRoot = &Node{Root:true}
			forest[segmentSize] = segmentRoot
		}

		if err := segmentRoot.Add(route); err != nil {
			panic(fmt.Sprintf("Error in adding route: %v due to: %v", route.Pattern, err))
		}
	}

	return forest
}

func CleanAndSortRoutes(routes []Route) []Route {
	hasDefaultRoute := false
	for i, route := range routes {
		routes[i].Pattern = CleanseRoute(route.Pattern)
		hasDefaultRoute = routes[i].Pattern == ""
	}

	if !hasDefaultRoute {
		for _, defaultPage := range DefaultPages {
			route := routeWithFilepath(routes, defaultPage)
			routes = append(routes,
			Route{
				ResourceId: route.ResourceId,
				Filetype: route.Filetype,
				Filepath: route.Filepath,
				Pattern:  "",
				IsStatic: route.IsStatic,
			})
			break
		}
	}

	//TODO: Also for any directory, DON'T OVERRIDE Conflicting Routes
	//if resource.IsDefault == true ||
	//	route.Filepath == route.Pattern &&  // If Static File
	//	stringSuffixInList(resource.FilePath, DefaultPages) {   // Is a Static Default Page
	//	defaultRoute := "/"
	//	redirectPath := ""
	//	if strings.Count(route.Pattern, "/") > 1{
	//		defaultRoute = RemoveLast(route.Pattern, "/")
	//		redirectPath = defaultRoute + "/"
	//	}
	//	records = append(records,
	//		Route{
	//			ResourceId: route.ResourceId,
	//			Filetype: route.Filetype,
	//			Filepath: route.Filepath,
	//			Pattern:  defaultRoute,
	//			RedirectPath: redirectPath,
	//		})
	//}

	// TODO: Can be avoided as, Tree Grouping is done
	// Ascending Order by Segment Size
	segmentCount := func(r1, r2 *Route) bool {
		return strings.Count(r1.Pattern, "/") < strings.Count(r2.Pattern, "/")
	}

	// Ascending Order by No. Occurrence of Regex
	regexCount := func(r1, r2 *Route) bool {
		return strings.Count(r1.Pattern, StringRegexpStart) < strings.Count(r2.Pattern, StringRegexpStart)
	}

	// Descending Order by Index of Regex from Beginning of Segment
	regexPositionFromStart := func(r1, r2 *Route) bool {
		return WeightedSegmentScoreFromStart(r1.Pattern) > WeightedSegmentScoreFromStart(r2.Pattern)
	}

	// Descending Order by Index of Regex from Ending of Segment
	regexPositionFromLast := func(r1, r2 *Route) bool {
		return WeightedSegmentScoreFromLast(r1.Pattern) > WeightedSegmentScoreFromLast(r2.Pattern)
	}

	OrderedBy(segmentCount, regexCount, regexPositionFromStart, regexPositionFromLast).Sort(routes)
	return routes
}

func Match(patternTree *Node, cleanUrl string) (MatchResponse, string) {
	segments := strings.Split(cleanUrl, "/")
	result := traverse(patternTree, 1, segments, len(segments))
	return MatchResponse{result, result != nil,  ""}, ""
}

func MatchInTree(incomingUrl string, forest Forest) (MatchResponse, string) {
	cleanUrl := CleanseRoute(incomingUrl)
	segmentCount := strings.Count(cleanUrl, "/")

	patternTree := forest.GetTree(segmentCount)

	if patternTree == nil {
		count, err := forest.CountTrees()
		log.Printf("Got Tree Count as: %v", count)
		if count > 0 && err == "" {
			return MatchResponse{}, ""
		} else if count == -1 {
			return MatchResponse{}, err
		} else {
			return MatchResponse{}, fmt.Sprintf("Trees not generated")
		}
	} else {
		matchResult, err := Match(patternTree, cleanUrl)

		if matchResult.Matched {  // == "/"
			return matchResult, err
		} else {
			segmentCount := segmentCount + 1
			patternTree := forest.GetTree(segmentCount)

			if patternTree == nil {
				return matchResult, err
			}

			//newCleanUrl := fmt.Sprintf("%s%s", cleanUrl, indexDotHtml) // TODO: Use DefaultPages

			for _, defaultPage := range DefaultPages {
				newCleanUrl := fmt.Sprintf("%s%s", cleanUrl, defaultPage)
				matchResult, err := Match(patternTree, newCleanUrl)

				if matchResult.Matched &&
					strings.ToLower(matchResult.Result.Route.Filepath) ==
					strings.ToLower(newCleanUrl) {	// it's a static page
					if incomingUrl[len(incomingUrl)-1] == 47 { 		 	// has trailing slash  47 == '/'
						return matchResult, err
					} else {
						matchResult.RedirectPath = fmt.Sprintf("%s/", cleanUrl)  // Add Redirect Path
						return matchResult, err
					}
				}
			}

			return matchResult, err
		}
	}
}

func MatchWithForest(forest map[int] *Node, incomingUrl string) (MatchResponse, string) {
	defer TrackTime("Matching with Forest")()
	return MatchInTree(incomingUrl, ForestInMemory{forest})
}

func MatchWithCache(prefix string, incomingUrl string) (MatchResponse, string) {
	defer TrackTime("Serving Match Request From Cache")()
	return MatchInTree(incomingUrl, ForestInCache{prefix})
}

func PrintForest(forest map[int] *Node) {
	for k, root := range forest {
		fmt.Printf("\nSegment: %v\n", k)
		root.Show()
	}
}


//func main() {
//	records := ReadFromDatabase("new_KitsuneResources",
//		bson.M{"ProjectId": "5a1d651fa2fe3f0516d75e83", "IsArchived": false})
//	//records := ReadFromFile("resources_bnb.csv")
//	forest := MakeForest(records)
//
//	//PrintForest(forest)
//
//	r, e := MatchWithForest(forest, "/")
//	fmt.Printf("Matched: %v", r.Matched)
//	fmt.Printf("\nFile: %v: %v", e, r.Result.Route.Filepath)
//	fmt.Printf("\nResult: %v: %v", e, r.Result.Route)
//}