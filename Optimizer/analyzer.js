const cheerio = require('cheerio');
const models = require('./models/new_models');
const $ = require('cheerio');
const fs = require('fs');
const is_relative_url = require('is-relative-url');
const url_toolkit = require('url-toolkit');
const querystring = require('querystring');
const path = require('path');
const logger_helper = require('./helpers/logger');

const is_really_relative = link => {
    return (link.startsWith('//')) ? false : is_relative_url(link);
};

class Analyzer {
    process(file, project_id, resources, projectSettings) {
        this.project_id = project_id;
        return new Promise((resolve, reject) => {
            this.logger = logger_helper.get_logger(project_id);
            this.projectSettings = projectSettings;
            this.file = file.local_file;
            this.resources = resources;
            // this.file_directory = path.dirname('/some/directory/stylesheet.css');
            this.file_directory = path.dirname(file.source_path);
            this.file_directory = (this.file_directory === '/') ? this.file_directory : this.file_directory += '/';
            this.broken_links = {};
            fs.readFile(this.file, 'utf-8', (err, data) => {
                if (err) reject(err); // TODO Use KError Object
                this.analyzed_data = '';
                // if (this.file.endsWith('Boost.html')) debugger;
                if (this.file.endsWith('.html')) {
                    this.analyzed_data = this.scan_html(data);
                } else if (this.file.endsWith('.css')) {
                    this.analyzed_data = this.scan_css(data);
                } else if(!this.projectSettings[process.env.USE_STATIC_ASSET_DOMAIN_MASK_KEY] &&
                    this.file.endsWith('.html.dl')) {
                    this.analyzed_data = this.scan_html(data);
                }
                // let new_filename = `${path.dirname(this.file)}/${path.basename(this.file, path.extname(this.file))}[PLACEHOLDERSPLACEHOLDERS]${path.extname(this.file)}`;
                let file_changed = (this.analyzed_data !== '') ? fs.writeFileSync(this.file, this.analyzed_data) : false;
                this.logger.info(`[analyzer] done for ${this.file}`);
                // console.log(this.analyzed_data);
                this.to_resolve = {"local_file":this.file,"broken_links": this.broken_links};
                resolve(this.to_resolve);
            });
        });
    }

    make_full_relative(link) {
        return url_toolkit.normalizePath(this.file_directory + link);
    }

    get_full_relative(link) {
        return  (link.startsWith('/')) ? link : this.make_full_relative(link);
    }

    is_broken(link) {
        link = (link.startsWith('/')) ? link : this.file_directory + link;
        link = (link.endsWith('/')) ? link + 'index.html' : link;
        if (!this.resources.includes(link)) {
            this.logger.info(`[analyzer] [broken_links] ${link} in ${this.file}`);
            this.broken_links[link] = 'line number';
        }
    }

    replacement_for(link) {
        link = link.trim();
        if (link.startsWith('#')) return link;

        // If the link has Kitsune Language in it
        if ( /\[\[+(.*?)\]\]+/g.test(link) ) {
            // TODO: Parse Query Params, Fragments
            return this.placeholder_for(link);
        }

        let url_link = url_toolkit.parseURL(link);
        if(url_link == null) return `${this.placeholder_for(link)}`;
        let frag = url_link.fragment;
        url_link.fragment = '';
        let query = url_link.query;
        url_link.query = '';
        if (query !== '') {
            const parsed_query = querystring.parse(query.substr(1));
            if ('v' in parsed_query) delete parsed_query['v'];
            query = (Object.keys(parsed_query).length !== 0) ?'&' + querystring.stringify(parsed_query) : '';
        }
        return `${this.placeholder_for(url_toolkit.buildURLFromParts(url_link))}${query}${frag}`
    }

    placeholder_for(link) {
        if (is_really_relative(link)) {
            // If already is a placeholder, just return
            if (/\[Kitsune_(.*)\]/gi.test(link)) return link;
            if (/\[Kitsune:base64_(.*)\]/gi.test(link)) return link;

            const link_copy = link;
            // Change [business.rootaliasurl.url]/style.css to /style.css
            let static_link = link_copy.replace(/\/?\[\[(\s*?)(business.rootaliasurl.url)(\s*?)\]\]+/ig, '');

            // If the link was only [[business.rootaliasurl.url]] or still has any schema reference, return
            if (static_link === '') return link;

            // If static_link still has language reference
            if ( /\[\[+(.*?)\]\]+/g.test(static_link) ) return link;
            // TODO: return if not asset reference, e.g. href='[business.rootalias.url]#reservations'

            let full_relative_link = this.get_full_relative(static_link);

            let clean_full_relative_link = full_relative_link.split('?')[0];
            clean_full_relative_link = clean_full_relative_link.split('#')[0];
            // Masked CDN for Static Assets
            let usingCdn = false;
            // TODO: Remove the extension check (as it may fail for unlisted extensions)
            // TODO: If `link` was not from a.href then prefix kit-cdn.com link
            if (!this.projectSettings[process.env.USE_STATIC_ASSET_DOMAIN_MASK_KEY]) {
                if (/\.(css|js|jpg|jpeg|svg|png|gif|webp|ico|ttf|eot|woff2|woff|bmp|ejs|pdf|ps|pict|eps|svgz|csv|mid|swf|doc|midi|ppt|pptx|tif|xls|xlsx|docx|tiff|jar|otf|zip|txt|rar|mov|mp4|mp3|mpeg|webm|avi|gz)$/i.test(clean_full_relative_link)) {
                    full_relative_link = process.env.ASSET_CDN_LINK + this.project_id + '/cwd' + full_relative_link;
                    this.logger.info(`[analyzer] CDN Linker: \n for: ${link} \n  is: ${full_relative_link}`);
                    usingCdn = true;
                } else if (/\[\[+(.*?)\]\]+/g.test(link)) {
                    // ^ check for language, so it holds true only for links like [[business.rootaliasurl.url]]/index.html
                    // static reference should be made into placeholders
                    return link;
                }
            }
            if (!usingCdn) this.is_broken(full_relative_link);
            return `[Kitsune:base64_${new Buffer(full_relative_link).toString('base64')}]`;
        } else {
            return link;
        }
    }

    get_link(elm, attr) {
        let link = $(elm).attr(attr);
        if (attr === 'srcset') {
            let links = link.split(',');
            links.forEach((url) => {
                url = url.trim().split(' ')[0];
                (is_really_relative(link)) ? link = link.replace(url, this.replacement_for(url)) : false;
            });
            $(elm).attr(attr, link);
        } else {
            // console.log(`${is_really_relative(link)} : ${this.placeholder_for(link)}`);
            (is_really_relative(link)) ? $(elm).attr(attr, this.replacement_for(link)) : false;
        }
    }

    scan_url(css_text) {
        return css_text.replace(/\burl\('?"?([^'|"]+?)'?"?\)/ig, (a, b) => {
                let p = this.placeholder_for(b);
                return a.replace(b, p)
            }
        )
    }

    scan_css_imports(css_text) {
        return css_text.replace(/@import[\s]+(?!.*url)'?"?([^\s|'|"]+)'?"?/ig, (a, b) => a.replace(b, this.replacement_for(b)))
    }

    scan_css(css_text) {
        this.logger.info(`[analyzer] scanning css in ${this.file}`);
        css_text = this.scan_css_imports(css_text);
        return this.scan_url(css_text);
    }

    scan_html(html_text) {
        const $ = cheerio.load(html_text,
            {decodeEntities: this.projectSettings['html.decodeEntities'],
            _useHtmlParser2: true});
        this.logger.info(`[analyzer] scanning html for ${this.file}`);
        $('*[href]').each((i, e) => {
            this.get_link(e, 'href');
        });
        $('*[src]').each((i, e) => {
            this.get_link(e, 'src');
        });
        $('*[srcset]').each((i, e) => {
            this.get_link(e, 'srcset');
        });
        $('*[style]').each((i, elm) => {
            let css_text = $(elm).attr('style');
            let replaced_css = this.scan_url(css_text);
            (replaced_css !== css_text) ? $(elm).attr('style', replaced_css) : false;
        });
        $('style').each((i, elm) => {
            let css_text = $(elm).html();
            let replaced_css = this.scan_css(css_text);
            (replaced_css !== css_text) ? $(elm).html(replaced_css) : false;
        });
        return $.html();
    }
}

module.exports = Analyzer;

// new Analyzer({
//     source_path: 'temp/weird.html',
//     local_file: 'temp/weird.html'
// }, 'project_id',  [], {});
