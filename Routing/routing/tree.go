package routing

import (
	"fmt"
	"strings"
	"regexp"
	"log"
	"github.com/pkg/errors"
	"net/url"
)

func (n *Node) ChildBySegment(segment string) *Node {
	// Can't use maps, as duplicate segments needs to be inserted for catch-all routes
	// 3 =|-> * -> a -> b
	// 	  |-> p -> * -> *
	// 	  |-> * -> * -> *
	for _, ch := range n.Children {
		if ch.Segment == segment {
			return ch
		}
	}
	return nil
}

func (n *Node) Add(route Route) (err error) {
	lastSegment := ""
	defer func() {
		if r := recover(); r != nil {
			err = errors.New(fmt.Sprintf("Regex compilation error in %v due to: %v", lastSegment, r))
		}
	}()
	//fmt.Printf("adding: %v\n", route)
	parent := n
	for _, segment := range strings.Split(route.Pattern, "/") {
		if segment == "" {
			continue
		}
		child := parent.ChildBySegment(segment)
		if route.IsCatchAllRoute() {
			child = nil
		}
		if child == nil {
			// Check if the pattern is valid regex
			lastSegment = segment
			_ = regexp.MustCompile(fmt.Sprintf(patternPlaceholder, segment))

			child = &Node{Segment:segment}
			parent.Children = append(parent.Children, child)
		}
		parent = child
	}
	parent.Route = &route
	return nil
}

func (n *Node) Show() {
	n.ShowWithIndent(0)
}

func (n *Node) ShowWithIndent(i int) {
	spaces := "     "
	indent := strings.Repeat(spaces, i)
	if i == 1 {
		indent = strings.Repeat(strings.Replace(spaces, " ", "-", -1), i)
	}
	i++

	f := ""
	if n.Route != nil && n.Route.Filepath != "" {
		f = "file: " + n.Route.Filepath + "; pattern_path: " + n.Route.Pattern + "\n|"
	}

	if n.Root {
		//fmt.Println("[]-> Root")
	} else {
		fmt.Printf("|%v|-> Segment: %v; %v\n", indent, n.Segment, f)
	}


	for _, ch := range n.Children {
		ch.ShowWithIndent(i)
	}
}

const patternPlaceholder = "(?i)^%s$" // == Case Insensitive + Start Anchor + PATTERN + End Anchor

func theseMatch(pattern string, text string) (result bool) {
	defer func() {
		if r := recover(); r != nil {
			log.Fatalf("Invalid regex in pattern: %v, err: %v", pattern, r)
			// TODO: Return with some other response code to signal MAYBE THIS IS THE FILE
			result = false
		}
	}()
	pattern = fmt.Sprintf(patternPlaceholder, pattern)
	regex := regexp.MustCompile(pattern)
	doMatch := regex.MatchString(url.PathEscape(text))
 	return doMatch
}

func traverse(subTree *Node, segIndex int, segments []string, totalSegmentLength int) (*Node) {
	//fmt.Println("\nUsing ----------------- v")
	//subTree.Show()

	if totalSegmentLength == segIndex {
		return subTree
	}

	currentSegmentValue := segments[segIndex]

	if subTree.Children == nil {
		return nil
	} else {
		for _, node := range subTree.Children {
			if theseMatch(node.Segment, currentSegmentValue) {
				if result := traverse(node, segIndex + 1, segments, totalSegmentLength); result == nil {
					continue
				} else {
					return result
				}
			}
		}
	}

	return nil
}