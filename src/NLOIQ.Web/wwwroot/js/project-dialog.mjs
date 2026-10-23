import { youtubeEmbedUrl, wrappedIndex } from './interaction-utils.mjs';
const dialog = document.getElementById('project-dialog');
const projects = [...document.querySelectorAll('[data-project]')];
if (dialog && typeof dialog.showModal === 'function' && projects.length) {
    const media = document.getElementById('dialog-media');
    const title = document.getElementById('dialog-title');
    const description = document.getElementById('dialog-description');
    const note = document.getElementById('dialog-video-note');
    const position = document.getElementById('dialog-position');
    const closeButton = document.getElementById('dialog-close');
    let current = 0;
    let opener = null;
    let wasScrollLocked = false;
    function clearMedia() {
        // Removing the browsing context stops playback on navigation and close.
        media.replaceChildren();
    }
    function render() {
        clearMedia();
        document.getElementById('dialog-content').scrollTop = 0;
        const project = projects[current];
        title.textContent = project.querySelector('[data-project-title]').textContent;
        description.textContent = project.querySelector('[data-project-description]').textContent;
        position.textContent = new Intl.NumberFormat(document.documentElement.lang).format(current + 1) + ' / ' + new Intl.NumberFormat(document.documentElement.lang).format(projects.length);
        const embed = youtubeEmbedUrl(project.dataset.videoId);
        if (embed) {
            const iframe = document.createElement('iframe');
            iframe.src = embed;
            iframe.title = dialog.dataset.videoTitle + ': ' + title.textContent;
            iframe.className = 'h-full w-full border-0';
            iframe.allow = 'encrypted-media; fullscreen; picture-in-picture';
            iframe.allowFullscreen = true;
            iframe.referrerPolicy = 'strict-origin-when-cross-origin';
            media.append(iframe);
            note.textContent = dialog.dataset.videoNote;
        } else {
            const image = project.querySelector('img').cloneNode();
            image.className = 'h-full w-full object-contain';
            image.style.transform = '';
            image.removeAttribute('data-parallax-layer');
            image.loading = 'eager';
            media.append(image);
            note.textContent = dialog.dataset.noVideoNote;
        }
    }
    function close() { if (dialog.open) dialog.close(); }
    for (const project of projects) {
        for (const trigger of project.querySelectorAll('[data-project-open]')) {
            trigger.hidden = false;
            trigger.addEventListener('click', event => {
                event.preventDefault();
                current = projects.indexOf(project);
                opener = trigger;
                render();
                wasScrollLocked = document.documentElement.classList.contains('overflow-hidden');
                document.documentElement.classList.add('overflow-hidden');
                dialog.showModal();
                closeButton.focus();
            });
        }
    }
    // Explicitly wrap at the panel boundaries; native modality keeps the rest of the page inert.
    dialog.addEventListener('keydown', event => {
        if (event.key !== 'Tab') return;
        const controls = [...dialog.querySelectorAll('button:not([disabled]), a[href], iframe, input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex="0"]')].filter(node => node.getClientRects().length);
        const first = controls[0], last = controls.at(-1);
        if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
        else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
    });
    closeButton.addEventListener('click', close);
    document.getElementById('dialog-previous').addEventListener('click', () => { current = wrappedIndex(current - 1, projects.length); render(); });
    document.getElementById('dialog-next').addEventListener('click', () => { current = wrappedIndex(current + 1, projects.length); render(); });
    // Native showModal makes the background inert and confines keyboard focus.
    // A backdrop pointerdown and click must both occur outside the panel.
    let backdropDown = false;
    function isBackdrop(event) {
        const bounds = dialog.getBoundingClientRect();
        return event.target === dialog && (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom);
    }
    dialog.addEventListener('pointerdown', event => { backdropDown = isBackdrop(event); });
    dialog.addEventListener('click', event => { if (backdropDown && isBackdrop(event)) close(); backdropDown = false; });
    dialog.addEventListener('cancel', event => { event.preventDefault(); close(); });
    dialog.addEventListener('close', () => {
        clearMedia();
        if (!wasScrollLocked) document.documentElement.classList.remove('overflow-hidden');
        if (opener?.isConnected) opener.focus({ preventScroll: true });
    });
}
