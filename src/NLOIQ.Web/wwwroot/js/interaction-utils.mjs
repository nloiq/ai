export function isYouTubeVideoId(value) {
    return typeof value === 'string' && value.length === 11 && /^[A-Za-z0-9_-]+$/.test(value);
}
export function youtubeEmbedUrl(value) {
    if (!isYouTubeVideoId(value)) return null;
    const url = new URL('https://www.youtube-nocookie.com/embed/' + value);
    url.searchParams.set('autoplay', '0');
    url.searchParams.set('playsinline', '1');
    return url.href;
}
export function parallaxOffset(scrollTop, viewportHeight, top, height, amount) {
    if (![scrollTop, viewportHeight, top, height, amount].every(Number.isFinite) || viewportHeight <= 0 || height <= 0) return 0;
    const progress = (scrollTop + viewportHeight / 2 - top - height / 2) / ((viewportHeight + height) / 2);
    return Math.max(-1, Math.min(1, progress)) * Math.min(32, Math.max(0, amount));
}
export function wrappedIndex(index, count) {
    return count > 0 ? ((index % count) + count) % count : 0;
}
