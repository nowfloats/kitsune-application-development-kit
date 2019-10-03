export function SortPropertyByGroupName([ aName ] , [ bName ]) {
	const a = aName.toUpperCase();
	const b = bName.toUpperCase();
	if (a < b) {
		return -1;
	}
	if (a > b) {
		return 1;
	}
    // names must be equal
	return 0;
}