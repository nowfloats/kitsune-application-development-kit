function compareString(a, b) {

    if (typeof (a) == "string" && typeof (b) == "string") {
        a = a ? a.trim().toLocaleLowerCase() : "";
        b = b ? b.trim().toLocaleLowerCase() : "";
        return a === b;
    }

    return false;
}