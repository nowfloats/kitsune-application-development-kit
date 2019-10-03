const dot = (color = '#ccc', outline = false) => ({
    alignItems: 'center',
    display: 'flex',

    ':before': {
        backgroundColor: outline ? null : color,
        border: outline ? `1px solid ${color}` : null,
        borderRadius: 10,
        content: '" "',
        display: 'block',
        marginRight: 8,
        height: 10,
        width: 10,
    },
});

// Curious blue // Waterloo // Flamingo
const BASE_THEME_COLOR = '51,163,220';//'123,130,150';//'240,100,40';
// Blue bayeux
const BASE_FONT_COLOR = '81,105,133';

export default function (props) {
    const { colorMap, creatable } = props;
    return {
        // Don't show the separator
        clearIndicator: (styles) => ({ ...styles, display: creatable ? null : 'none' }),
        menu: styles => ({ ...styles, marginTop: 0, maxHeight: '30vh', borderRadius: '0 0 5px' }),
        menuList: styles => ({ ...styles, paddingTop: 0, maxHeight: '30vh' }),
        control: (styles, { hasValue, getValue, isDisabled, isFocused }) => {
            const commonStyles = {
                ...styles,
                width: '100%',
                opacity: isDisabled ? 0.6 : null,
                cursor: isDisabled ? 'not-allowed' : null,
                boxShadow: 'none',
                border: `1px solid rgba(133,141,162, ${isFocused ? 1 : 0.5}) !important`,

            };

            // If provided a color map, use that to code everything
            if (colorMap) {
                const data = hasValue ? getValue()[0] : {};
                const color = data && data.value in colorMap ? colorMap[data.value] : colorMap['default'];
                return {
                    ...commonStyles,
                    border: hasValue ? `1px solid rgba(${color}, 0.4) !important` : commonStyles.border,
                    backgroundColor: hasValue ? `rgba(${color}, 0.05) !important` : styles.backgroundColor
                }
            } else {
                return commonStyles;
            }
        },
        indicator: (styles, { data }) => {
            const commonStyles = {
                ...styles,
            };

            if (colorMap) {
                const color = data && data.value in colorMap ? colorMap[data.value] : colorMap['default']
                return {
                    ...commonStyles,
                    borderColor: `rgb(${color}) transparent transparent`
                }
            } else {
                return commonStyles;
            }
        },
        option: (styles, { data, isDisabled, isFocused, isSelected }) => {
            const commonStyles = {
                ...styles,
                backgroundColor: isFocused ? `rgba(${BASE_THEME_COLOR}, 0.05) !important` : null,
                color: isFocused ? `rgba(${BASE_FONT_COLOR}, 1)` : isSelected ? `rgba(${BASE_FONT_COLOR}, 0.7)` : `rgba(${BASE_FONT_COLOR}, 0.6)`
            };

            if (colorMap) {
                const color = data && data.value in colorMap ? colorMap[data.value] : colorMap['default']
                return {
                    ...commonStyles,
                    backgroundColor: isFocused || isSelected ? `rgba(${color}, 0.05) !important` : null,
                    color: isFocused || isSelected ? `rgb(${color}) !important` : null,
                    cursor: isDisabled ? 'not-allowed' : 'default',

                    '::before': {
                        ...styles['::before'],
                        backgroundColor: color,
                    },

                    ...dot(`rgb(${color})`)
                }
            } else {
                return commonStyles;
            }
        },
        placeholder: styles => {
            const commonStyles = {
                ...styles,
                color: `rgba(${BASE_FONT_COLOR}, 0.5) !important`,
            };

            if (colorMap) {
                const color = colorMap['default']
                return {
                    ...commonStyles,
                    color: `rgb(${color}) !important`,
                    ...dot(`rgb(${color})`, true)
                }
            } else {
                return commonStyles;
            }
        },
        singleValue: (styles, { data }) => {
            const commonStyles = {
                ...styles,
                color: `rgba(${BASE_FONT_COLOR}, 0.8) !important`,
            };

            if (colorMap) {
                const color = data && data.value in colorMap ? colorMap[data.value] : colorMap['default']
                return {
                    ...commonStyles,
                    color: `rgb(${color}) !important`,
                    ...dot(`rgb(${color})`)
                }
            } else {
                return commonStyles;
            }
        },
    }
}