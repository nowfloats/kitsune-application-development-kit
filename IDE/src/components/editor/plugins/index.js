import React from 'react';
import DataModelIcon from '../../../images/language-white.svg';
import SchemaBase from '../../schema/schema-creator/base';
import JSONSchemEditor from './json-schema-editor';
import getJSONSchema from './json-schemas';

export const EDITOR_PLUGINS = {
    JSON_EDITOR: 'JSON_EDITOR',
    SCHEMA_VIEWER: 'SCHEMA_VIEWER'
}

const PLUGIN_CONF = {
    [EDITOR_PLUGINS.JSON_EDITOR]: {
        title: 'JSON Editor',
        icon: {
            clazz: 'icon-json-editor',
            title: 'json editor',
            text: '{ }'
        },
        Component: ({ fileName, fileType, fileData, onChange, className }) => {
            let data = {},
                schema = {};

            try {
                data = JSON.parse(fileData);
                schema = getJSONSchema(fileName, fileType, data);
            } catch (err) {
                return <PluginError message={err.message} />
            }

            return Object.keys(data).length === 0 ?
                (
                    <PluginError message="Empty JSON" />
                ) :
                (
                    <JSONSchemEditor
                        schema={schema}
                        formData={JSON.parse(fileData)}
                        onChange={({ formData }) => onChange(formData)}
                    />
                )
        }
    },
    [EDITOR_PLUGINS.SCHEMA_VIEWER]: {
        title: 'Data Model Preview',
        icon: {
            clazz: 'icon-data-model',
            title: 'data models',
            src: DataModelIcon
        },
        Component: ({ schemaClasses }) => (
            !!schemaClasses ?
                <SchemaBase
                    baseClasses={schemaClasses}
                    helpers={{}}
                    readOnly={true} />
                : null
        )
    }
}

const PluginError = ({ message }) => (
    <div className="plugin-error">
        {message}
    </div>
)

const PluginIcon = ({ clazz, title, src, text, onClick, isActive }) => {
    return (
        <div key={`plugin-icon-${src}`} className={`plugin-icon ${!!isActive ? 'active' : ''} ${clazz}`}
            title={title}
            onClick={onClick}
        >
            {!!src ? <img src={src} alt={title} /> : null}
            {!!text ? <span className="icon-text">{text}</span> : null}
        </div>
    )
}

export const renderPluginIcons = (fileTypeConfig, onClick, active) => {
    const { types } = fileTypeConfig;
    const result = [];
    for (let type of types) {
        if (type in PLUGIN_CONF) {
            result.push(PluginIcon({
                ...(PLUGIN_CONF[type].icon),
                isActive: (active === type),
                onClick: () => {
                    if (onClick) onClick(type);
                }
            }));
        }
    }

    return result;
}

export const renderPluginComponents = (fileTypeConfig, props, active) => {
    if (!!active && !!fileTypeConfig) {
        const { types } = fileTypeConfig;
        for (let type of types) {
            if (type in PLUGIN_CONF && type === active) {
                const { title, Component } = PLUGIN_CONF[type];
                return (
                    <div className="plugin-container">
                        <div className="plugin-title">
                            <hr />
                            <span>{title}</span>
                            <hr />
                        </div>
                        <div className="plugin-content">
                            <Component {...props} />
                        </div>
                    </div>
                );
            }
        }
    }

    return null;
}