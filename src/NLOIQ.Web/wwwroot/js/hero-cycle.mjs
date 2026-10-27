// One signal lap, a smooth circle transformation, a hold, then an infinity return.
export function heroCycle(seconds) {
    const t=((Number.isFinite(seconds)?Math.max(0,seconds):0)%12);
    const ease=x=>{x=Math.max(0,Math.min(1,x));return x*x*(3-2*x);};
    const morph=ease((t-6)/2)*(1-ease((t-10)/2));
    return {morph,travel:Math.min(t/6,1),completion:ease((t-5.5)/.5)*(1-ease((t-10)/2)),phase:t<6?'infinity':t<8?'to-circle':t<10?'circle':'to-infinity'};
}
