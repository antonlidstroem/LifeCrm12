window.lifecrm = {
    downloadFile: function (filename, contentType, byteArray) {
        const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },
    copyToClipboard: function (text) {
        return navigator.clipboard.writeText(text);
    },
    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
};
