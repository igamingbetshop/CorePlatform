"use strict";(self.webpackChunkAgentManagmentSystem=self.webpackChunkAgentManagmentSystem||[]).push([[4916],{4916:(M,l,o)=>{o.r(l),o.d(l,{AgentEmployeesComponent:()=>p});var d=o(2100),c=o(6692),g=o(7408),m=o(7672),e=o(2116),_=o(9288),u=o(2864),h=o(748),C=o(8196);let p=(()=>{class s extends g.K{constructor(i,n,t,r,a,f){super(f),this.agentService=i,this.dialog=n,this.snackbarService=t,this.activateRoute=r,this.localStorageService=a,this.rowData=[],this.currencyValue=this.configService.currency,this.genders=[],this.rowModelType=m.kt.CLIENT_SIDE,this.columnDefs=[],this.setColumnDefs()}ngOnInit(){this.agentId=this.activateRoute.snapshot.queryParams.agentId,this.genders=this.localStorageService.get("enums")?.genders,this.level=this.activateRoute.snapshot.queryParams.level,this.getRows()}setColumnDefs(){this.columnDefs=[{headerName:"Common.Id",headerValueGetter:this.localizeHeader.bind(this),field:"Id",sortable:!0},{headerName:"Common.FirstName",headerValueGetter:this.localizeHeader.bind(this),field:"FirstName",sortable:!0},{headerName:"Common.LastName",headerValueGetter:this.localizeHeader.bind(this),field:"LastName",sortable:!0},{headerName:"Common.UserName",headerValueGetter:this.localizeHeader.bind(this),field:"Username",sortable:!0},{headerName:"Common.MemberInformationPermission",headerValueGetter:this.localizeHeader.bind(this),field:"MemberInformationPermission",filter:!1,sortable:!0},{headerName:"Common.ViewBetsAndForecast",headerValueGetter:this.localizeHeader.bind(this),field:"ViewBetsAndForecast",filter:!1,sortable:!0},{headerName:"Common.ViewBetsLists",headerValueGetter:this.localizeHeader.bind(this),field:"ViewBetsLists",filter:!1,sortable:!0},{headerName:"Common.ViewReport",headerValueGetter:this.localizeHeader.bind(this),field:"ViewReport",filter:!1,sortable:!0},{headerName:"Common.ViewTransfer",headerValueGetter:this.localizeHeader.bind(this),field:"ViewTransfer",filter:!1,sortable:!0},{headerName:"Common.CreationTime",headerValueGetter:this.localizeHeader.bind(this),field:"CreationTime",sortable:!0}]}getRows(){let i;if(this.agentId){let n=this.agentId.split(",");i=n[n.length-1]}else i=this.agentId;this.mainService.getSubAccounts(+i).subscribe(n=>{0===n.ResponseCode?this.rowData=n.ResponseObject:this.snackbarService.showError(n.Description)})}static#e=this.\u0275fac=function(n){return new(n||s)(e.GI1(_.Y),e.GI1(u.qW),e.GI1(h.i),e.GI1(d.gV),e.GI1(C.s),e.GI1(e.zZn))};static#t=this.\u0275cmp=e.In1({type:s,selectors:[["app-agent-employees"]],standalone:!0,features:[e.eg9,e.UHJ],decls:5,vars:13,consts:[[1,"container"],[1,"content-action"],[1,"title"],[1,"grid-content"],["rowSelection","single",1,"ag-theme-balham",3,"headerHeight","rowHeight","rowData","suppressCopyRowsToClipboard","suppressRowClickSelection","rowModelType","columnDefs","defaultColDef","animateRows","ensureDomOrder","sideBar","enableCellTextSelection","getContextMenuItems","gridReady","columnPinned","columnMoved","columnResized","columnVisible"]],template:function(n,t){1&n&&(e.I0R(0,"div",0)(1,"div",1),e.wR5(2,"div",2),e.C$Y(),e.I0R(3,"div",3)(4,"ag-grid-angular",4),e.qCj("gridReady",function(a){return t.onGridReady(a)})("columnPinned",function(a){return t.onColumnPinned(a)})("columnMoved",function(a){return t.onColumnMoved(a)})("columnResized",function(a){return t.onColumnResized(a)})("columnVisible",function(a){return t.onColumnVisible(a)}),e.C$Y()()()),2&n&&(e.yG2(4),e.E7m("headerHeight",t.headerHeight)("rowHeight",t.rowHeight)("rowData",t.rowData)("suppressCopyRowsToClipboard",!0)("suppressRowClickSelection",!0)("rowModelType",t.rowModelType)("columnDefs",t.columnDefs)("defaultColDef",t.defaultColDef)("animateRows",!0)("ensureDomOrder",!0)("sideBar",t.sideBar)("enableCellTextSelection",!0)("getContextMenuItems",t.getContextMenuItems))},dependencies:[c.Oc,c.U5,d.qQ],styles:[".container[_ngcontent-%COMP%]{width:100%;height:100%;padding:0 10px;box-sizing:border-box;height:85%}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]{height:6%;display:flex;align-items:center;flex-wrap:wrap;height:7%}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]{font-size:20px;letter-spacing:.04em;color:#076192;flex:1;padding-left:10px}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]{text-decoration:none;color:#076192}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]   span[_ngcontent-%COMP%]{color:#000}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]{margin-left:8px;background-color:#239dff;padding:10px;border:1px solid #239dff;min-width:104px;color:#fff;border-radius:4px;font-size:16px;font-weight:500}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]     .mat-button-wrapper{color:#fff!important}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]{color:#076192!important;background-color:#a0c4d8;pointer-events:none}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]     .mat-button-wrapper{color:#076192!important}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]:hover{cursor:pointer;border:1px solid #076192;background:linear-gradient(#076192,#258ecd 50%,#0573ba)}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]{width:100%;height:85%}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]     ag-grid-angular{width:100%;height:100%}"]})}return s})()}}]);