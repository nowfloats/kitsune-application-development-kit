package routing

import (
	"io/ioutil"
	"log"
	"os"
	"reflect"
	"sort"
	"strconv"
	"testing"
)

type routeTest struct{
	incomingUrl string
	fileName string
	shouldMatch bool
	redirectPath string
	isStatic bool
}

var forest = MakeForest(ReadFromFile("resources_bnb.csv"))
//var routes, _ = ReadFromDatabase("new_KitsuneResourcesProduction", bson.M{"ProjectId": "5a4ced817a25030518487e05"})
//var forest = MakeForest(routes)

func testRoute(t *testing.T, forest map[int] *Node, test routeTest) {
	matchResponse, _ := MatchWithForest(forest, test.incomingUrl)
	if matchResponse.Matched != test.shouldMatch {
		t.Errorf("MatchWithForest Result not Equal: expected %v, got: %v for: %v", test.shouldMatch, matchResponse.Matched, test.incomingUrl)
		return
	}

	if test.shouldMatch {
		if test.fileName != matchResponse.Result.Route.Filepath {
			t.Errorf("File Path not Equal: expected %v, got: %v for: %v", test.fileName, matchResponse.Result.Route.Filepath, test.incomingUrl)
			return
		}

		if test.redirectPath != "" &&  test.redirectPath != matchResponse.RedirectPath {
			t.Errorf("Redirect Path not Equal: expected %v, got %v", test.redirectPath, matchResponse.RedirectPath)
			return
		}

		if test.isStatic != matchResponse.Result.Route.IsStatic {
			t.Errorf("IsStatic not Equal: expected %v, got %v", test.isStatic, matchResponse.Result.Route.IsStatic)
		}
	}
}

func doTests(t *testing.T, tests []routeTest) {
	if _, ok := os.LookupEnv("DISABLE_LOGGING"); ok {
		log.SetOutput(ioutil.Discard)
	}
	for _, test := range tests {
		testRoute(t, forest, test)
	}
}

func doTestsForForest(forest map[int] *Node, t *testing.T, tests []routeTest) {
	if _, ok := os.LookupEnv("DISABLE_LOGGING"); ok {
		log.SetOutput(ioutil.Discard)
	}
	for _, test := range tests {
		testRoute(t, forest, test)
	}
}

func TestMatchDynamicUrls(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/India/p/Amplify",
			fileName: "/product-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/Ind,ia/p/Amp,lify",
			fileName: "/product-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/India/p/Amplify!#$%^&*()-.",
			fileName: "/product-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/latest-offers/in-bhubaneswar,cuttack/",
			fileName: "/offer-list.html.dl",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestMatchStaticUrls(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/image-gallery.html",
			fileName: "/image-gallery.html",
			shouldMatch: true,
		},
		{
			incomingUrl: "/index.html",
			fileName: "/index.html",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestMatchingPartial(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/latest-updates/u25",
			fileName: "/update-details.html.dl",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestNonMatching(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/something/that/should/never/exist",
			fileName: "/non-existent.html",
			shouldMatch: false,
		},
		{
			incomingUrl: "/latest-updates/uSomething/NotThere",
			fileName: "/catch-all-three.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/hacss/hastyle.css",
			fileName: "/css/style.css",
			shouldMatch: false,
		},
	}

	doTests(t, tests)
}

func TestEdgeCases(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/css/default-skin.css",
			fileName: "/css/default-skin.css",
			shouldMatch: true,
		},
		{
			incomingUrl: "/css/default-skin.css#",
			fileName: "/css/default-skin.css",
			shouldMatch: true,
		},
		{
			incomingUrl: "/css/default-skin.css?/something.html",
			fileName: "/css/default-skin.css",
			shouldMatch: true,
		},
		{
			incomingUrl: "/css/default-skin.css.html",
			fileName: "/css/default-skin.css",
			shouldMatch: false,
		},
		{
			incomingUrl: "/css/default-skin.css@",
			fileName: "/css/default-skin.css",
			shouldMatch: false,
		},
	}

	doTests(t, tests)
}

func TestExtraSlashes(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/hello/o/over-the-top/////////////",
			fileName: "/offer-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/hello///o/over-the-top/",
			fileName: "/offer-details.html.dl",
			shouldMatch: false,
		},
		{
			incomingUrl: "///hello/o/over-the-top/",
			fileName: "/offer-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/hello/o/over-the-top/",
			fileName: "/offer-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/hello/o/over-the-top",
			fileName: "/offer-details.html.dl",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestDifferentCasing(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/HELLO/UNDER-THE-CURVE",
			fileName: "/update-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/hello/under-the-curve",
			fileName: "/update-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/HeLLo/Under-The-Curve",
			fileName: "/update-details.html.dl",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestOptionalPaginationSegment(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/search/keywords/1",
			fileName: "/search.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/search/keywords",
			fileName: "/search.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/locations/shops-1",
			fileName: "/locations-LIST.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/locations/",
			fileName: "/locations-LIST.html.dl",
			shouldMatch: false,
		},
		{
			incomingUrl: "/shops-1/",
			fileName: "/shops-LIST.html.dl",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestTrailingSlashMatch(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/kotak/index.html",
			fileName: "/kotak/index.html",
			shouldMatch: true,
		},
		{
			incomingUrl: "/kotak/",
			fileName: "/kotak/index.html",
			shouldMatch: true,
		},
		{
			incomingUrl: "/kotak",
			fileName: "/kotak/index.html",
			shouldMatch: true,
			redirectPath: "/kotak/",
		},
		{
			incomingUrl: "/home/about/index.html",
			fileName: "/home/about/index.html",
			shouldMatch: true,
		},
		{
			incomingUrl: "/home/about/",
			fileName: "/home/about/index.html",
			shouldMatch: true,
		},
		{
			incomingUrl: "/home/about",
			fileName: "/home/about/index.html",
			shouldMatch: true,
			redirectPath: "/home/about/",
		},
	}

	doTests(t, tests)
}

func TestDifferentCasingInStatic(t *testing.T) {
	tests := []routeTest{
		{
			incomingUrl: "/oPtImIzE-your-SITE",
			fileName: "/OpTiMiZe-YOUR-site/index.html",
			shouldMatch: true,
			redirectPath: "/oPtImIzE-your-SITE/",
		},
		{
			incomingUrl: "/optimize-your-site/",
			fileName: "/OpTiMiZe-YOUR-site/index.html",
			shouldMatch: true,
		},
		{
			incomingUrl: "/OPTIMIZE-YOUR-SITE/index.html",
			fileName: "/OpTiMiZe-YOUR-site/index.html",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestIndexHtmlAtRoot(t *testing.T) {
	tests := []routeTest{
		{
			incomingUrl: "/index.html",
			fileName: "/index.html",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestIndexMatch(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/",
			fileName: "/index.html",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestMixedPatterns(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/delhi/o/a-day-in-city-tour",
			fileName: "/dynamic-a.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/delhi/o/apple-a-day-keeps-the-doctor-away",
			fileName: "/dynamic-apple.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/delhi/o/apple-computers-in-town",
			fileName:    "/dynamic-apple-computers.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/delhi/o/boating-in-city",
			fileName: "/dynamic-city.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/delhi/o/coffee-in-city-of-diamonds",
			fileName: "/dynamic-city-diamonds.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/delhi/o/apple-computers-in-city",
			fileName: "/dynamic-apple-computers.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/delhi/o/something-that-is-not-matched-by-others",
			fileName: "/offer-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/delhi/o/apple-computers-letter",
			fileName: "/dynamic-apple-computers-letter.html.dl",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestCommaMatch(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/for-address-silicon%20valley,madhapur,hyderabad-in-india",
			fileName: "/campaign-pages.html.dl",
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestIsStaticProperty(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/",
			fileName: "/index.html",
			isStatic: false,
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func TestCleanAndSortRoutes(t *testing.T) {
	routes := CleanAndSortRoutes(ReadFromFile("resources_ordered.csv"))

	var routesOrder []int

	for _, route := range routes {
		if route.Filepath == "" {
			continue
		}
		routeIndex, _ := strconv.Atoi(route.Filepath)
		routesOrder = append(routesOrder, routeIndex)
	}

	sortedOrder := routesOrder[:]
	sort.Ints(sortedOrder[:])

	if !reflect.DeepEqual(sortedOrder, routesOrder) {
		t.Errorf("Routes not sorted correctly.\n expected: %v\n got: %v", sortedOrder, routesOrder)
	}
}

func TestWithCatchAll(t *testing.T) {
	tests := []routeTest {
		{
			incomingUrl: "/pages/permalink-title/someId",
			fileName: "/Custom.html.dl",
			isStatic: false,
			shouldMatch: true,
		},
	}

	doTests(t, tests)
}

func Test_FixRightStaticEscalation(t *testing.T) {
	forest := MakeForest(ReadFromFile("resources_alexandria.csv"))

	tests := []routeTest {
		{
			incomingUrl: "/anything/-/-",
			fileName: "/escalates.html",
			isStatic: false,
			shouldMatch: true,
		},
		{
			incomingUrl: "/anything/here/there",
			fileName: "/catch-all.html",
			isStatic: false,
			shouldMatch: true,
		},
		{
			incomingUrl: "/pages/permalink-title/someId",
			fileName: "/pages.html",
			isStatic: false,
			shouldMatch: true,
		},
	}

	doTestsForForest(forest, t, tests)
}