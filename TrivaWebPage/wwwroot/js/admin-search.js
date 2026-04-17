(function () {
    const root = document.getElementById("adminSearchRoot");
    const input = document.getElementById("adminSearchInput");
    const panel = document.getElementById("adminSearchPanel");
    if (!root || !input || !panel) {
        return;
    }

    const baseUrl = root.dataset.searchUrl || "/AdminSearch/Quick";
    let timer = 0;
    let abortController = null;

    function hidePanel() {
        panel.classList.add("hidden");
        panel.innerHTML = "";
    }

    function renderResults(items) {
        panel.innerHTML = "";
        if (!items.length) {
            const empty = document.createElement("div");
            empty.className =
                "px-3 py-2 text-sm text-brand-lightMuted dark:text-brand-darkMuted";
            empty.textContent = "Sonuç yok";
            panel.appendChild(empty);
            panel.classList.remove("hidden");
            return;
        }

        for (let idx = 0; idx < items.length; idx++) {
            const item = items[idx];
            const row = document.createElement("div");
            row.className =
                "admin-search-row px-3 py-2 cursor-default border-b border-brand-lightBorder dark:border-brand-darkBorder last:border-0 hover:bg-black/5 dark:hover:bg-white/5 rounded-none";
            row.dataset.url = item.url;

            const title = document.createElement("div");
            title.className =
                "text-sm font-medium text-brand-lightText dark:text-brand-darkText";
            title.textContent =
                (item.kind === "page" ? "Sayfa: " : "İşlem: ") + item.title;

            const sub = document.createElement("div");
            sub.className =
                "text-xs text-brand-lightMuted dark:text-brand-darkMuted mt-0.5";
            if (item.kind === "action") {
                sub.textContent = "Sayfalar: " + (item.subtitle || "");
            } else {
                sub.textContent = "Slug: " + (item.subtitle || "");
            }

            row.appendChild(title);
            row.appendChild(sub);

            row.addEventListener("click", function () {
                panel.querySelectorAll(".admin-search-row").forEach(function (el) {
                    el.classList.remove("bg-black/10", "dark:bg-white/10");
                });
                row.classList.add("bg-black/10", "dark:bg-white/10");
            });

            row.addEventListener("dblclick", function () {
                if (item.url) {
                    window.location.href = item.url;
                }
            });

            panel.appendChild(row);
        }

        panel.classList.remove("hidden");
    }

    function runSearch() {
        const q = input.value.trim();
        if (q.length === 0) {
            hidePanel();
            return;
        }

        if (abortController) {
            abortController.abort();
        }
        abortController = new AbortController();

        fetch(baseUrl + "?q=" + encodeURIComponent(q), {
            headers: { Accept: "application/json" },
            signal: abortController.signal
        })
            .then(function (res) {
                if (!res.ok) {
                    throw new Error("bad status");
                }
                return res.json();
            })
            .then(renderResults)
            .catch(function (err) {
                if (err.name === "AbortError") {
                    return;
                }
                panel.innerHTML = "";
                const fail = document.createElement("div");
                fail.className = "px-3 py-2 text-sm text-red-600 dark:text-red-400";
                fail.textContent = "Arama başarısız";
                panel.appendChild(fail);
                panel.classList.remove("hidden");
            });
    }

    input.addEventListener("input", function () {
        window.clearTimeout(timer);
        timer = window.setTimeout(runSearch, 280);
    });

    input.addEventListener("focus", function () {
        if (input.value.trim().length > 0) {
            window.clearTimeout(timer);
            timer = window.setTimeout(runSearch, 0);
        }
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            hidePanel();
        }
    });

    document.addEventListener("mousedown", function (e) {
        if (!root.contains(e.target)) {
            hidePanel();
        }
    });
})();
