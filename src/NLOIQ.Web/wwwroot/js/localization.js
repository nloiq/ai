'use strict';
// Keep native constraint validation while supplying messages in the chosen site language.
document.addEventListener('invalid',event=>{
 const field=event.target;if(!field.validity)return;
 const validity=field.validity;let message='';
 if(validity.valueMissing)message=field.dataset.valRequired||document.body.dataset.validationRequired;
 else if(validity.typeMismatch)message=field.dataset.valEmail||document.body.dataset.validationInvalid;
 else if(validity.patternMismatch)message=field.dataset.valRegex||document.body.dataset.validationInvalid;
 else if(validity.tooLong)message=field.dataset.valLength||document.body.dataset.validationInvalid;
 else if(validity.rangeOverflow||validity.rangeUnderflow)message=field.dataset.valRange||document.body.dataset.validationInvalid;
 else message=document.body.dataset.validationInvalid;
 field.setCustomValidity(message||'');
},true);
document.addEventListener('input',event=>event.target.setCustomValidity?.(''));
document.addEventListener('change',event=>event.target.setCustomValidity?.(''));
