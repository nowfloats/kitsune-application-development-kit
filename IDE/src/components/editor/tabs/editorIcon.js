import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { config } from '../../../config';

export default class EditorTabIcon extends Component {
    static propTypes = {
        fileType: PropTypes.string
    }

    render() {
        const { extensionsIconMap } = config.INTERNAL_SETTINGS;
        const { fileType, ...otherProps } = this.props;
        const iconType = extensionsIconMap.get(fileType) === undefined ? 'fas fa-file' : extensionsIconMap.get(fileType);

        return (
            <i className={`editor-file-icon ${iconType}`} {...otherProps} />
        )
    }
}
