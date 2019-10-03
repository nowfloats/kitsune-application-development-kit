package routing

import (
	"sort"
	"strings"
	"fmt"
)

func printBySegment(routes []Route) {
	for _, route := range routes {
		segs := strings.Count(route.Pattern, "/")
		count := strings.Count(route.Pattern, StringRegexpStart)
		fscore := WeightedSegmentScoreFromStart(route.Pattern)
		lscore := WeightedSegmentScoreFromLast(route.Pattern)
		fmt.Printf("%-75s%-30s\t%v\t\ts:%v c:%v f:%-3v l:%v\n", route.Pattern, route.Filepath, route.Filetype, segs, count, fscore, lscore)
	}
}

type lessFunc func(p1, p2 *Route) bool

// multiSorter implements the Sort interface, sorting the changes within.
type multiSorter struct {
	changes []Route
	less    []lessFunc
}

// Sort sorts the argument slice according to the less functions passed to OrderedBy.
func (ms *multiSorter) Sort(changes []Route) {
	ms.changes = changes
	sort.Sort(ms)
}

// OrderedBy returns a Sorter that sorts using the less functions, in order.
// Call its Sort method to sort the data.
func OrderedBy(less ...lessFunc) *multiSorter {
	return &multiSorter{
		less: less,
	}
}

// Len is part of sort.Interface.
func (ms *multiSorter) Len() int {
	return len(ms.changes)
}

// Swap is part of sort.Interface.
func (ms *multiSorter) Swap(i, j int) {
	ms.changes[i], ms.changes[j] = ms.changes[j], ms.changes[i]
}

// Less is part of sort.Interface. It is implemented by looping along the
// less functions until it finds a comparison that is either Less or
// !Less. Note that it can call the less functions twice per call. We
// could change the functions to return -1, 0, 1 and reduce the
// number of calls for greater efficiency: an exercise for the reader.
func (ms *multiSorter) Less(i, j int) bool {
	p, q := &ms.changes[i], &ms.changes[j]
	// Try all but the last comparison.
	var k int
	for k = 0; k < len(ms.less)-1; k++ {
		less := ms.less[k]
		switch {
		case less(p, q):
			// p < q, so we have a decision.
			return true
		case less(q, p):
			// p > q, so we have a decision.
			return false
		}
		// p == q; try the next comparison.
	}
	// All comparisons to here said "equal", so just return whatever
	// the final comparison reports.
	return ms.less[k](p, q)
}