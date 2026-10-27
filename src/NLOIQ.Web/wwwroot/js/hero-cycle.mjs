// Continuous signal motion: one infinity lap, a circle lap, then the infinity return.
export function heroCycle(seconds) {
    const t=((Number.isFinite(seconds)?Math.max(0,seconds):0)%12);
    const ease=x=>{x=Math.max(0,Math.min(1,x));return x*x*(3-2*x);};
    const morph=ease((t-6)/2)*(1-ease((t-10)/2));
    // Half a lap in each transition and a complete lap while circular.
    // Three laps per cycle makes the reset seamless on the periodic surface.
    const laps=t<=6?t/6:t<8?1+(t-6)/4:t<10?1.5+(t-8)/2:2.5+(t-10)/4;
    const travel=t<=6?laps:laps%1;
    return {morph,travel,completion:ease((t-5.5)/.5)*(1-ease((t-10)/2)),phase:t<6?'infinity':t<8?'to-circle':t<10?'circle':'to-infinity'};
}
