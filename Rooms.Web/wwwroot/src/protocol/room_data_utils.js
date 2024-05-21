const decoder = new TextDecoder("utf-8");
const encoder = new TextEncoder("utf-8");

class RoomDataUtils {
    static isBlank(d) {
        return typeof (d) === "number" && (d === 32 || d === 9 || d === 10 || d === 13 || d === 12)
            || typeof (d) === "string" && (d === ' ' || d === '\t' || d === '\n' || d === '\r' || d === '\f');
    }

    static isSign(d) {
        return typeof (d) === "number" && (d === 43 || d === 45)
            || typeof (d) === "string" && (d === '+' || d === '-');

    }

    static isLetter(d) {
        return typeof (d) === "number" && (d === 95 || (d >= 65 && d <= 90) || (d >= 97 && d <= 122))
            || typeof (d) === "string" && (d === '_' || (d >= 'A' && d <= 'Z') || (d >= 'a' && d <= 'z'));
    }

    static isDigit(d) {
        return typeof (d) === "number" && (d >= 48 && d <= 57)
            || typeof (d) === "string" && (d >= '0' && d <= '9');
    }

    static isHexadecimal(d) {
        return typeof (d) === "number" && ((d >= 48 && d <= 57) || (d >= 65 && d <= 70) || (d >= 97 && d <= 102))
            || typeof (d) === "string" && ((d >= '0' && d <= '9') || (d >= 'A' && d <= 'F') || (d >= 'a' && d <= 'f'));
    }

    static scanWord(data, min = 1, max = Number.MAX_VALUE) {
        let index = 0;
        while (index < data.length && this.isLetter(data[index])) {
            index++;
            if (index > max)
                return 0;
        }
        if (index < min)
            return 0;
        return index;
    }

    static scanDigit(data, min = 1, max = Number.MAX_VALUE) {
        let index = 0;
        while (index < data.length && this.isDigit(data[index])) {
            index++;
            if (index > max)
                return 0;
        }
        if (index < min)
            return 0;
        return index;
    }

    static scanHexadecimal(data, min = 1, max = Number.MAX_VALUE) {
        let index = 0;
        while (index < data.length && this.isHexadecimal(data[index])) {
            index++;
            if (index > max)
                return 0;
        }
        if (index < min)
            return 0;
        return index;
    }

    static slice(data, index, length = null) {
        if (data instanceof Uint8Array) return data.subarray(index, length ? index + length : data.length);
        if (typeof (data) === "string") return data.substring(index, length ? index + length : data.length);
        throw new Error("Unsupported data type");
    }

}

export {
    decoder,
    encoder,
    RoomDataUtils
}