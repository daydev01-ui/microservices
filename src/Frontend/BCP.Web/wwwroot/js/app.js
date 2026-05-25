window.bcpApp = {
    copyToClipboard: function (text) {
        navigator.clipboard.writeText(text).catch(() => {
            const el = document.createElement('textarea');
            el.value = text;
            document.body.appendChild(el);
            el.select();
            document.execCommand('copy');
            document.body.removeChild(el);
        });
    },
    downloadFile: function (filename, base64Content, mimeType) {
        const link = document.createElement('a');
        link.href = 'data:' + mimeType + ';base64,' + base64Content;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },
    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    showToast: function (message, type) {
        const container = document.getElementById('toast-container') || (() => {
            const c = document.createElement('div');
            c.id = 'toast-container';
            c.style.cssText = 'position:fixed;bottom:1rem;right:1rem;z-index:9999;';
            document.body.appendChild(c);
            return c;
        })();
        const toast = document.createElement('div');
        const colors = { success: '#198754', error: '#dc3545', info: '#0dcaf0', warning: '#ffc107' };
        toast.style.cssText = `background:${colors[type]||colors.info};color:#fff;padding:0.75rem 1.25rem;border-radius:8px;margin-top:0.5rem;font-size:0.9rem;box-shadow:0 4px 12px rgba(0,0,0,0.15);transition:opacity 0.3s;`;
        toast.textContent = message;
        container.appendChild(toast);
        setTimeout(() => { toast.style.opacity = '0'; setTimeout(() => toast.remove(), 300); }, 3000);
    }
};
