function scrollToTop(id) {
    var elem = document.getElementById(id);
    if (elem) {
        document.getElementById(id).scrollTop = 0;
    }
}