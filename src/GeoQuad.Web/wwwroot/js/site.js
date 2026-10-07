// GeoQuad – script dùng chung (PHẦN 0).

// Nạp KaTeX auto-render cho $...$ và $$...$$ (NFR-12).
window.addEventListener('DOMContentLoaded', function () {
    if (typeof renderMathInElement !== 'function') {
        return;
    }

    renderMathInElement(document.body, {
        delimiters: [
            { left: '$$', right: '$$', display: true },
            { left: '$', right: '$', display: false },
            { left: '\[', right: '\]', display: true },
            { left: '\(', right: '\)', display: false }
        ],
        throwOnError: false
    });
});
