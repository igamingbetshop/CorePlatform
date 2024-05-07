"use strict";(self.webpackChunkAgentManagmentSystem=self.webpackChunkAgentManagmentSystem||[]).push([[6308],{6308:(B,_,o)=>{o.r(_),o.d(_,{AddEditAnnouncementComponent:()=>K});var u=o(1368),s=o(6504),h=o(7816),c=o(4060),p=o(1560),f=o(8416),C=o(2096),l=o(2864),y=o(3992),E=o(3840),I=o(2064),e=o(2116),g=o(8196),v=o(2236),A=o(5068),R=o(748),T=o(3576);function G(t,r){1&t&&(e.I0R(0,"span",15),e.OEk(1),e.wVc(2,"translate"),e.C$Y()),2&t&&(e.yG2(),e.cNF(e.kDX(2,1,"Common.AddAnnouncement")))}function O(t,r){1&t&&(e.I0R(0,"span",15),e.OEk(1),e.wVc(2,"translate"),e.C$Y()),2&t&&(e.yG2(),e.cNF(e.kDX(2,1,"Common.EditAnnouncement")))}function D(t,r){1&t&&(e.I0R(0,"mat-error"),e.OEk(1),e.wVc(2,"translate"),e.C$Y()),2&t&&(e.yG2(),e.oRS(" ",e.kDX(2,1,"Errors.TheFieldIsRequired")," "))}function M(t,r){if(1&t&&(e.I0R(0,"div",8)(1,"mat-form-field",9)(2,"mat-label"),e.OEk(3),e.wVc(4,"translate"),e.C$Y(),e.wR5(5,"input",16),e.yuY(6,D,3,3,"mat-error"),e.C$Y()()),2&t){const n=e.GaO();e.yG2(3),e.cNF(e.kDX(4,3,"Common.NickName")),e.yG2(2),e.E7m("placeholder","NickName"),e.yG2(),e.C0Y(6,n.nickNameControl.hasError("required")?6:-1)}}function k(t,r){if(1&t&&(e.I0R(0,"mat-option",18),e.OEk(1),e.C$Y()),2&t){const n=r.$implicit;e.E7m("value",n.Id),e.yG2(),e.cNF(n.Name)}}function N(t,r){1&t&&(e.I0R(0,"mat-error"),e.OEk(1),e.wVc(2,"translate"),e.C$Y()),2&t&&(e.yG2(),e.oRS(" ",e.kDX(2,1,"Errors.TheFieldIsRequired")," "))}function Y(t,r){if(1&t&&(e.I0R(0,"mat-form-field",9)(1,"mat-label"),e.OEk(2),e.wVc(3,"translate"),e.C$Y(),e.I0R(4,"mat-select",17),e.c53(5,k,2,2,"mat-option",18,e.wJt),e.C$Y(),e.yuY(7,N,3,3,"mat-error"),e.C$Y()),2&t){const n=e.GaO();e.yG2(2),e.cNF(e.kDX(3,2,"Common.Type")),e.yG2(3),e.oho(n.types),e.yG2(2),e.C0Y(7,n.typeControl.hasError("required")?7:-1)}}function P(t,r){if(1&t&&(e.I0R(0,"mat-option",18),e.OEk(1),e.C$Y()),2&t){const n=r.$implicit;e.E7m("value",n.Id),e.yG2(),e.cNF(n.Name)}}function S(t,r){1&t&&(e.I0R(0,"mat-error"),e.OEk(1),e.wVc(2,"translate"),e.C$Y()),2&t&&(e.yG2(),e.oRS(" ",e.kDX(2,1,"Errors.TheFieldIsRequired")," "))}function U(t,r){if(1&t&&(e.I0R(0,"mat-form-field",9)(1,"mat-label"),e.OEk(2),e.wVc(3,"translate"),e.C$Y(),e.I0R(4,"mat-select",19),e.c53(5,P,2,2,"mat-option",18,e.wJt),e.C$Y(),e.yuY(7,S,3,3,"mat-error"),e.C$Y()),2&t){const n=e.GaO();e.yG2(2),e.cNF(e.kDX(3,2,"Common.ReceiverType")),e.yG2(3),e.oho(n.receiverTypeIds),e.yG2(2),e.C0Y(7,n.receiverControl.hasError("required")?7:-1)}}function $(t,r){1&t&&(e.I0R(0,"mat-error"),e.OEk(1),e.wVc(2,"translate"),e.C$Y()),2&t&&(e.yG2(),e.oRS(" ",e.kDX(2,1,"Errors.TheFieldIsRequired")," "))}function b(t,r){if(1&t){const n=e.KQA();e.I0R(0,"div",8)(1,"mat-form-field",9)(2,"mat-label"),e.OEk(3),e.wVc(4,"translate"),e.C$Y(),e.I0R(5,"textarea",20),e.qCj("blur",function(){e.usT(n);const a=e.GaO();return e.CGJ(a.convertToArray("UserIds"))}),e.wVc(6,"translate"),e.C$Y()()()}2&t&&(e.yG2(3),e.cNF(e.kDX(4,2,"Common.UserIds")),e.yG2(2),e.E7m("placeholder",e.kDX(6,4,"Common.UserIds")))}function F(t,r){if(1&t){const n=e.KQA();e.I0R(0,"div",8)(1,"mat-form-field",9)(2,"mat-label"),e.OEk(3),e.wVc(4,"translate"),e.C$Y(),e.I0R(5,"textarea",21),e.qCj("blur",function(){e.usT(n);const a=e.GaO();return e.CGJ(a.convertToArray("ClientIds"))}),e.wVc(6,"translate"),e.C$Y()()()}2&t&&(e.yG2(3),e.cNF(e.kDX(4,2,"Common.ClientIds")),e.yG2(2),e.E7m("placeholder",e.kDX(6,4,"Common.ClientIds")))}let K=(()=>{class t{get messageControl(){return this.formGroup.get("Message")}get receiverControl(){return this.formGroup.get("ReceiverType")}get nickNameControl(){return this.formGroup.get("NickName")}get typeControl(){return this.formGroup.get("Type")}constructor(n,i,a,m,d,W,V){this.dialogRef=n,this.data=i,this.fb=a,this.localStorageService=m,this.enumService=d,this.mainService=W,this.snackbarService=V,this.states=[],this.genders=[],this.languages=[],this.types=[],this.receiverTypeIds=[],this.isEdit=!1}ngOnInit(){this.data.announcement?.Id&&(this.isEdit=!0),this.announcement={Id:void 0,Message:null,ReceiverType:null,NickName:null,State:!1,Type:null,UserIds:null,ClientIds:null},this.isEdit&&(this.announcement={Id:this.data.announcement.Id,Message:this.data.announcement.Message,ReceiverType:this.data.announcement.ReceiverType,NickName:this.data.announcement.NickName,State:1==this.data.announcement.State,Type:this.data.announcement.Type,ClientIds:this.data.announcement.ClientIds,UserIds:this.data.announcement.UserIds}),this.createForm(),this.setStates(),this.genders=this.localStorageService.get("enums").genders,this.languages=this.localStorageService.get("enums").languages,this.types=this.data.types,this.receiverTypeIds=this.data.receiverTypeIds}setStates(){this.enumService.getUserStates().subscribe(n=>{0===n.ResponseCode&&(this.states=n.ResponseObject)})}createForm(){this.formGroup=this.fb.group({Id:[this.announcement.Id],Message:[this.announcement.Message,[s.AQ.required]],ReceiverType:[this.announcement.ReceiverType,[s.AQ.required]],NickName:[this.announcement.NickName,[s.AQ.required]],State:[this.announcement.State],Type:[this.announcement.Type,[s.AQ.required]],UserIds:[this.announcement.UserIds],ClientIds:[this.announcement.ClientIds]})}get errorControl(){return this.formGroup.controls}close(){this.dialogRef.close()}onSubmit(){if(!this.formGroup.invalid){const n=this.formGroup.getRawValue();n.State=!1===n.State?2:1,n.UserIds=this.parseComma(n.UserIds),n.ClientIds=this.parseComma(n.ClientIds),this.mainService.saveAnnouncement(n).pipe((0,y.U)(1)).subscribe(i=>{0===i.ResponseCode?this.dialogRef.close(!0):this.snackbarService.showError(i.Description)})}}convertToArray(n){const i=this.formGroup.get(n)?.value;i?.split(",").map(a=>parseFloat(a?.trim())).filter(a=>!isNaN(parseFloat(a))),this.formGroup.get(n)?.setValue(i)}parseComma(n){return n?n.split(",").map(i=>parseInt(i.trim())):[]}static#e=this.\u0275fac=function(i){return new(i||t)(e.GI1(l.yI),e.GI1(l.sR),e.GI1(s.KE),e.GI1(g.s),e.GI1(v.Y),e.GI1(A.M),e.GI1(R.i))};static#t=this.\u0275cmp=e.In1({type:t,selectors:[["app-add-edit-announcement"]],standalone:!0,features:[e.UHJ],decls:30,vars:22,consts:[[1,"dialog-container"],["mat-dialog-title","",1,"mat-dialog-title"],["class","title",4,"ngIf"],["alt","icon",1,"icon",3,"click"],[1,"modal-body"],[1,"modal-form",3,"formGroup"],["class","form-row form-one-row",4,"ngIf"],["class","mat-form-field","appearance","outline",4,"ngIf"],[1,"form-row","form-one-row"],["appearance","outline",1,"mat-form-field"],["matInput","","formControlName","Message","placeholder","Message"],["formControlName","State",1,"example-margin"],["mat-dialog-actions","",1,"mat-dialog-actions"],[1,"modal-cancel-btn",3,"click"],["type","submit",1,"modal-primary-btn",3,"disabled","click"],[1,"title"],["matInput","","formControlName","NickName",3,"placeholder"],["formControlName","Type"],[3,"value"],["formControlName","ReceiverType"],["matInput","","formControlName","Message","formControlName","UserIds",3,"placeholder","blur"],["matInput","","formControlName","Message","formControlName","ClientIds",3,"placeholder","blur"]],template:function(i,a){if(1&i&&(e.I0R(0,"div",0)(1,"div",1),e.yuY(2,G,3,3,"span",2)(3,O,3,3,"span",2),e.I0R(4,"mat-icon",3),e.qCj("click",function(){return a.close()}),e.OEk(5,"close"),e.C$Y()(),e.I0R(6,"div",4)(7,"form",5),e.yuY(8,M,7,5,"div",6)(9,Y,8,4,"mat-form-field",7)(10,U,8,4,"mat-form-field",7),e.I0R(11,"div",8)(12,"mat-form-field",9)(13,"mat-label"),e.OEk(14),e.wVc(15,"translate"),e.C$Y(),e.wR5(16,"textarea",10),e.yuY(17,$,3,3,"mat-error"),e.C$Y()(),e.yuY(18,b,7,6,"div",6)(19,F,7,6,"div",6),e.I0R(20,"mat-checkbox",11),e.OEk(21),e.wVc(22,"translate"),e.C$Y()()(),e.I0R(23,"div",12)(24,"button",13),e.qCj("click",function(){return a.close()}),e.OEk(25),e.wVc(26,"translate"),e.C$Y(),e.I0R(27,"button",14),e.qCj("click",function(){return a.onSubmit()}),e.OEk(28),e.wVc(29,"translate"),e.C$Y()()()),2&i){let m,d;e.yG2(2),e.E7m("ngIf",!a.isEdit),e.yG2(),e.E7m("ngIf",a.isEdit),e.yG2(4),e.E7m("formGroup",a.formGroup),e.yG2(),e.E7m("ngIf",!a.isEdit),e.yG2(),e.E7m("ngIf",!a.isEdit),e.yG2(),e.E7m("ngIf",!a.isEdit),e.yG2(4),e.cNF(e.kDX(15,14,"Common.Message")),e.yG2(3),e.C0Y(17,a.messageControl.hasError("required")?17:-1),e.yG2(),e.E7m("ngIf",10===(null==(m=a.formGroup.get("ReceiverType"))?null:m.value)&&!a.isEdit),e.yG2(),e.E7m("ngIf",2===(null==(d=a.formGroup.get("ReceiverType"))?null:d.value)&&!a.isEdit),e.yG2(2),e.cNF(e.kDX(22,16,"Common.Active")),e.yG2(4),e.cNF(e.kDX(26,18,"Common.Cancel")),e.yG2(2),e.E7m("disabled",a.formGroup.invalid),e.yG2(),e.oRS(" ",e.kDX(29,20,"Common.Create")," ")}},dependencies:[u.MD,u.u_,s.y,s.sz,s.ot,s.ue,s.u,p.oB,p.qL,s.sl,s.uW,s.Wo,c.wb,c.Up,c.w5,c.wJ,C.d5,C.kX,T.I5,f.cN,f.yi,h.oJ,l.sr,l.WQ,l.Yp,E.Vn,E.WK,I.O0,I.sD]})}return t})()}}]);