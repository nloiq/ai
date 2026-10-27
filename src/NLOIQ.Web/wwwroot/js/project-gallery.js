'use strict';
(() => {
 const gallery=document.querySelector('[data-project-gallery]');if(!gallery)return;
 const items=[...gallery.querySelectorAll('[data-gallery-item]')];if(!items.length)return;
 const main=gallery.querySelector('[data-gallery-main]'),open=gallery.querySelector('[data-gallery-open]'),caption=gallery.querySelector('[data-gallery-caption]'),position=gallery.querySelector('[data-gallery-position]');
 const dialog=gallery.querySelector('#project-image-dialog'),large=gallery.querySelector('[data-gallery-dialog-image]'),largeCaption=gallery.querySelector('[data-gallery-dialog-caption]'),largePosition=gallery.querySelector('[data-gallery-dialog-position]');
 const previous=gallery.querySelector('[data-gallery-previous]'),next=gallery.querySelector('[data-gallery-next]'),close=gallery.querySelector('[data-gallery-close]');
 const format=new Intl.NumberFormat(document.documentElement.lang||'en');const rtl=document.documentElement.dir==='rtl';let current=0,opener=null,wasLocked=false,backdropDown=false;
 function render(index){
  current=(index+items.length)%items.length;const data=items[current].dataset;
  main.src=data.url;main.alt=data.alt||'';main.width=Number(data.width);main.height=Number(data.height);
  if(data.srcset)main.srcset=data.srcset;else main.removeAttribute('srcset');open.href=data.url;
  caption.textContent=data.caption||'';caption.hidden=!data.caption;
  const count=format.format(current+1)+' / '+format.format(items.length);position.textContent=count;largePosition.textContent=count;
  for(const [i,item]of items.entries())item.setAttribute('aria-pressed',String(i===current));
  if(dialog.open){large.src=data.url;large.alt=data.alt||'';large.width=Number(data.width);large.height=Number(data.height);largeCaption.textContent=data.caption||'';largeCaption.hidden=!data.caption;}
 }
 items.forEach((item,index)=>{item.setAttribute('role','button');item.setAttribute('aria-controls','project-gallery-stage');item.addEventListener('click',event=>{event.preventDefault();render(index);});item.addEventListener('keydown',event=>{if(event.key===' '){event.preventDefault();item.click();}});});
 gallery.addEventListener('keydown',event=>{if(!event.target.closest('[data-gallery-item]'))return;let index=current;if(event.key==='ArrowRight')index+=rtl?-1:1;else if(event.key==='ArrowLeft')index+=rtl?1:-1;else if(event.key==='Home')index=0;else if(event.key==='End')index=items.length-1;else return;event.preventDefault();render(index);items[current].focus();});
 previous.hidden=next.hidden=items.length<2;
 open.addEventListener('click',event=>{if(typeof dialog.showModal!=='function')return;event.preventDefault();opener=open;wasLocked=document.documentElement.classList.contains('overflow-hidden');document.documentElement.classList.add('overflow-hidden');dialog.showModal();render(current);close.focus();});
 close.addEventListener('click',()=>dialog.close());previous.addEventListener('click',()=>render(current-1));next.addEventListener('click',()=>render(current+1));
 dialog.addEventListener('keydown',event=>{if(event.key==='ArrowRight'||event.key==='ArrowLeft'){event.preventDefault();const forward=event.key==='ArrowRight'?!rtl:rtl;render(current+(forward?1:-1));}if(event.key!=='Tab')return;const controls=[...dialog.querySelectorAll('button')].filter(x=>!x.hidden&&!x.disabled);const first=controls[0],last=controls.at(-1);if(event.shiftKey&&document.activeElement===first){event.preventDefault();last.focus();}else if(!event.shiftKey&&document.activeElement===last){event.preventDefault();first.focus();}});
 function outside(event){const rect=dialog.getBoundingClientRect();return event.target===dialog&&(event.clientX<rect.left||event.clientX>rect.right||event.clientY<rect.top||event.clientY>rect.bottom);}
 dialog.addEventListener('pointerdown',event=>{backdropDown=outside(event);});dialog.addEventListener('click',event=>{if(backdropDown&&outside(event))dialog.close();backdropDown=false;});
 dialog.addEventListener('cancel',event=>{event.preventDefault();dialog.close();});dialog.addEventListener('close',()=>{large.removeAttribute('src');large.alt='';if(!wasLocked)document.documentElement.classList.remove('overflow-hidden');if(opener?.isConnected)opener.focus({preventScroll:true});});
 render(0);
})();
