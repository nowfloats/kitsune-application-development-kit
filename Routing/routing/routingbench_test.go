package routing

import (
	"testing"
)

func benchmarkRoute(b *testing.B, forest map[int] *Node, test routeTest) {
	b.ResetTimer()
	for i := 0; i < b.N ; i++ {
		MatchWithForest(forest, test.incomingUrl)
	}
}

func doBenchmarks(b *testing.B, tests []routeTest) {
	forest := MakeForest(ReadFromFile("resources_bnb.csv"))
	for _, test := range tests {
		benchmarkRoute(b, forest, test)
	}
}

func BenchmarkTreeGeneration(b *testing.B) {
	for i := 0; i < b.N; i++  {
		MakeForest(ReadFromFile("resources_bnb.csv"))
	}
}

func BenchmarkMatchDynamicUrls(b *testing.B) {
	tests := []routeTest {
		{
			incomingUrl: "/India/p/Amplify",
			fileName: "/product-details.html.dl",
			shouldMatch: true,
		},
		{
			incomingUrl: "/latest-offers/in-bhubaneswar/",
			fileName: "/offer-list.html.dl",
			shouldMatch: true,
		},
	}

	doBenchmarks(b, tests)
}

func BenchmarkMatchStaticUrls(b *testing.B) {
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

	doBenchmarks(b, tests)
}

func BenchmarkMatchingPartial(b *testing.B) {
	tests := []routeTest {
		{
			incomingUrl: "/latest-updates/u25",
			fileName: "/update-details.html.dl",
			shouldMatch: true,
		},
	}

	doBenchmarks(b, tests)
}

func BenchmarkNonMatching(b *testing.B) {
	tests := []routeTest {
		{
			incomingUrl: "/something/that/should/never/exist",
			fileName: "/non-existent.html",
			shouldMatch: false,
		},
		{
			incomingUrl: "/latest-updates/uSomething/NotThere",
			fileName: "/non-existent.html",
			shouldMatch: false,
		},
		{
			incomingUrl: "/hacss/hastyle.css",
			fileName: "/css/style.css",
			shouldMatch: false,
		},
	}

	doBenchmarks(b, tests)
}

func BenchmarkEdgeCases(b *testing.B) {
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

	doBenchmarks(b, tests)
}

func BenchmarkExtraSlashes(b *testing.B) {
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

	doBenchmarks(b, tests)
}

func BenchmarkDifferentCasing(b *testing.B) {
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

	doBenchmarks(b, tests)
}

func BenchmarkOptionalPaginationSegment(b *testing.B) {
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
	}

	doBenchmarks(b, tests)
}