'use strict';
const root = document.documentElement;
const themeControl = document.getElementById('theme');
const systemTheme = matchMedia('(prefers-color-scheme:dark)');
let preference = 'system';
try { preference = localStorage.getItem('nlo-design-theme') || 'system'; } catch {}
if (!['light','dark','system'].includes(preference)) preference = 'system';
themeControl.value = preference;
function applyTheme() { root.dataset.theme = preference === 'system' ? (systemTheme.matches ? 'dark' : 'light') : preference; }
applyTheme();
themeControl.addEventListener('change', () => { preference = themeControl.value; try { localStorage.setItem('nlo-design-theme', preference); } catch {} applyTheme(); });
systemTheme.addEventListener('change', applyTheme);
const concepts = [...document.querySelectorAll('[data-concept]')];
for (const button of concepts) button.addEventListener('click', () => {
  for (const item of concepts) { const selected = item === button; item.setAttribute('aria-pressed', String(selected)); item.classList.toggle('bg-ink',selected); item.classList.toggle('text-paper',selected); item.classList.toggle('border-ink',selected); item.classList.toggle('border-line',!selected); document.getElementById('concept-'+item.dataset.concept).hidden = !selected; }
});
const english = new Map([...document.querySelectorAll('[data-copy]')].map(el => [el,el.innerHTML]));
const arabic = {
reviewLabel:'المرحلة الأولى · اتجاهان للمراجعة · رسومات توضيحية', appearance:'المظهر', conceptA:'٠١ / الملف النباتي', conceptB:'٠٢ / حقل الخريف', viewSystem:'استعراض نظام التصميم ↓',logo:'موضع الشعار', about:'من نحن',projects:'المشاريع',services:'الخدمات',contact:'تواصل معنا ↖',eyebrow:'للطبيعة نظام. ونحن نهندس نظامك.',aTitle:'نهندس ما<br>لا يُرى.<br><em class="text-accent">ونصل ما</em><br>يهمّك.',intro:'تقدّم NLO IQ أنظمة تيار خفيف ذكية وحلولاً كهربائية وبرمجيات هندسية مصمّمة لربط البيئات المعقّدة ببعضها.',explore:'استكشف أعمالنا ↖',discover:'تعرّف على NLO IQ',artNote:'رسم تصوّري · ليس مشروعاً للشركة',aboutLabel:'٠١ / الفكرة',aboutTitle:'حيث تلتقي الدقّة<br>بالإمكانات.',aboutNote:'مقدّمة تحريرية تجمع النص المعتمد للشركة مع دراسة نباتية هندسية. يُضاف المحتوى النهائي قبل النشر.',workLabel:'٠٢ / أعمال مختارة — دراسة تخطيط',projectPlaceholder:'موضع صورة<br>المشروع',projectNote:'تفاصيل المشاريع الحقيقية ووسائطها بانتظار الاعتماد.',serviceLabel:'٠٣ / دراسة تفاعل الخدمات',serviceRow:'٠١ — أنظمة التيار الخفيف',serviceNote:'فئة مقترحة لمراجعة التصميم. يحتاج نطاق الخدمة الفعلي إلى تأكيد الشركة.',contactTitle:'هل تفكّر في نظام جديد؟',contactNote:'تُضاف بيانات التواصل المؤكّدة هنا.',bEyebrow:'NLO IQ / تناغم الأنظمة',bTitle:'للطبيعة<br>نظام.<br><em class="text-gold">ونحن نهندس نظامك.</em>',landscapeNote:'حقل الخريف / رسم تصوّري',fieldFooter:'بنية عضوية. روابط مدروسة.',scroll:'اكتشف الفكرة ↓',systemLabel:'الأساس المشترك / المرحلة الأولى',systemTitle:'مفردات الخريف.',typeSample:'دقّة بإيقاع إنساني.',typeNote:'حروف إنجليزية تحريرية وأشكال عربية واضحة مع تباعد مريح بين السطور. تستخدم المراجعة خطوط النظام؛ يبقى اختيار الخطوط المرخّصة قراراً تصميمياً.',controlsLabel:'عناصر التحكّم / الحالات',primary:'إجراء رئيسي ↖',disabled:'غير متاح',spacing:'التباعد: ٤ / ٨ / ١٢ / ١٦ / ٢٤ / ٣٢ / ٤٨ / ٦٤ / ٩٦. زوايا واضحة وخطوط دقيقة ومؤشّرات تركيز ظاهرة.',footer:'NLO IQ · دراسة تصميم للمرحلة الأولى · ليست نسخة الإنتاج'
};
const languageButton = document.getElementById('language');
languageButton.addEventListener('click', () => {
  const ar = root.lang !== 'ar'; root.lang = ar ? 'ar' : 'en'; root.dir = ar ? 'rtl' : 'ltr';
  for (const [element,original] of english) element.innerHTML = ar ? (arabic[element.dataset.copy] || original) : original;
  languageButton.textContent = ar ? 'English' : 'العربية'; languageButton.lang = ar ? 'en' : 'ar';
  const labels = ar ? ['النظام','فاتح','داكن'] : ['System','Light','Dark']; [...themeControl.options].forEach((option,index) => option.textContent = labels[index]);
  document.getElementById('sample-feedback').textContent = '';
});
document.getElementById('sample-action').addEventListener('click', () => { document.getElementById('sample-feedback').textContent = root.lang === 'ar' ? 'معاينة لحالة التفاعل فقط؛ لا يتم إرسال أي بيانات.' : 'Interaction state preview only; no information is submitted.'; });
document.getElementById('year').textContent = new Date().getFullYear();
