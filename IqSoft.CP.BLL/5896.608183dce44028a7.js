"use strict";(self.webpackChunkAgentManagmentSystem=self.webpackChunkAgentManagmentSystem||[]).push([[5896],{516:(y,f,e)=>{e.d(f,{E:()=>P});var o=e(2116),t=e(7408),C=e(7672),D=(e(7024),e(5648));let P=(()=>{class p extends t.K{constructor(a){super(a),this.filterService=(0,o.uUt)(D.q),this.pagination=!0,this.paginationAutoPageSize=!1,this.paginationPageSize=100,this.cacheBlockSize=100,this.suppressPaginationPanel=!1,this.rowModelType=C.kt.SERVER_SIDE,this.serverSideStoreType="full",this.pageSizes=[100,500,1e3,2e3,5e3],this.defaultPageSize=100,this.paginationPage=1,this.defaultColDef={flex:1,editable:!1,filter:"agTextColumnFilter",resizable:!0,unSortIcon:!1,menuTabs:["filterMenuTab","generalMenuTab"],minWidth:15}}onFilterModified(a){this.colId=a.column.colId}keyPressHandler(a){if(document.querySelector(".ag-theme-balham .ag-popup")&&this.colId&&this.gridApi&&"13"===a.key){const d=this.gridApi.getFilterInstance(this.colId);this.colId=void 0,d&&d.applyModel(),this.gridApi.onFilterChanged(),this.gridApi.hidePopupMenu()}}onPaginationChanged(a){const r=this.gridApi?.paginationGetCurrentPage();isNaN(r)||(this.paginationPage=r+1)}onPaginationGoToPage(a){"Enter"===a.key&&(a.preventDefault(),this.gridApi?.paginationGoToPage(this.paginationPage-1))}setFilter(a,r){this.isEmpty(a)||Object.entries(a).forEach(([d,s])=>{const g=d+"s";if(r[g]={ApiOperationTypeList:[],IsAnd:this.getIsAnd(s.operator)},s.hasOwnProperty("condition1")&&s.condition1&&r[g].ApiOperationTypeList.push(this.mapFilterData(s.condition1)),s.hasOwnProperty("condition2")&&s.condition2)r[g].ApiOperationTypeList.push(this.mapFilterData(s.condition2));else{if(0===s.ApiOperationTypeList?.length)return;r[g].ApiOperationTypeList.push(this.mapFilterData(s))}})}mapFilterData(a){const r={OperationTypeId:a.type};switch(a.filterType){case"number":const d=parseFloat(a.filter);isNaN(d)||(r.DecimalValue=d,r.IntValue=Math.round(d));break;case"text":void 0!==a.filter&&(r.StringValue=a.filter);break;case"date":r.DateTimeValue=void 0!==a.dateFrom?a.dateFrom:new Date;break;case"boolean":void 0!==a.filter&&(r.BooleanValue=1===a.filter);break;case"set":r.OperationTypeId=11,void 0!==a.values&&(r.ArrayValue=a.values)}return console.log(r,"appendedFilter"),r}setFilterDropdown(a,r){const d=a.request.filterModel;for(const s of r)d[s]&&!d[s].filter&&(d[s].hasOwnProperty("condition1")?(d[s].condition1.filter=d[s].condition1.type,d[s].condition2.filter=d[s].condition2.type,d[s].condition1.type=1,d[s].condition2.type=1):(d[s].filter=d[s].type,d[s].type=1))}changeFilerName(a,r,d){for(let s=0;s<r.length;s++){const g=r[s];a[g]&&(a[d[s]]=a[g],delete a[g])}}static#e=this.\u0275fac=function(r){return new(r||p)(o.GI1(o.zZn))};static#t=this.\u0275dir=o.Sc5({type:p,standalone:!0,features:[o.eg9]})}return p})()},7084:(y,f,e)=>{e.d(f,{s:()=>o});var o=function(t){return t[t.Agents=215]="Agents",t[t.Clients=216]="Clients",t[t.Users=217]="Users",t[t.ReferralLinks=218]="ReferralLinks",t[t.Announcement=219]="Announcement",t[t.ReportByBetShops=223]="ReportByBetShops",t[t.ReportByInternetClientBets=225]="ReportByInternetClientBets",t[t.ReportByAgents=227]="ReportByAgents",t[t.ReportByAgentsCasion=228]="ReportByAgentsCasion",t[t.ReportByTransactions=229]="ReportByTransactions",t[t.Deposits=231]="Deposits",t[t.Withdrawals=232]="Withdrawals",t[t.PaymentForms=233]="PaymentForms",t[t.Tickets=234]="Tickets",t}(o||{})},4976:(y,f,e)=>{e.d(f,{E:()=>o});class o{constructor(){this.OrderBy=null}}},5896:(y,f,e)=>{e.r(f),e.d(f,{ClientsComponent:()=>w});var o=e(1528),t=e(1368),C=e(2100),M=e(6504),D=e(2096),P=e(6692),p=e(9120),u=e(3576),a=e(2864),d=(e(7024),e(4976)),s=e(516),g=e(2064),b=e(7816),v=e(7672),S=e(1560),z=e(9556),T=e(7084),n=e(2116),W=e(9112),N=e(2236),A=e(748),I=e(8196);function V(_,L){1&_&&n.wR5(0,"router-outlet")}const B=_=>({hide:_});let w=(()=>{class _ extends s.E{constructor(i,c,l,m,h,H,G){super(G),this.clientService=i,this.dialog=c,this.activateRoute=l,this.enumService=m,this.snackbarService=h,this.localStorageService=H,this.rowData=[],this.currencyValue=this.configService.currency,this.genders=[],this.userStateEnum=[],this.createServerSideDatasource=()=>({getRows:R=>{let E=new d.E;E.SkipCount=this.paginationPage-1,E.TakeCount=Number(this.cacheBlockSize),this.setSort(R.request.sortModel,E),this.setFilter(R.request.filterModel,E),this.clientService.getClients(E).subscribe(O=>{if(0===O.ResponseCode){const U=O.ResponseObject.Entities;if(this.rowData=O.ResponseObject.Entities,this.configService.isAffiliate){const x=O.ResponseObject.TotalDepositAmount;this.gridApi?.setPinnedBottomRowData([{TotalDepositAmount:`${x}  ${this.currencyValue}`}])}R.success({rowData:U,rowCount:O.ResponseObject.Count})}else this.snackbarService.showError(O.Description)})}}),this.columnDefs=[],this.adminMenuId=T.s.Clients}ngOnInit(){this.getUserStateEnum(),this.genders=this.localStorageService.get("enums")?.genders}getUserStateEnum(){this.enumService.getUserStateEnum().subscribe(i=>{0===i.ResponseCode?(this.userStateEnum=i.ResponseObject,this.setColumnDefs()):(this.snackbarService.showError(i.Description),this.setColumnDefs())})}setColumnDefs(){this.columnDefs=this.configService.isAffiliate?[{headerName:"Common.Id",headerValueGetter:this.localizeHeader.bind(this),field:"Id",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.AffiliateId",headerValueGetter:this.localizeHeader.bind(this),field:"AffiliateId",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.AffiliatePlaformId",headerValueGetter:this.localizeHeader.bind(this),field:"AffiliatePlaformId",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.Currency",headerValueGetter:this.localizeHeader.bind(this),field:"CurrencyId",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.UserName",headerValueGetter:this.localizeHeader.bind(this),field:"UserName",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.RefId",headerValueGetter:this.localizeHeader.bind(this),field:"RefId",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.ConvertedTotalDepositAmount",headerValueGetter:this.localizeHeader.bind(this),field:"ConvertedTotalDepositAmount",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.TotalDepositAmount",headerValueGetter:this.localizeHeader.bind(this),field:"TotalDepositAmount",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.FirstDepositeDate",headerValueGetter:this.localizeHeader.bind(this),field:"FirstDepositDate",filter:!1,sortable:!0,minWidth:80,cellRenderer:function(i){return`${new t.y("en-US").transform(i.data.FirstDepositDate,"medium")}`}},{headerName:"Common.LastDepositeDate",headerValueGetter:this.localizeHeader.bind(this),field:"LastDepositDate",filter:!1,sortable:!0,minWidth:80,cellRenderer:function(i){return`${new t.y("en-US").transform(i.data.LastDepositeDate,"medium")}`}},{headerName:"Common.CreationDate",headerValueGetter:this.localizeHeader.bind(this),field:"CreationDate",filter:!1,sortable:!0,minWidth:80}]:[{headerName:"Common.Id",headerValueGetter:this.localizeHeader.bind(this),field:"Id",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.Level",headerValueGetter:this.localizeHeader.bind(this),field:"Level",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.Email",headerValueGetter:this.localizeHeader.bind(this),field:"Email",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.IsEmailVerified",headerValueGetter:this.localizeHeader.bind(this),field:"IsEmailVerified",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.Currency",headerValueGetter:this.localizeHeader.bind(this),field:"CurrencyId",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.UserName",headerValueGetter:this.localizeHeader.bind(this),field:"UserName",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.PartnerId",headerValueGetter:this.localizeHeader.bind(this),field:"PartnerId",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.Gender",headerValueGetter:this.localizeHeader.bind(this),field:"Gender",filter:!1,sortable:!0,minWidth:80,cellRenderer:i=>{const c=i.value,l=this.genders?.find(m=>m.Id===c);return l?l.Name:"Unknown Gender"}},{headerName:"Common.BirthDate",headerValueGetter:this.localizeHeader.bind(this),field:"BirthDate",filter:!1,sortable:!0,minWidth:80,cellRenderer:function(i){return`${new t.y("en-US").transform(i.data.BirthDate,"medium")}`}},{headerName:"Common.SendMail",headerValueGetter:this.localizeHeader.bind(this),field:"SendMail",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.SendSms",headerValueGetter:this.localizeHeader.bind(this),field:"SendSms",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.FirstName",headerValueGetter:this.localizeHeader.bind(this),field:"FirstName",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.LastName",headerValueGetter:this.localizeHeader.bind(this),field:"LastName",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.RegionId",headerValueGetter:this.localizeHeader.bind(this),field:"RegionId",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.RegistrationIp",headerValueGetter:this.localizeHeader.bind(this),field:"RegistrationIp",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.DocumentNumber",headerValueGetter:this.localizeHeader.bind(this),field:"DocumentNumber",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.DocumentType",headerValueGetter:this.localizeHeader.bind(this),field:"DocumentType",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.DocumentIssuedBy",headerValueGetter:this.localizeHeader.bind(this),field:"DocumentIssuedBy",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.IsDocumentVerified",headerValueGetter:this.localizeHeader.bind(this),field:"IsDocumentVerified",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.Address",headerValueGetter:this.localizeHeader.bind(this),field:"Address",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.MobileNumber",headerValueGetter:this.localizeHeader.bind(this),field:"MobileNumber",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.Phone",headerValueGetter:this.localizeHeader.bind(this),field:"Phone",filter:!1,sortable:!1,hide:!0,minWidth:80},{headerName:"Common.Fax",headerValueGetter:this.localizeHeader.bind(this),field:"Fax",filter:!1,sortable:!1,hide:!0,minWidth:80},{headerName:"Common.IsMobileNumberVerified",headerValueGetter:this.localizeHeader.bind(this),field:"IsMobileNumberVerified",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.LanguageId",headerValueGetter:this.localizeHeader.bind(this),field:"LanguageId",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.TimeStamp",headerValueGetter:this.localizeHeader.bind(this),field:"TimeStamp",filter:!1,sortable:!1,hide:!0,minWidth:80},{headerName:"Common.CreationTime",headerValueGetter:this.localizeHeader.bind(this),field:"CreationTime",filter:!1,sortable:!0,minWidth:80,cellRenderer:function(i){return`${new t.y("en-US").transform(i.data.CreationTime,"medium")}`}},{headerName:"Common.LastUpdateTime",headerValueGetter:this.localizeHeader.bind(this),field:"LastUpdateTime",filter:!1,sortable:!0,minWidth:80,cellRenderer:function(i){return`${new t.y("en-US").transform(i.data.LastUpdateTime,"medium")}`}},{headerName:"Common.Group",headerValueGetter:this.localizeHeader.bind(this),field:"Group",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.State",headerValueGetter:this.localizeHeader.bind(this),field:"State",filter:!1,sortable:!0,minWidth:80,cellRenderer:i=>{const c=i.value,l=this.userStateEnum?.find(m=>m.Id===c);return l?l.Name:"State Unknown"}},{headerName:"Common.Closed",headerValueGetter:this.localizeHeader.bind(this),field:"Closed",filter:!1,sortable:!1,hide:!0,minWidth:80},{headerName:"Common.CallToPhone",headerValueGetter:this.localizeHeader.bind(this),field:"CallToPhone",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.SendPromotions",headerValueGetter:this.localizeHeader.bind(this),field:"SendPromotions",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.InformedFrom",headerValueGetter:this.localizeHeader.bind(this),field:"InformedFrom",filter:!1,sortable:!1,hide:!0,minWidth:80},{headerName:"Common.ZipCode",headerValueGetter:this.localizeHeader.bind(this),field:"ZipCode",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.HasNote",headerValueGetter:this.localizeHeader.bind(this),field:"HasNote",filter:!1,sortable:!0,minWidth:80},{headerName:"Common.Info",headerValueGetter:this.localizeHeader.bind(this),field:"Info",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.RealBalance",headerValueGetter:this.localizeHeader.bind(this),field:"RealBalance",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.BonusBalance",headerValueGetter:this.localizeHeader.bind(this),field:"BonusBalance",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.GGR",headerValueGetter:this.localizeHeader.bind(this),field:"GGR",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.NGR",headerValueGetter:this.localizeHeader.bind(this),field:"NGR",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.UserId",headerValueGetter:this.localizeHeader.bind(this),field:"UserId",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.AllowOutright",headerValueGetter:this.localizeHeader.bind(this),field:"AllowOutright",filter:!1,sortable:!1,minWidth:80},{headerName:"Common.AllowParentOutright",headerValueGetter:this.localizeHeader.bind(this),field:"AllowParentOutright",filter:!1,sortable:!1,hide:!0,minWidth:80},{headerName:"Common.AllowDoubleCommission",headerValueGetter:this.localizeHeader.bind(this),field:"AllowDoubleCommission",filter:!1,sortable:!1,hide:!0,minWidth:80},{headerName:"Common.AllowParentDoubleCommission",headerValueGetter:this.localizeHeader.bind(this),field:"AllowParentDoubleCommission",filter:!1,sortable:!1,hide:!0,minWidth:80},{headerName:"Common.NickName",headerValueGetter:this.localizeHeader.bind(this),field:"NickName",filter:!1,sortable:!0,minWidth:80,hide:!0},{headerName:"Common.View",headerValueGetter:this.localizeHeader.bind(this),filter:!1,cellRenderer:i=>i.node.rowPinned?"":'<i style="color:#076192; cursor: pointer; margin-left: 0" class="material-icons">\n            visibility\n          </i>',pinned:"right",width:1,minWidth:28,onCellClicked:i=>this.toRedirectToClient(i)}]}onGridReady(i){super.onGridReady(i),this.gridApi?.setServerSideDatasource(this.createServerSideDatasource())}toRedirectToClient(i){this.router.navigate(["/main/clients/client/main"],{queryParams:{clientId:i.data.Id}})}onPageSizeChanged(i){this.cacheBlockSize=i.value,this.gridApi?.paginationSetPageSize(Number(this.cacheBlockSize)),setTimeout(()=>{this.gridApi?.refreshServerSide({purge:!0})},0)}onCreateClient(){var i=this;return(0,o.c)(function*(){const{CreateClientComponent:c}=yield Promise.all([e.e(3840),e.e(7456)]).then(e.bind(e,7456));i.dialog.open(c,{width:v.il.MEDIUM}).afterClosed().subscribe(m=>{m&&i.addRowToDataSource(m)})})()}addRowToDataSource(i){this.rowData.push(i),this.gridApi?.refreshServerSide({purge:!0})}static#e=this.\u0275fac=function(c){return new(c||_)(n.GI1(W.i),n.GI1(a.qW),n.GI1(C.gV),n.GI1(N.Y),n.GI1(A.i),n.GI1(I.s),n.GI1(n.zZn))};static#t=this.\u0275cmp=n.In1({type:_,selectors:[["app-clients"]],standalone:!0,features:[n.eg9,n.UHJ],decls:13,vars:24,consts:[[1,"container",3,"ngClass"],[1,"content-action"],[1,"title"],[1,"grid-content"],["rowSelection","single",1,"ag-theme-balham",3,"headerHeight","rowHeight","rowData","suppressCopyRowsToClipboard","suppressRowClickSelection","rowModelType","columnDefs","defaultColDef","animateRows","ensureDomOrder","sideBar","enableCellTextSelection","pagination","getContextMenuItems","gridReady","columnPinned","columnMoved","columnResized","columnVisible"],[1,"footer-ag-grid"],[3,"cacheBlockSize","defaultPageSize","pageChange"],[1,"match-action"],["mat-stroked-button","",1,"mat-btn",3,"click"]],template:function(c,l){1&c&&(n.I0R(0,"div",0)(1,"div",1)(2,"div",2),n.OEk(3),n.C$Y()(),n.I0R(4,"div",3)(5,"ag-grid-angular",4),n.qCj("gridReady",function(h){return l.onGridReady(h)})("columnPinned",function(h){return l.onColumnPinned(h)})("columnMoved",function(h){return l.onColumnMoved(h)})("columnResized",function(h){return l.onColumnResized(h)})("columnVisible",function(h){return l.onColumnVisible(h)}),n.C$Y(),n.I0R(6,"div",5)(7,"app-pagination",6),n.qCj("pageChange",function(h){return l.onPageSizeChanged(h)}),n.C$Y(),n.I0R(8,"div",7)(9,"button",8),n.qCj("click",function(){return l.onCreateClient()}),n.OEk(10),n.wVc(11,"translate"),n.C$Y()()()()(),n.yuY(12,V,1,0,"router-outlet")),2&c&&(n.E7m("ngClass",n.S45(22,B,l.activateRoute.snapshot.queryParams.clientId)),n.yG2(3),n.cNF("Menus.Clients"),n.yG2(2),n.E7m("headerHeight",l.headerHeight)("rowHeight",l.rowHeight)("rowData",l.rowData)("suppressCopyRowsToClipboard",!0)("suppressRowClickSelection",!0)("rowModelType",l.rowModelType)("columnDefs",l.columnDefs)("defaultColDef",l.defaultColDef)("animateRows",!0)("ensureDomOrder",!0)("sideBar",l.sideBar)("enableCellTextSelection",!0)("pagination",l.pagination)("getContextMenuItems",l.getContextMenuItems),n.yG2(2),n.E7m("cacheBlockSize",l.cacheBlockSize)("defaultPageSize",l.defaultPageSize),n.yG2(3),n.cNF(n.kDX(11,20,"Clients.AddClient")),n.yG2(2),n.C0Y(12,l.activateRoute.snapshot.queryParams.clientId?12:-1))},dependencies:[t.MD,t.QF,M.y,P.Oc,P.U5,g.O0,g.sD,D.d5,b.oJ,b.Gw,a.sr,p.iU,u.Ko,S.oB,C.qQ,C.cP,z.i],styles:[".container[_ngcontent-%COMP%]{width:100%;height:100%;padding:0 10px;box-sizing:border-box}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]{height:6%;display:flex;align-items:center;flex-wrap:wrap;height:60px}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]{font-size:20px;letter-spacing:.04em;color:#076192;flex:1;padding-left:10px}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]{text-decoration:none;color:#076192}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]   span[_ngcontent-%COMP%]{color:#000}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]{margin-left:8px;background-color:#239dff;padding:10px;border:1px solid #239dff;min-width:104px;color:#fff;border-radius:4px;font-size:16px;font-weight:500}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]     .mat-button-wrapper{color:#fff!important}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]{color:#076192!important;background-color:#a0c4d8;pointer-events:none}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]     .mat-button-wrapper{color:#076192!important}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]:hover{cursor:pointer;border:1px solid #076192;background:linear-gradient(#076192,#258ecd 50%,#0573ba)}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]{width:100%;height:93%}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]     .ag-paging-panel{align-items:center;display:flex;justify-content:flex-end}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]     ag-grid-angular{width:100%;height:100%}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]{position:absolute;left:23%;bottom:14px;display:flex;align-items:center;width:77%}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .mat-select[_ngcontent-%COMP%]    {font-size:14px}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .mat-select[_ngcontent-%COMP%]     .mat-select{height:36px;border-radius:4px;display:flex!important;align-items:center;width:169px!important;padding:0 8px 0 10px;background-color:#0573ba}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .mat-select[_ngcontent-%COMP%]     .mat-select[aria-expanded=true] .mat-select-trigger .mat-select-arrow-wrapper .mat-select-arrow{border-bottom:5px solid #FFFFFF;border-top:5px solid transparent;margin:0 4px 4px}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .mat-select[_ngcontent-%COMP%]     .mat-select .mat-select-trigger .mat-select-value{color:#fff}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .mat-select[_ngcontent-%COMP%]     .mat-select .mat-select-trigger .mat-select-value .mat-select-placeholder{color:#fff;opacity:.9}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .mat-select[_ngcontent-%COMP%]     .mat-select .mat-select-trigger .mat-select-arrow-wrapper .mat-select-arrow{color:#fff}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .mat-select[_ngcontent-%COMP%]     .mat-select.mat-select-disabled{opacity:.7;pointer-events:none}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]{margin-left:auto}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]{margin-right:10px;background-color:#239dff;padding:10px;border:1px solid #239dff;min-width:104px;color:#fff;border-radius:4px;font-size:16px;font-weight:500}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]     .mat-button-wrapper{color:#fff!important}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]{color:#076192!important;background-color:#a0c4d8;pointer-events:none}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]     .mat-button-wrapper{color:#076192!important}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]   .footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]:hover{cursor:pointer;border:1px solid #076192;background:linear-gradient(#076192,#258ecd 50%,#0573ba)}@media screen and (max-width: 780px){.footer-ag-grid[_ngcontent-%COMP%]{display:flex!important;bottom:4px!important;left:55%!important;width:45%!important;gap:3px!important}.footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]{display:flex!important}.footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]{margin-right:10px;background-color:#239dff;padding:10px;border:1px solid #239dff;min-width:104px;color:#fff;border-radius:4px;font-size:16px;font-weight:500}.footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]     .mat-button-wrapper{color:#fff!important}.footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]{color:#076192!important;background-color:#a0c4d8;pointer-events:none}.footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]     .mat-button-wrapper{color:#076192!important}.footer-ag-grid[_ngcontent-%COMP%]   .match-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]:hover{cursor:pointer;border:1px solid #076192;background:linear-gradient(#076192,#258ecd 50%,#0573ba)}}"]})}return _})()},9556:(y,f,e)=>{e.d(f,{i:()=>p});var o=e(2116),t=e(6504),C=e(4060),M=e(2096),D=e(3576);function P(u,a){if(1&u&&(o.I0R(0,"mat-option",2),o.OEk(1),o.C$Y()),2&u){const r=a.$implicit;o.E7m("value",r),o.yG2(),o.cNF(r)}}let p=(()=>{class u{constructor(){this.cacheBlockSize=(0,o.YhN)(),this.defaultPageSize=(0,o.YhN)(),this.pageSizes=[100,500,1e3,2e3,5e3],this.pageChange=new o._w7}onPageSizeChanged(r){this.pageChange.emit(r)}static#e=this.\u0275fac=function(d){return new(d||u)};static#t=this.\u0275cmp=o.In1({type:u,selectors:[["app-pagination"]],inputs:{cacheBlockSize:[o.Wk5.SignalBased,"cacheBlockSize"],defaultPageSize:[o.Wk5.SignalBased,"defaultPageSize"]},outputs:{pageChange:"pageChange"},standalone:!0,features:[o.UHJ],decls:4,vars:1,consts:[[1,"pages-sizes"],["panelClass","overlay-dropdown small","disableOptionCentering","",1,"pagination-select",3,"value","selectionChange"],[3,"value"]],template:function(d,s){1&d&&(o.I0R(0,"div",0)(1,"mat-select",1),o.qCj("selectionChange",function(b){return s.onPageSizeChanged(b)}),o.c53(2,P,2,2,"mat-option",2,o.wJt),o.C$Y()()),2&d&&(o.yG2(),o.E7m("value",s.defaultPageSize()),o.yG2(),o.oho(s.pageSizes))},dependencies:[t.y,M.d5,M.kX,D.I5,C.wb],styles:[".pages-sizes{margin-bottom:6px}.mat-mdc-select-panel-above{width:110px!important}.pagination-select{border:1px solid #239dff;border-radius:4px;background-color:#0573ba;padding-left:3px;padding-right:3px}.pagination-select .mat-mdc-select-trigger .mat-mdc-select-value .mat-mdc-select-value-text{color:#fff!important}.pagination-select .mat-mdc-select-trigger .mat-mdc-select-arrow-wrapper svg{color:#fff!important}.pagination-select[aria-expanded=true] .mat-mdc-select-trigger .mat-mdc-select-arrow-wrapper .mat-mdc-select-arrow{background-color:#0573ba!important;border-bottom:5px solid #0573BA;border-top:5px solid transparent;margin:0 4px 4px}.pagination-select[aria-expanded=true] .mat-mdc-select-trigger .mat-mdc-select-arrow-wrapper .mat-mdc-select-arrow svg{top:-200%;left:0;transform:rotate(180deg)}\n"],encapsulation:2})}return u})()}}]);