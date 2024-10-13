function setDarkModePreference(isDarkMode) {
    document.cookie = `darkMode=${isDarkMode}; path=/; expires=${new Date(new Date().getTime() + 365 * 24 * 60 * 60 * 1000).toUTCString()}`;

    document.body.classList.toggle('dark', isDarkMode);

    window.location.reload();
}

function getCookie(name) {
    let matches = document.cookie.match(new RegExp(
        "(?:^|; )" + name.replace(/([.$?*|{}()[]\\\/+^])/g, '\\$1') + "=([^;]*)"
    ));
    return matches ? decodeURIComponent(matches[1]) : undefined;
    console.log("triggered")
}

window.onload = function () {
    const darkModeCookie = getCookie('darkMode');
    const isDarkMode = darkModeCookie === 'true';

    if (isDarkMode) {
        document.body.classList.add('dark');
        document.getElementById('darkModeToggle').checked = true;
    }
};