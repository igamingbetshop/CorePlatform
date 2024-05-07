"use strict";(self.webpackChunkAgentManagmentSystem=self.webpackChunkAgentManagmentSystem||[]).push([[5636],{7084:(E,C,r)=>{r.d(C,{s:()=>t});var t=function(n){return n[n.Agents=215]="Agents",n[n.Clients=216]="Clients",n[n.Users=217]="Users",n[n.ReferralLinks=218]="ReferralLinks",n[n.Announcement=219]="Announcement",n[n.ReportByBetShops=223]="ReportByBetShops",n[n.ReportByInternetClientBets=225]="ReportByInternetClientBets",n[n.ReportByAgents=227]="ReportByAgents",n[n.ReportByAgentsCasion=228]="ReportByAgentsCasion",n[n.ReportByTransactions=229]="ReportByTransactions",n[n.Deposits=231]="Deposits",n[n.Withdrawals=232]="Withdrawals",n[n.PaymentForms=233]="PaymentForms",n[n.Tickets=234]="Tickets",n}(t||{})},9196:(E,C,r)=>{r.d(C,{k:()=>v,o:()=>m});var t=r(2116),n=r(1368),u=r(2096),f=r(4060),D=r(8416),w=r(9120),_=r(6504),y=r(3576),i=r(7816),g=r(7048),b=r(2064);const k=(s,c)=>c.Id;function R(s,c){if(1&s&&(t.I0R(0,"mat-option",16),t.OEk(1),t.C$Y()),2&s){const e=c.$implicit;t.E7m("value",e.Id),t.yG2(),t.cNF(e.Name)}}function P(s,c){if(1&s){const e=t.KQA();t.I0R(0,"div",14)(1,"mat-select",15),t.qCj("selectionChange",function(a){t.usT(e);const d=t.GaO();return t.CGJ(d.getByPartnerData(a.value))}),t.I0R(2,"mat-option",16),t.OEk(3),t.C$Y(),t.c53(4,R,2,2,"mat-option",16,k),t.C$Y()()}if(2&s){const e=t.GaO();t.yG2(),t._6D("placeholder","SelectPartner"),t.yG2(),t.E7m("value",null),t.yG2(),t.cNF("SelectPartner"),t.yG2(),t.oho(e.partners)}}function A(s,c){if(1&s){const e=t.KQA();t.I0R(0,"button",7),t.qCj("click",function(){t.usT(e);const a=t.GaO();return t.CGJ(a.selectTime("All Times"))}),t.OEk(1),t.wVc(2,"translate"),t.C$Y()}if(2&s){const e=t.GaO();t.eAK("selected-time","All Times"===e.selectedItem),t.yG2(),t.cNF(t.kDX(2,3,"Common.AllTimes"))}}let v=(()=>{class s{constructor(e){this.translate=e,this.allTimesFilter=(0,t.YhN)(!0),this.toDateChange=new t._w7,this.startDateChange=new t._w7,this.partnerIdChange=new t._w7,this.titleClick=new t._w7,this.titleName="",this.selectedItem="today",this.startDate()}ngOnInit(){this.title&&this.translate.get(this.title).subscribe(e=>{this.titleName=e})}formatDateTime(e){return e?`${e.getFullYear()}-${(e.getMonth()+1).toString().padStart(2,"0")}-${e.getDate().toString().padStart(2,"0")}T${e.getHours().toString().padStart(2,"0")}:${e.getMinutes().toString().padStart(2,"0")}`:""}startDate(){const[e,o]=m.startDate();this.fromDate=e,this.toDate=o}selectTime(e){const[o,a]=m.selectTime(e);this.fromDate=o,this.toDate=a,this.selectedItem=e,this.getCurrentPage()}onStartDateChange(e){this.fromDate=e instanceof Date?e:this.parseDateTimeString(e)}onEndDateChange(e){this.toDate=e instanceof Date?e:this.parseDateTimeString(e)}parseDateTimeString(e){const o=e.split("T");if(2===o.length){const[a,d]=o,[l,p,h]=a.split("-").map(Number),[O,T]=d.split(":").map(Number);return new Date(l,p-1,h,O,T)}return new Date}getByPartnerData(e){this.partnerId=e,this.getCurrentPage()}getCurrentPage(){this.toDateChange.emit({fromDate:this.fromDate,toDate:this.toDate})}onDropdownOpen(e,o){}onTitleClick(){this.titleClick.emit(!0)}static#t=this.\u0275fac=function(o){return new(o||s)(t.GI1(b.qS))};static#e=this.\u0275cmp=t.In1({type:s,selectors:[["app-header"]],inputs:{title:"title",partners:"partners",allTimesFilter:[t.Wk5.SignalBased,"allTimesFilter"]},outputs:{toDateChange:"toDateChange",startDateChange:"startDateChange",partnerIdChange:"partnerIdChange",titleClick:"titleClick"},standalone:!0,features:[t.UHJ],decls:29,vars:28,consts:[[1,"content-action"],[1,"title"],[3,"click"],[1,"contianer-wrap"],["class","custom-dropdown"],[1,"date-time-tabs"],["class","tab-btn","mat-stroked-button","",3,"selected-time"],["mat-stroked-button","",1,"tab-btn",3,"click"],["Dropdown","",1,"calendar-picker",3,"openedDropdown"],["matInput","","type","datetime-local","name","meeting-time",3,"ngModel","ngModelChange"],["dropdownContent",""],["DropdownEnd","",1,"calendar-picker",3,"openedDropdown"],["dropdownEndContent",""],["mat-stroked-button","",1,"mat-btn",3,"click"],[1,"custom-dropdown"],["panelClass","overlay-dropdown","disableOptionCentering","",3,"placeholder","selectionChange"],[3,"value"]],template:function(o,a){if(1&o){const d=t.KQA();t.I0R(0,"div",0)(1,"div",1)(2,"a",2),t.qCj("click",function(){return a.onTitleClick()}),t.OEk(3),t.C$Y()(),t.I0R(4,"div",3),t.yuY(5,P,6,3,"div",4),t.I0R(6,"div",5),t.yuY(7,A,3,5,"button",6),t.I0R(8,"button",7),t.qCj("click",function(){return a.selectTime("month")}),t.OEk(9),t.wVc(10,"translate"),t.C$Y(),t.I0R(11,"button",7),t.qCj("click",function(){return a.selectTime("week")}),t.OEk(12),t.wVc(13,"translate"),t.C$Y(),t.I0R(14,"button",7),t.qCj("click",function(){return a.selectTime("yesterday")}),t.OEk(15),t.wVc(16,"translate"),t.C$Y(),t.I0R(17,"button",7),t.qCj("click",function(){return a.selectTime("today")}),t.OEk(18),t.wVc(19,"translate"),t.C$Y()(),t.I0R(20,"div",8),t.qCj("openedDropdown",function(p){t.usT(d);const h=t.Gew(22);return t.CGJ(a.onDropdownOpen(p,h))}),t.I0R(21,"input",9,10),t.qCj("ngModelChange",function(p){return a.onStartDateChange(p)}),t.C$Y()(),t.I0R(23,"div",11),t.qCj("openedDropdown",function(p){t.usT(d);const h=t.Gew(25);return t.CGJ(a.onDropdownOpen(p,h))}),t.I0R(24,"input",9,12),t.qCj("ngModelChange",function(p){return a.onEndDateChange(p)}),t.C$Y()(),t.I0R(26,"button",13),t.qCj("click",function(){return a.getCurrentPage()}),t.OEk(27),t.wVc(28,"translate"),t.C$Y()()()}2&o&&(t.yG2(3),t.oRS(" ",a.titleName," "),t.yG2(2),t.C0Y(5,a.partners?5:-1),t.yG2(2),t.C0Y(7,a.allTimesFilter()?7:-1),t.yG2(),t.eAK("selected-time","month"===a.selectedItem),t.yG2(),t.cNF(t.kDX(10,18,"Common.Month")),t.yG2(2),t.eAK("selected-time","week"===a.selectedItem),t.yG2(),t.cNF(t.kDX(13,20,"Common.Week")),t.yG2(2),t.eAK("selected-time","yesterday"===a.selectedItem),t.yG2(),t.cNF(t.kDX(16,22,"Common.Yesterday")),t.yG2(2),t.eAK("selected-time","today"===a.selectedItem),t.yG2(),t.cNF(t.kDX(19,24,"Common.Today")),t.yG2(3),t.E7m("ngModel",a.formatDateTime(a.fromDate)),t.yG2(3),t.E7m("ngModel",a.formatDateTime(a.toDate)),t.yG2(3),t.cNF(t.kDX(28,26,"Common.Go")))},dependencies:[n.MD,_.y,_.ot,_.ue,_._G,u.d5,u.kX,y.I5,i.oJ,i.Gw,f.wb,D.cN,D.yi,w.iU,y.Ko,g.SU,_.sl,b.O0,b.sD],styles:["@media only screen and (max-width: 1200px){.content-action{padding:24px 10px 0}.content-action .contianer-wrap .date-time-tabs{display:flex;flex-wrap:wrap;gap:10px;margin:0}.content-action .contianer-wrap .date-time-tabs .tab-btn{margin:0;border:1px solid rgb(196,196,196)!important;background-color:transparent!important;white-space:nowrap}.content-action .contianer-wrap .date-time-tabs .tab-btn.selected-time{background-color:#cacfd6!important}}.content-action{height:6%;display:flex;align-items:center;flex-wrap:wrap;padding-right:10px;padding-left:10px}.content-action .title{font-size:20px;letter-spacing:.04em;color:#076192;flex:1;padding-left:10px}.content-action .title a{text-decoration:none;color:#076192}.content-action .title span{color:#000}.content-action .contianer-wrap{display:flex}.content-action .contianer-wrap .custom-dropdown{margin-right:8px}.content-action .contianer-wrap .custom-dropdown ::ng-deep{font-size:14px}.content-action .contianer-wrap .custom-dropdown ::ng-deep .mat-select{height:36px;border-radius:4px;display:flex!important;align-items:center;width:169px!important;padding:0 8px 0 10px;background-color:#0573ba}.content-action .contianer-wrap .custom-dropdown ::ng-deep .mat-select[aria-expanded=true] .mat-select-trigger .mat-select-arrow-wrapper .mat-select-arrow{border-bottom:5px solid #FFFFFF;border-top:5px solid transparent;margin:0 4px 4px}.content-action .contianer-wrap .custom-dropdown ::ng-deep .mat-select .mat-select-trigger .mat-select-value{color:#fff}.content-action .contianer-wrap .custom-dropdown ::ng-deep .mat-select .mat-select-trigger .mat-select-value .mat-select-placeholder{color:#fff;opacity:.9}.content-action .contianer-wrap .custom-dropdown ::ng-deep .mat-select .mat-select-trigger .mat-select-arrow-wrapper .mat-select-arrow{color:#fff}.content-action .contianer-wrap .custom-dropdown ::ng-deep .mat-select.mat-select-disabled{opacity:.7;pointer-events:none}.content-action .contianer-wrap .date-time-tabs .tab-btn{margin-right:8px;border:1px solid #C4C4C4;box-shadow:none;transition:.3s ease-in}.content-action .contianer-wrap .date-time-tabs .tab-btn .mat-button-focus-overlay{background-color:#fff!important}.content-action .contianer-wrap .date-time-tabs .tab-btn:hover{border:1px solid #076192!important}.content-action .contianer-wrap .date-time-tabs .tab-btn:hover .mat-button-wrapper{color:#076192}.content-action .contianer-wrap .date-time-tabs .tab-btn.selected-time{background-color:#e1eef5!important;border:1px solid #A0C4D7!important;color:#076192!important}.content-action .contianer-wrap .calendar-picker{margin-right:8px}.content-action .contianer-wrap .calendar-picker input{border:1px solid #C4C4C4;height:34px;transition:background-color .3s,color .3s;font-size:14px}body.theme-dark .content-action .contianer-wrap .calendar-picker input{background-color:#303030;color:#fff}.content-action .contianer-wrap .mat-btn{margin-left:8px;background-color:#239dff;padding:10px;border:1px solid #239dff;min-width:104px;color:#fff;border-radius:4px;font-size:16px;font-weight:500;color:#fff!important}.content-action .contianer-wrap .mat-btn ::ng-deep .mat-button-wrapper{color:#fff!important}.content-action .contianer-wrap .mat-btn.disabled{color:#076192!important;background-color:#a0c4d8;pointer-events:none}.content-action .contianer-wrap .mat-btn.disabled ::ng-deep .mat-button-wrapper{color:#076192!important}.content-action .contianer-wrap .mat-btn:hover{cursor:pointer;border:1px solid #076192;background:linear-gradient(#076192,#258ecd 50%,#0573ba)}.content-action .contianer-wrap .tab-btn .mat-stroked-button{border:1px solid #C4C4C4;color:#a8a8a8;background-color:#fff;box-shadow:none;transition:.3s ease-in}.content-action .contianer-wrap .tab-btn .mat-stroked-button .mat-button-focus-overlay{background-color:#fff!important}.content-action .contianer-wrap .tab-btn .mat-stroked-button:hover{border:1px solid #076192!important}.content-action .contianer-wrap .tab-btn .mat-stroked-button:hover .mat-button-wrapper{color:#076192}@media screen and (max-width: 600px){.contianer-wrap{display:flex;width:100%;overflow-x:auto;overflow-y:hidden;flex-wrap:nowrap!important;margin-top:8px}.contianer-wrap .date-time-tabs{flex-wrap:nowrap!important;margin-left:0!important}.content-action{margin-bottom:40px}.contianer-wrap::-webkit-scrollbar{height:0}}\n"],encapsulation:2})}return s})();class m{static startDate(){let c=m.getDateNow();return c.setDate(c.getDate()+1),[m.getDateNow(),c]}static selectTime(c){let e=m.getDateNow(),o=m.getDateNow();switch(o.setDate(o.getDate()+1),c){case"today":break;case"yesterday":e.setDate(e.getDate()-1),o=m.getDateNow();break;case"week":const a=m.getDateNow(),d=new Date(a);d.setDate(a.getDate()-a.getDay()+(0===a.getDay()?-6:1)),new Date(a).setDate(d.getDate()+6),e=d;break;case"month":const p=m.getDateNow();e=new Date(p.getFullYear(),p.getMonth(),1);break;case"All Times":e.setFullYear(1989,11,31);break;case"LastYear":const T=m.getDateNow().getFullYear()-1;e=new Date(T,0,1),o=m.getDateNow()}return[e,o]}static getDateNow(){const c=new Date;return c.setHours(0),c.setMinutes(0),c.setSeconds(0),c.setMilliseconds(0),c}}},3064:(E,C,r)=>{r.d(C,{Q:()=>w});var t=r(2684),n=r(1e3),u=r(2116),f=r(8848),D=r(9600);let w=(()=>{class _{constructor(i,g){this.configService=i,this.mainApiService=g,this.url=this.configService.getApiUrl}getTransactions(i){return this.mainApiService.apiPost(this.url,t.M.REPORT,n.a.GET_TRANSACTIONS,{...i},!0)}getBetShopReport(i){return this.mainApiService.apiPost(this.url,t.M.REPORT,n.a.GET_BET_SHOP_BETS_REPORT,{...i},!0)}getReportByAgentInternetBet(i){return this.mainApiService.apiPost(this.url,t.M.REPORT,n.a.GET_REPORT_BY_AGENT_INTERNET_BET,{...i},!0)}getAgentSportReport(i,g){return this.mainApiService.apiPost(this.url,t.M.REPORT,n.a.GET_AGENT_SPORT_REPORT,{FromDate:i,ToDate:g},!0)}getAgentCasinoReport(i,g){return this.mainApiService.apiPost(this.url,t.M.REPORT,n.a.GET_AGENT_CASINO_REPORT,{FromDate:i,ToDate:g},!0)}getAgentReports(i){return this.mainApiService.apiPost(this.url,t.M.REPORT,n.a.GET_REPORT_BY_AGENTS,{...i},!0)}getBetInfo(i){return this.mainApiService.apiPost(this.url,t.M.REPORT,n.a.GET_BET_INFO,i,!0)}getOperationTypesEnum(){return this.mainApiService.apiPost(this.url,t.M.ENUMERATION_MODEL,n.a.GET_OPERATION_TYPES_ENUM,{},!0)}static#t=this.\u0275fac=function(g){return new(g||_)(u.CoB(f.a),u.CoB(D._))};static#e=this.\u0275prov=u.wxM({token:_,factory:_.\u0275fac,providedIn:"root"})}return _})()}}]);