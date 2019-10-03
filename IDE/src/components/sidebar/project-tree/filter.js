// Helper functions for filtering
export const defaultMatcher = (filterText, { name }) => name.toLowerCase().indexOf(filterText.toLowerCase()) !== -1;

export const positionMatcher = (filterText, { name }) => name.toLowerCase().indexOf(filterText.toLowerCase());

export const findNode = (node, filter, matcher) =>
	matcher(filter, node) || // i match
		(node.children && // or i have decendents and one of them match
		node.children.length &&
		node.children.find(child => findNode(child, filter, matcher)));

export const filterTree = (node, filter, matcher = defaultMatcher) =>
	matcher(filter, node) || !node.children ?
	node :
	{
		...node,
		children: node.children
			.filter(child => findNode(child, filter, matcher))
			.map(child => filterTree(child, filter, matcher))
	};

export const expandFilteredNodes = (node, filter, matcher = defaultMatcher) => {
	const { children } = node;
	if(!children || children.length === 0) {
		return {
			...node,
			toggle: false
		};
	}
	const childrenWithMatches = node.children.filter(child => findNode(child, filter, matcher));
	const shouldExpand = childrenWithMatches.length > 0;

	return {
		...node,
		children: shouldExpand ? childrenWithMatches.map(child => expandFilteredNodes(child, filter, matcher)) : children,
		toggled: shouldExpand
	}
};

export const highlightFilteredText = (node, filter) => {
	if(node.children === null) {
		const startPosition = positionMatcher(filter, node);
		const endPosition = startPosition + filter.length;
		return {
			...node,
			highlightPosition: startPosition,
			highlightText: filter,
			beforeHighlighted: node.name.substring(0, startPosition),
			highlighted: node.name.substring(startPosition, endPosition),
			afterHighlighted: node.name.substring(endPosition, node.name.length)
		}
	} else {
		return {
			...node,
			children: node.children.map(iterator => highlightFilteredText(iterator, filter))
		}
	}
};
