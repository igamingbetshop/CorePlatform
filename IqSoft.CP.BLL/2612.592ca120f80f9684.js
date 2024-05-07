"use strict";(self.webpackChunkAgentManagmentSystem=self.webpackChunkAgentManagmentSystem||[]).push([[2612],{2612:(y,c,a)=>{a.r(c),a.d(c,{CancelPaymentModalComponent:()=>M});var u=a(1368),n=a(6504),C=a(7816),s=a(2864),r=a(4060),_=a(1560),d=a(8416),t=a(2116),f=a(748),E=a(816),P=a(8516);const p=["autosize"];let M=(()=>{class m{constructor(l,e,o,i,g){this.dialogRef=l,this.fb=e,this.snackbarService=o,this.data=i,this.paymentsService=g,this.createForm()}ngOnInit(){this.id=this.data.id,this.formGroup.get("PaymentRequestId")?.setValue(this.id)}createForm(){this.formGroup=this.fb.group({PaymentRequestId:[null],Comment:[null]})}close(){this.dialogRef.close()}onSubmit(){this.paymentsService.rejectPaymentRequest(this.formGroup.value).subscribe(e=>{0===e.ResponseCode?(this.snackbarService.showSuccess("Payment has been canceled"),this.dialogRef.close()):this.snackbarService.showError(e.Description)})}static#t=this.\u0275fac=function(e){return new(e||m)(t.GI1(s.yI),t.GI1(n.KE),t.GI1(f.i),t.GI1(s.sR),t.GI1(E.E))};static#a=this.\u0275cmp=t.In1({type:m,selectors:[["app-cancel-payment-modal"]],viewQuery:function(e,o){if(1&e&&t.CC$(p,5),2&e){let i;t.wto(i=t.Gqi())&&(o.autosize=i.first)}},standalone:!0,features:[t.UHJ],decls:19,vars:2,consts:[[1,"dialog-container"],["mat-dialog-title","",1,"mat-dialog-title"],[1,"title"],["alt","icon",1,"icon",3,"click"],[1,"modal-body"],[1,"modal-form",3,"formGroup"],[1,"form-row"],[1,"mat-form-field"],["matInput","","cdkTextareaAutosize","","formControlName","Comment"],["autosize","cdkTextareaAutosize"],["mat-dialog-actions","",1,"mat-dialog-actions"],[1,"modal-cancel-btn",3,"click"],["type","submit",1,"modal-primary-btn",3,"disabled","click"]],template:function(e,o){1&e&&(t.I0R(0,"div",0)(1,"div",1)(2,"span",2),t.OEk(3,"Please give a brief reason for the cancellation"),t.C$Y(),t.I0R(4,"mat-icon",3),t.qCj("click",function(){return o.close()}),t.OEk(5,"close"),t.C$Y()(),t.I0R(6,"div",4)(7,"form",5)(8,"div",6)(9,"mat-form-field",7)(10,"mat-label"),t.OEk(11,"Info"),t.C$Y(),t.wR5(12,"textarea",8,9),t.C$Y()()()(),t.I0R(14,"div",10)(15,"button",11),t.qCj("click",function(){return o.close()}),t.OEk(16,"Cancel"),t.C$Y(),t.I0R(17,"button",12),t.qCj("click",function(){return o.onSubmit()}),t.OEk(18,"Submit"),t.C$Y()()()),2&e&&(t.yG2(7),t.E7m("formGroup",o.formGroup),t.yG2(10),t.E7m("disabled",o.formGroup.invalid))},dependencies:[u.MD,n.y,n.sz,n.ot,n.ue,n.u,_.oB,_.qL,n.sl,n.uW,n.Wo,r.wb,r.Up,r.w5,d.cN,d.yi,P.Qn,C.oJ,s.sr,s.WQ,s.Yp],styles:[".modal-body[_ngcontent-%COMP%]{display:grid;grid-template-columns:repeat(1fr)}.modal-body[_ngcontent-%COMP%]   .form-row[_ngcontent-%COMP%], .modal-body[_ngcontent-%COMP%]   .form-one-row[_ngcontent-%COMP%]{display:grid;grid-template-columns:1fr;gap:20px;width:340px}"]})}return m})()}}]);