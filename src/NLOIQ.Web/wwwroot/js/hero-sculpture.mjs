import { heroCycle } from './hero-cycle.mjs';
// Closed parametric infinity loop, rendered locally with WebGL. No external models or runtime.
export function createSculpture(canvas, isPaused) {
    const scene=canvas.closest('.hero-scene');
    let gl;
    try { gl=canvas.getContext('webgl',{alpha:true,antialias:true,powerPreference:'low-power'}); } catch { return null; }
    if(!gl)return null;
    const vertex=`precision mediump float;attribute vec2 uv;
    uniform float aspect;uniform float time;uniform vec2 pointer;uniform float morph;uniform float glowPass;
    varying vec3 vNormal;varying vec3 vPosition;varying vec2 vUv;
    vec3 center(float s){float t=s*6.2831853;return vec3(mix(1.65,1.18,morph)*cos(t),mix(.82*sin(2.*t),1.18*sin(t),morph),.34*sin(t)*(1.-morph));}
    void main(){vec3 tangent=normalize(center(uv.x+.001)-center(uv.x-.001));
    vec3 reference=abs(tangent.z)<.9?vec3(0.,0.,1.):vec3(0.,1.,0.);
    vec3 n=normalize(cross(tangent,reference));vec3 binormal=normalize(cross(tangent,n));
    vec3 surfaceNormal=n*cos(uv.y*6.2831853)+binormal*sin(uv.y*6.2831853);
    vec3 position=center(uv.x)+surfaceNormal*(.175+glowPass*.1);
    float ay=.14+sin(time*.22)*.1+pointer.x*.12;float az=-.1+sin(time*.16)*.025+pointer.y*.04;
    mat3 ry=mat3(cos(ay),0.,-sin(ay),0.,1.,0.,sin(ay),0.,cos(ay));
    mat3 rz=mat3(cos(az),sin(az),0.,-sin(az),cos(az),0.,0.,0.,1.);
    vec3 p=rz*ry*position;p*=min(1.,aspect/.92);p.y+=sin(time*.52)*.035;
    vPosition=p;vNormal=rz*ry*surfaceNormal;vUv=uv;
    float z=p.z-6.;gl_Position=vec4(p.x*2.65/aspect,p.y*2.65,-1.002*z-.2002,-z);}`;
    const fragment=`precision mediump float;uniform float time;uniform float dark;uniform float travel;uniform float completion;uniform float glowPass;
    varying vec3 vNormal;varying vec3 vPosition;varying vec2 vUv;
    void main(){vec3 n=normalize(vNormal);vec3 view=normalize(vec3(0.,0.,6.)-vPosition);
    float wave=abs(fract(vUv.x-travel+.5)-.5);
    float pulse=1.-smoothstep(.004,.021,wave);
    float trail=exp(-pow(wave/.07,2.));
    vec3 cyan=vec3(.06,.9,1.);vec3 violet=vec3(.4,.22,1.);
    vec3 signal=mix(violet,cyan,exp(-pow(wave/.035,2.)));
    if(glowPass>.5){float facing=pow(max(dot(n,view),0.),1.4);float energy=trail*.48+completion*.09;
    gl_FragColor=vec4(signal,energy*facing);return;}
    vec3 key=normalize(vec3(-3.,4.,5.));vec3 fill=normalize(vec3(3.,-1.,2.));
    float diffuse=max(dot(n,key),0.);float rim=pow(1.-max(dot(n,view),0.),2.8);
    float spec=pow(max(dot(n,normalize(key+view)),0.),65.);
    float sheen=pow(max(dot(n,normalize(fill+view)),0.),22.);
    vec3 pearl=mix(vec3(.8,.86,.98),vec3(.58,.72,.91),dark*.32);
    vec3 color=pearl*(.38+.66*diffuse)+vec3(.68,.85,1.)*spec*1.3+vec3(.08,.4,.8)*sheen*.32;
    color+=vec3(.13,.55,.88)*rim*.38;
    color*=mix(vec3(1.),vec3(.55,.76,1.),.13+.1*sin(vUv.x*6.28318));
    color+=signal*(trail*.3+pulse*.65+completion*.08);
    gl_FragColor=vec4(color,1.);}`;
    function compile(type,source){const shader=gl.createShader(type);gl.shaderSource(shader,source);gl.compileShader(shader);if(!gl.getShaderParameter(shader,gl.COMPILE_STATUS)){gl.deleteShader(shader);throw Error(gl.getShaderInfoLog(shader)||'Sculpture shader unavailable');}return shader;}
    let program;
    try{program=gl.createProgram();const vs=compile(gl.VERTEX_SHADER,vertex),fs=compile(gl.FRAGMENT_SHADER,fragment);gl.attachShader(program,vs);gl.attachShader(program,fs);gl.linkProgram(program);gl.deleteShader(vs);gl.deleteShader(fs);if(!gl.getProgramParameter(program,gl.LINK_STATUS))throw Error(gl.getProgramInfoLog(program)||'Sculpture program unavailable');}catch(error){console.warn('Hero renderer fallback:',error.message);if(program)gl.deleteProgram(program);return null;}
    const uvs=[],indices=[];const segments=200,sides=40;
    // Both shapes share periodic UV topology. The shader rebuilds the tube frame as it morphs.
    for(let i=0;i<=segments;i++)for(let j=0;j<=sides;j++){
        uvs.push(i/segments,j/sides);
        if(i<segments&&j<sides){const q=i*(sides+1)+j;indices.push(q,q+1,q+sides+1,q+1,q+sides+2,q+sides+1);}
    }
    gl.useProgram(program);
    const buffer=gl.createBuffer();gl.bindBuffer(gl.ARRAY_BUFFER,buffer);gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(uvs),gl.STATIC_DRAW);
    const location=gl.getAttribLocation(program,'uv');gl.enableVertexAttribArray(location);gl.vertexAttribPointer(location,2,gl.FLOAT,false,0,0);
    const index=gl.createBuffer();gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,index);gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,new Uint16Array(indices),gl.STATIC_DRAW);
    gl.enable(gl.DEPTH_TEST);gl.disable(gl.CULL_FACE);gl.clearColor(0,0,0,0);
    const uniforms=Object.fromEntries(['aspect','time','pointer','dark','morph','travel','completion','glowPass'].map(n=>[n,gl.getUniformLocation(program,n)]));
    let frame=0,last=0,elapsed=0,visible=true,lost=false;let px=0,py=0;
    function draw(now=0){frame=0;if(lost)return;const running=!isPaused()&&visible&&!document.hidden;if(running&&last)elapsed+=Math.min((now-last)/1000,.05);last=running?now:0;
        const width=canvas.clientWidth,height=canvas.clientHeight;if(!width||!height)return;
        const dpr=Math.min(devicePixelRatio||1,innerWidth<768?1.25:1.75);const w=Math.round(width*dpr),h=Math.round(height*dpr);if(canvas.width!==w||canvas.height!==h){canvas.width=w;canvas.height=h;gl.viewport(0,0,w,h);}
        gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);gl.useProgram(program);gl.uniform1f(uniforms.aspect,width/height);gl.uniform1f(uniforms.time,elapsed);gl.uniform2f(uniforms.pointer,isPaused()?0:px,isPaused()?0:py);gl.uniform1f(uniforms.dark,document.documentElement.dataset.theme==='dark'?1:0);const cycle=heroCycle(elapsed);gl.uniform1f(uniforms.morph,cycle.morph);gl.uniform1f(uniforms.travel,cycle.travel);gl.uniform1f(uniforms.completion,cycle.completion);
        gl.uniform1f(uniforms.glowPass,0);gl.drawElements(gl.TRIANGLES,indices.length,gl.UNSIGNED_SHORT,0);
        gl.enable(gl.BLEND);gl.blendFunc(gl.SRC_ALPHA,gl.ONE);gl.depthMask(false);gl.uniform1f(uniforms.glowPass,1);gl.drawElements(gl.TRIANGLES,indices.length,gl.UNSIGNED_SHORT,0);gl.depthMask(true);gl.disable(gl.BLEND);
        canvas.dataset.rendered='true';canvas.dataset.phase=cycle.phase;
        if(running)frame=requestAnimationFrame(draw);
    }
    function refresh(){if(frame){cancelAnimationFrame(frame);frame=0;}last=0;draw(performance.now());}
    const observer=new IntersectionObserver(entries=>{visible=entries[0].isIntersecting;refresh();},{threshold:.01});observer.observe(scene);
    const resize=new ResizeObserver(refresh);resize.observe(scene);
    const theme=new MutationObserver(refresh);theme.observe(document.documentElement,{attributes:true,attributeFilter:['data-theme']});
    scene.addEventListener('pointermove',event=>{if(isPaused()||event.pointerType==='touch')return;const r=scene.getBoundingClientRect();px=(event.clientX-r.left)/r.width-.5;py=(event.clientY-r.top)/r.height-.5;},{passive:true});scene.addEventListener('pointerleave',()=>{px=py=0;});
    canvas.addEventListener('webglcontextlost',event=>{event.preventDefault();lost=true;if(frame)cancelAnimationFrame(frame);scene.classList.remove('sculpture-ready');});
    canvas.addEventListener('webglcontextrestored',()=>{scene.classList.remove('sculpture-ready');},{once:true});
    document.addEventListener('visibilitychange',refresh);
    scene.classList.add('sculpture-ready');refresh();return{refresh};
}
