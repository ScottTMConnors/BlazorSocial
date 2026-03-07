let observer = null;

export function observe(sentinel, dotNetRef) {
    disconnect();

    observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                observer.disconnect();
                observer = null;
                dotNetRef.invokeMethodAsync('OnSentinelVisible');
            }
        });
    }, { rootMargin: '200px' });

    observer.observe(sentinel);
}

export function disconnect() {
    if (observer) {
        observer.disconnect();
        observer = null;
    }
}
