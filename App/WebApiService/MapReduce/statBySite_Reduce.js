function(key, values) {
    var result = {
        count: 0, pcsactpwr: 0, pvactpwr:0, timestamp: new Date().toLocaleString(), accumchg: 0, accumdhg: 0 };

    values.forEach(function (value) {
        result.count += value.count;
        result.pcsactpwr += value.pcsactpwr;
        if ((value.status & 0x04) == 0x04) {
            result.accumchg += value.pcsactpwr;
        }
        if ((value.status & 0x05) == 0x05) {
            result.accumdhg += value.pcsactpwr;
        }
        result.pvactpwr += value.pvactpwr;
        
    });
    result.pcsactpwr = result.pcsactpwr / 3600;
    result.accumdhg = result.accumdhg / 3600;
    result.accumchg = result.accumchg / 3600;
    if (result.pvactpwr != 0)
        result.pvactpwr = result.pvactpwr / 3600;

    return result;
}