// Closed parametric infinity loop, rendered locally with WebGL. No external models or runtime.
export function createSculpture(canvas, isPaused) {
    const scene=canvas.closest('.hero-scene');
    let gl;
    try { gl=canvas.getContext('webgl',{alpha:true,antialias:true,powerPreference:'low-power'}); } catch { return null; }
    if(!gl)return null;
    const vertex=`precision mediump float;attribute vec3 position;attribute vec3 normal;attribute vec2 uv;
    uniform float aspect;uniform float time;uniform vec2 pointer;
    varying vec3 vNormal;varying vec3 vPosition;varying vec2 vUv;
    void main(){float ay=.14+sin(time*.22)*.1+pointer.x*.12;float az=-.1+sin(time*.16)*.025+pointer.y*.04;
    mat3 ry=mat3(cos(ay),0.,-sin(ay),0.,1.,0.,sin(ay),0.,cos(ay));
    mat3 rz=mat3(cos(az),sin(az),0.,-sin(az),cos(az),0.,0.,0.,1.);
    vec3 p=rz*ry*position;p*=min(1.,aspect/.92);p.y+=sin(time*.52)*.035;
    vPosition=p;vNormal=rz*ry*normal;vUv=uv;
    float z=p.z-6.;gl_Position=vec4(p.x*2.65/aspect,p.y*2.65,-1.002*z-.2002,-z);}`;
    const fragment=`precision mediump float;uniform float time;uniform float dark;
    varying vec3 vNormal;varying vec3 vPosition;varying vec2 vUv;
    void main(){vec3 n=normalize(vNormal);vec3 view=normalize(vec3(0.,0.,6.)-vPosition);
    vec3 key=normalize(vec3(-3.,4.,5.));vec3 fill=normalize(vec3(3.,-1.,2.));
    float diffuse=max(dot(n,key),0.);float rim=pow(1.-max(dot(n,view),0.),2.8);
    float spec=pow(max(dot(n,normalize(key+view)),0.),65.);
    float sheen=pow(max(dot(n,normalize(fill+view)),0.),22.);
    vec3 pearl=mix(vec3(.8,.86,.98),vec3(.58,.72,.91),dark*.32);
    vec3 color=pearl*(.38+.66*diffuse)+vec3(.68,.85,1.)*spec*1.3+vec3(.08,.4,.8)*sheen*.32;
    color+=vec3(.13,.55,.88)*rim*.38;
    // A continuous cool-metal finish keeps the loop visually unbroken.
    color*=mix(vec3(1.),vec3(.55,.76,1.),.13+.1*sin(vUv.x*6.28318));
    float wave=abs(fract(vUv.x-time*.065+.5)-.5);
    float pulse=1.-smoothstep(.002,.014,wave);
    color+=vec3(.22,.86,.96)*pulse*.5;
    gl_FragColor=vec4(color,1.);}`;
    function compile(type,source){const shader=gl.createShader(type);gl.shaderSource(shader,source);gl.compileShader(shader);if(!gl.getShaderParameter(shader,gl.COMPILE_STATUS)){gl.deleteShader(shader);throw Error(gl.getShaderInfoLog(shader)||'Sculpture shader unavailable');}return shader;}
    let program;
    try{program=gl.createProgram();const vs=compile(gl.VERTEX_SHADER,vertex),fs=compile(gl.FRAGMENT_SHADER,fragment);gl.attachShader(program,vs);gl.attachShader(program,fs);gl.linkProgram(program);gl.deleteShader(vs);gl.deleteShader(fs);if(!gl.getProgramParameter(program,gl.LINK_STATUS))throw Error(gl.getProgramInfoLog(program)||'Sculpture program unavailable');}catch(error){console.warn('Hero renderer fallback:',error.message);if(program)gl.deleteProgram(program);return null;}
    const positions=[],normals=[],uvs=[],indices=[];const segments=200,sides=40;
    // Periodic centerline and tangent close the seam. Depth separates the crossing.
    const center=s=>{const t=s*Math.PI*2;return[1.65*Math.cos(t),.82*Math.sin(2*t),.34*Math.sin(t)];};
    const normalize=v=>{const l=Math.hypot(...v)||1;return v.map(n=>n/l)};
    const cross=(a,b)=>[a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]];
    for(let i=0;i<=segments;i++){const s=i/segments,c=center(s),a=center(s-.001),b=center(s+.001),t=normalize(b.map((v,k)=>v-a[k]));const n=normalize(cross(t,[0,0,1])),bin=normalize(cross(t,n));
        for(let j=0;j<=sides;j++){const angle=j/sides*Math.PI*2;const normal=n.map((v,k)=>v*Math.cos(angle)+bin[k]*Math.sin(angle));const radius=.175;positions.push(...c.map((v,k)=>v+normal[k]*radius));normals.push(...normal);uvs.push(s,j/sides);
            if(i<segments&&j<sides){const q=i*(sides+1)+j;indices.push(q,q+1,q+sides+1,q+1,q+sides+2,q+sides+1);}}
    }
    gl.useProgram(program);
    const buffers=[];
    for(const[name,data,size]of [['position',positions,3],['normal',normals,3],['uv',uvs,2]]){const buffer=gl.createBuffer();buffers.push(buffer);gl.bindBuffer(gl.ARRAY_BUFFER,buffer);gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(data),gl.STATIC_DRAW);const location=gl.getAttribLocation(program,name);gl.enableVertexAttribArray(location);gl.vertexAttribPointer(location,size,gl.FLOAT,false,0,0);}
    const index=gl.createBuffer();buffers.push(index);gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,index);gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,new Uint16Array(indices),gl.STATIC_DRAW);
    gl.enable(gl.DEPTH_TEST);gl.disable(gl.CULL_FACE);gl.clearColor(0,0,0,0);
    const uniforms=Object.fromEntries(['aspect','time','pointer','dark'].map(n=>[n,gl.getUniformLocation(program,n)]));
    let frame=0,last=0,elapsed=0,visible=true,lost=false;let px=0,py=0;
    function draw(now=0){frame=0;if(lost)return;const running=!isPaused()&&visible&&!document.hidden;if(running&&last)elapsed+=Math.min((now-last)/1000,.05);last=running?now:0;
        const width=canvas.clientWidth,height=canvas.clientHeight;if(!width||!height)return;
        const dpr=Math.min(devicePixelRatio||1,innerWidth<768?1.25:1.75);const w=Math.round(width*dpr),h=Math.round(height*dpr);if(canvas.width!==w||canvas.height!==h){canvas.width=w;canvas.height=h;gl.viewport(0,0,w,h);}
        gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);gl.useProgram(program);gl.uniform1f(uniforms.aspect,width/height);gl.uniform1f(uniforms.time,elapsed);gl.uniform2f(uniforms.pointer,isPaused()?0:px,isPaused()?0:py);gl.uniform1f(uniforms.dark,document.documentElement.dataset.theme==='dark'?1:0);gl.drawElements(gl.TRIANGLES,indices.length,gl.UNSIGNED_SHORT,0);canvas.dataset.rendered='true';
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
