import React, { Component } from 'react';
import { connect } from 'react-redux';
import Editor from './index';
import Tab from './tabs/index';
import { tabChanged, fileClose, checkFileChanged } from '../../actions/editor';

class MultiTab extends Component {
    constructor(props) {
        super(props);

        this.switchTab = this.switchTab.bind(this);
        this.closeTab = this.closeTab.bind(this);

        this.activeTab = null;
    }

    closeTab(index) {
        this.props.checkFileChanged(() => {
            this.props.closeTab(index);
        }, index);
    }

    switchTab(index) {
        this.props.switchTab(index);
    }

    scrollToActiveTab() {
        const activeTab = document.querySelector('#multi-tab-active');
        if (activeTab) {
            // If we have an active tab, get the bounding rectangle
            const activeRect = activeTab.getBoundingClientRect();

            // Get the tab container, and its bounding rectangle
            const container = document.querySelector('.tabs');
            const contRect = container.getBoundingClientRect();

            let scroll = 0;
            if(activeRect.right > contRect.right) {
                // The active tab is to the right of the visible area
                scroll = activeRect.right - contRect.right;
            } else if(activeRect.left < contRect.left) {
                // The active tab is to the left of the visible area
                scroll = activeRect.left - contRect.left;
            }

            // Scroll by the amount active tab is ahead of the container
            // When its behind, the difference is negative, so we scroll back
            container.scrollLeft += scroll;
        }
    }

    isReadOnlyType(fileType) {
        return ([''].indexOf(fileType) >= 0);
    }

    componentDidUpdate() {
        this.scrollToActiveTab();
    }

    render() {
        const { className, style, activeTabs, visibleIndex, isFetching } = this.props;
        const hasSupportedEditors = activeTabs.some(tab => !!tab.type);

        return (
            <div style={style} className={`multi-tab ${hasSupportedEditors ? '' : 'hide'} ${className || ''}`}>
                <div className="tabs">
                    {
                        activeTabs.map((editor, index) => {
                            const { name, type } = editor;
                            const isVisible = visibleIndex === index;
                            return (!!!type ? null :
                                <Tab
                                    id={`multi-tab-${isVisible ? 'active' : index}`}
                                    isActive={isVisible}
                                    editor={editor}
                                    key={`tabbed-editor-tab-${index}`}
                                    readOnly={this.isReadOnlyType(type)}
                                    filename={name}
                                    removeTab={() => this.closeTab(index)}
                                    onClick={() => this.switchTab(index)}
                                />
                            )
                        })
                    }
                </div>
                <div className="tab-content-area">
                    {
                        activeTabs.map((editor, index) => {
                            const { type } = editor;
                            const isVisible = visibleIndex === index;

                            return (!!!type ? null :
                                <Editor
                                    key={`tabbed-editor-${index}`}

                                    editor={editor}
                                    tabIndex={index}
                                    isVisible={isVisible}

                                    className={className}

                                    style={{
                                        flex: isVisible ? 1 : 0
                                    }}

                                    isEditorFetching={isFetching}
                                />
                            )
                        })
                    }
                </div>
            </div>
        )
    }
}

const mapStateToProps = state => {
    const { activeTabs, visibleIndex, isFetching } = state.editorReducer;

    return {
        activeTabs,
        visibleIndex,
        isFetching
    }
};

const mapDispatchToProps = dispatch => ({
    switchTab: index => dispatch(tabChanged(index)),
    closeTab: index => dispatch(fileClose(index)),
    checkFileChanged: (callback, tabIndex) => dispatch(checkFileChanged(callback, tabIndex))
});

export default connect(mapStateToProps, mapDispatchToProps)(MultiTab);