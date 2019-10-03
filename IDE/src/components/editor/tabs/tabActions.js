import React, { Component } from 'react';
import { toastr } from 'react-redux-toastr';
import FullScreenIcon from '../../../images/enlarge-icon.svg';
import SaveIcon from '../../../images/saveIcon.svg';

export class TabActions extends Component {
    static propTypes = {

    }

    state = {
        isCodeButtonToggled: false
    }

    constructor(props) {
        super(props);

        this.toggleDropdownCodeButtons = this.toggleDropdownCodeButtons.bind(this);
        this.cloneWebForm = this.cloneWebForm.bind(this);
        this.fullScreen = this.fullScreen.bind(this);
        this.saveTask = this.saveTask.bind(this);
    }

    fullScreen() {
        const { editor: element } = this.props;
        let editor = ace.edit(`kitsune-editor-${element.path.replace(/\//g, '-')}`);
        editor = editor.container;
        if (editor.requestFullscreen) {
            editor.requestFullscreen();
        } else if (editor.mozRequestFullScreen) {
            editor.mozRequestFullScreen();
        } else if (editor.webkitRequestFullscreen) {
            editor.webkitRequestFullscreen();
        } else if (editor.msRequestFullscreen) {
            editor.msRequestFullscreen();
        }
    }

    saveTask() {
        const { webForm, schema, editor } = this.props;
        const { isOpen, helper: webformHelper } = webForm || { isOpen: false };
        const { helper: schemaHelper } = schema || {};
        const { helper: editorHelper } = editor;

        if (!editor.fileChanged) {
            toastr.info('no changes to save');
            return;
        }

        if (isOpen) {
            // Webform
            webformHelper.saveWebform();
        } else if (schema.isOpen) {
            // Kit-schema
            schemaHelper.saveSchema();
        } else {
            // Code editors/viewers
            editorHelper.saveFile();
        }
    }

    toggleDropdownCodeButtons() {
        this.setState({ isCodeButtonToggled: !this.state.isCodeButtonToggled })
    }

    cloneWebForm() {
        const { dispatch } = this.props;
        dispatch(setCanClone(true));
        dispatch(modalOpen(<CreateWebform />, createWebformLabel, null));
    }

    renderHeader(plugins) {
        const { webForm, editor, readOnly } = this.props;
        const { type } = editor;

        switch (type) {
            case 'image':
                return null;
            default:
                const { isEditable, isOpen } = webForm ? webForm : { isOpen: false };
                const { fileChanged } = editor;
                let saveButton = null;
                let cloneButon = null;
                let webformButtons = null;

                if (isOpen) {
                    // Webform is open

                    if (!isEditable) {
                        cloneButon = (<div className='clone-webform' title='clone current webForm'
                            onClick={this.cloneWebForm}>
                            <button className='header-btn' onClick={this.cloneWebForm} title='clone current webform'>
                                clone and edit
						</button>
                        </div>);
                        saveButton = null;
                    }
                    else {
                        saveButton = (<div className='save-task'
                            title={fileChanged ? 'save' : 'no changes to save'}
                            onClick={this.saveTask}><img src={SaveIcon} alt='save icon' /></div>);
                        cloneButon = null;
                    }

                    webformButtons = (
                        <div className={`dropdown-wf${isOpen ? ' show-code-buttons' : ''}`}>
                            <button className={`getCode header-btn js${this.props.webForm.isEditable ?
                                ' disable-copy' : ' enable-copy'}`}
                                data-clipboard-text=''
                                disabled={isEditable}
                                title={!isEditable ? 'get the generated js code' :
                                    'to get the generated js code please save the webform'}>
                                copy JS
						    </button>
                            <button className={`header-btn dropbtn-wf${isEditable ?
                                ' disable-copy' : ' enable-copy'}`}
                                onClick={this.toggleDropdownCodeButtons} />
                            <div id='myDropdown' className={`dropdown-content-wf${this.state.isCodeButtonToggled ?
                                ' show-html-button' : ''}`}>
                                <button className={`getCode  header-btn  html${this.props.webForm.isEditable ?
                                    ' disable-copy' : ' enable-copy'}`}
                                    disabled={isEditable}
                                    title={!isEditable ? 'get the generated html + js code' :
                                        'to get the generated html +  code please save the webform'}
                                    onClick={this.toggleDropdownCodeButtons} data-clipboard-text='' >
                                    copy JS + HTML</button>
                            </div>
                        </div>
                    )
                }
                else {
                    // Schema, or editor is open

                    saveButton = <div className='save-task'
                        title={fileChanged ? 'save' : 'no changes to save'}
                        onClick={this.saveTask}><img src={SaveIcon} alt='save icon' /></div>;
                    cloneButon = null;
                }

                return (
                    <div className='tab-header'>
                        {isOpen ? webformButtons : null}
                        {!readOnly ? plugins : null}
                        {cloneButon}
                        {!readOnly ? saveButton : null}
                        <div className='full-screen' onClick={this.fullScreen} title='full screen mode' ><img src={FullScreenIcon}
                            alt='full screen' /></div>
                    </div>
                );
        }
    }

    render() {
        const { editor, plugins } = this.props;

        if (!!editor) {
            return (
                this.renderHeader(plugins)
            )
        } else {
            throw new Error('Required editor instance not provided.')
        }
    }
}


export default TabActions;
