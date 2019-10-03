const expect = require('chai').expect
const Analyzer = require('../analyzer')

describe('Analyzer', function() {
    describe('placeholder_for', function() {
        process.env.USE_STATIC_ASSET_DOMAIN_MASK_KEY = 'enable_static_asset_cdn_domain_mask'
        process.env.ASSET_CDN_LINK = '//kit-cdn.com/'

        let analyzer = new Analyzer()
        analyzer.file_directory = '/about/'
        analyzer.projectSettings = {}
        analyzer.project_id = 'project_id'
        analyzer.resources = []
        analyzer.broken_links = []
        analyzer.logger = {}
        analyzer.logger.info = function(){}
        analyzer.projectSettings[process.env.USE_STATIC_ASSET_DOMAIN_MASK_KEY] = false
        const cdnPrefix = () => process.env.ASSET_CDN_LINK + analyzer.project_id + '/cwd'
        const toBase64 = (text) => new Buffer(text).toString('base64')

        it('make placeholders for relative links', function () {
            expect(analyzer.placeholder_for('/index.html')).to.equal(`[Kitsune:base64_${toBase64('/index.html')}]`)
        });

        it('normalize links within directories', function () {
            expect(analyzer.placeholder_for('team.html')).to.equal(`[Kitsune:base64_${toBase64('/about/team.html')}]`)
        });

        it('do not make placeholders for absolute links', function () {
            expect(analyzer.placeholder_for('http://abc.com/main.css')).to.equal(`http://abc.com/main.css`)
            expect(analyzer.placeholder_for('//abc.com/main.css')).to.equal(`//abc.com/main.css`)
        });

        it('use kit-cdn.com for asset references', function () {
            expect(analyzer.placeholder_for('/css/main.css'))
                .to.equal(`[Kitsune:base64_${toBase64(cdnPrefix() + '/css/main.css')}]`)
            expect(analyzer.placeholder_for('/js/script.js'))
                .to.equal(`[Kitsune:base64_${toBase64(cdnPrefix() + '/js/script.js')}]`)
            expect(analyzer.placeholder_for('/images/banner.jpg'))
                .to.equal(`[Kitsune:base64_${toBase64(cdnPrefix() + '/images/banner.jpg')}]`)
        });

        it('do not use kit-cdn.com for hyper links', function () {
            expect(analyzer.placeholder_for('/contact/team'))
                .to.equal(`[Kitsune:base64_${toBase64('/contact/team')}]`)
            expect(analyzer.placeholder_for('team'))
                .to.equal(`[Kitsune:base64_${toBase64('/about/team')}]`)
        });

        it('use kit-cdn.com with relative asset references', function () {
            expect(analyzer.placeholder_for('custom.css'))
                .to.equal(`[Kitsune:base64_${toBase64(cdnPrefix() + '/about/custom.css')}]`)
            expect(analyzer.placeholder_for('app.js'))
                .to.equal(`[Kitsune:base64_${toBase64(cdnPrefix() + '/about/app.js')}]`)
        })

        it('check for business rootalias url usage', function () {
            expect(analyzer.placeholder_for('[[business.rootaliasurl.url]]'))
                .to.equal('[[business.rootaliasurl.url]]');
            expect(analyzer.placeholder_for('[[business.rootaliasurl.url]]/index.html'))
                .to.equal('[[business.rootaliasurl.url]]/index.html')
        });

        it('use kit-cdn.com with business.rootaliasurl', function () {
            expect(analyzer.placeholder_for('[[business.rootaliasurl.url]]/assets/style.css'))
                .to.equal(`[Kitsune:base64_${toBase64(cdnPrefix() + '/assets/style.css')}]`)

            expect(analyzer.placeholder_for('[[business.rootaliasurl.url]]/assets/scripts.js'))
                .to.equal(`[Kitsune:base64_${toBase64(cdnPrefix() + '/assets/scripts.js')}]`)

            analyzer.file_directory = '/' // when in home page
            expect(analyzer.placeholder_for('[[business.rootaliasurl.url]]/assets/style.css'))
                .to.equal(`[Kitsune:base64_${toBase64(cdnPrefix() + '/assets/style.css')}]`)
        });

        it('ignore links with language reference other than rootaliasurl', function () {
            expect(analyzer.placeholder_for('[[business.name]]')).to.equal('[[business.name]]')
            expect(analyzer.placeholder_for('[[business.rootaliasurl.url]]/[[business.name]]'))
                .to.equal('[[business.rootaliasurl.url]]/[[business.name]]')
            expect(analyzer.placeholder_for('[[business.rootaliasurl.url]]/[[some_variable]]/main.css'))
                .to.equal('[[business.rootaliasurl.url]]/[[some_variable]]/main.css')
        });

        it('if link already has placeholder, return it', function() {
            expect(analyzer.placeholder_for('[Kitsune_/something]')).to.equal('[Kitsune_/something]')
            expect(analyzer.placeholder_for('[Kitsune:base64_LXzy=]')).to.equal('[Kitsune:base64_LXzy=]')
            expect(analyzer.placeholder_for('[Kitsune_abc]')).to.not.equal('[Kitsune_[Kitsune_abc]]')
        });
    });
});
