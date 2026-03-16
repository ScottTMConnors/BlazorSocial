export function createScrollObserver() {
    let handler = null;
    let rafId = null;
    let _sentinel = null;
    let _dotNetRef = null;

    function check() {
        rafId = null;
        if (!_sentinel) return;
        const rect = _sentinel.getBoundingClientRect();
        const viewHeight = window.innerHeight || document.documentElement.clientHeight;
        if (rect.top <= viewHeight + 200) {
            const ref = _dotNetRef;
            obs.disconnect();
            ref.invokeMethodAsync('OnSentinelVisible');
        }
    }

    function scheduleCheck() {
        if (!rafId) {
            rafId = requestAnimationFrame(check);
        }
    }

    const obs = {
        observe(sentinel, dotNetRef) {
            obs.disconnect();
            _sentinel = sentinel;
            _dotNetRef = dotNetRef;
            handler = scheduleCheck;

            window.addEventListener('scroll', handler, { passive: true });
            window.addEventListener('resize', handler, { passive: true });
            // Capture phase catches scroll events from *any* element (scroll doesn't bubble)
            document.addEventListener('scroll', handler, { passive: true, capture: true });

            scheduleCheck();
        },
        disconnect() {
            if (handler) {
                window.removeEventListener('scroll', handler);
                window.removeEventListener('resize', handler);
                document.removeEventListener('scroll', handler, { capture: true });
                handler = null;
            }
            if (rafId) {
                cancelAnimationFrame(rafId);
                rafId = null;
            }
            _sentinel = null;
            _dotNetRef = null;
        }
    };

    return obs;
}
