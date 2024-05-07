"use strict";(self.webpackChunkAgentManagmentSystem=self.webpackChunkAgentManagmentSystem||[]).push([[8640],{516:(M,p,l)=>{l.d(p,{E:()=>h});var c=l(2116),f=l(7408),P=l(7672),_=(l(7024),l(5648));let h=(()=>{class a extends f.K{constructor(t){super(t),this.filterService=(0,c.uUt)(_.q),this.pagination=!0,this.paginationAutoPageSize=!1,this.paginationPageSize=100,this.cacheBlockSize=100,this.suppressPaginationPanel=!1,this.rowModelType=P.kt.SERVER_SIDE,this.serverSideStoreType="full",this.pageSizes=[100,500,1e3,2e3,5e3],this.defaultPageSize=100,this.paginationPage=1,this.defaultColDef={flex:1,editable:!1,filter:"agTextColumnFilter",resizable:!0,unSortIcon:!1,menuTabs:["filterMenuTab","generalMenuTab"],minWidth:15}}onFilterModified(t){this.colId=t.column.colId}keyPressHandler(t){if(document.querySelector(".ag-theme-balham .ag-popup")&&this.colId&&this.gridApi&&"13"===t.key){const i=this.gridApi.getFilterInstance(this.colId);this.colId=void 0,i&&i.applyModel(),this.gridApi.onFilterChanged(),this.gridApi.hidePopupMenu()}}onPaginationChanged(t){const n=this.gridApi?.paginationGetCurrentPage();isNaN(n)||(this.paginationPage=n+1)}onPaginationGoToPage(t){"Enter"===t.key&&(t.preventDefault(),this.gridApi?.paginationGoToPage(this.paginationPage-1))}setFilter(t,n){this.isEmpty(t)||Object.entries(t).forEach(([i,o])=>{const r=i+"s";if(n[r]={ApiOperationTypeList:[],IsAnd:this.getIsAnd(o.operator)},o.hasOwnProperty("condition1")&&o.condition1&&n[r].ApiOperationTypeList.push(this.mapFilterData(o.condition1)),o.hasOwnProperty("condition2")&&o.condition2)n[r].ApiOperationTypeList.push(this.mapFilterData(o.condition2));else{if(0===o.ApiOperationTypeList?.length)return;n[r].ApiOperationTypeList.push(this.mapFilterData(o))}})}mapFilterData(t){const n={OperationTypeId:t.type};switch(t.filterType){case"number":const i=parseFloat(t.filter);isNaN(i)||(n.DecimalValue=i,n.IntValue=Math.round(i));break;case"text":void 0!==t.filter&&(n.StringValue=t.filter);break;case"date":n.DateTimeValue=void 0!==t.dateFrom?t.dateFrom:new Date;break;case"boolean":void 0!==t.filter&&(n.BooleanValue=1===t.filter);break;case"set":n.OperationTypeId=11,void 0!==t.values&&(n.ArrayValue=t.values)}return console.log(n,"appendedFilter"),n}setFilterDropdown(t,n){const i=t.request.filterModel;for(const o of n)i[o]&&!i[o].filter&&(i[o].hasOwnProperty("condition1")?(i[o].condition1.filter=i[o].condition1.type,i[o].condition2.filter=i[o].condition2.type,i[o].condition1.type=1,i[o].condition2.type=1):(i[o].filter=i[o].type,i[o].type=1))}changeFilerName(t,n,i){for(let o=0;o<n.length;o++){const r=n[o];t[r]&&(t[i[o]]=t[r],delete t[r])}}static#e=this.\u0275fac=function(n){return new(n||a)(c.GI1(c.zZn))};static#t=this.\u0275dir=c.Sc5({type:a,standalone:!0,features:[c.eg9]})}return a})()},8640:(M,p,l)=>{l.r(p),l.d(p,{AgentCommissionPlanComponent:()=>n});var c=l(1368),f=l(2100),P=l(4796),O=l(516),_=l(2064),h=l(6692),a=l(2116),D=l(748);const t=["agGrid"];let n=(()=>{class i extends O.E{constructor(r,s){super(r),this.injector=r,this.snackbarService=s,this.components={numericEditor:P.K},this.isServerSideGroup=e=>e.group,this.getServerSideGroupKey=e=>e.Id,this.autoGroupColumnDef={headerName:"GroupId",field:"Id",checkboxSelection:!0,cellRendererParams:{innerRenderer:e=>e.data.Id}},this.createServerSideDatasource=()=>({getRows:e=>{const d={SkipCount:0,TakeCount:-1};-1==e.parentNode.level?d.ProductId=1:d.ParentId=e.parentNode.data.Id,this.setFilter(e.request.filterModel,d),this.mainService.getProducts(d).subscribe(u=>{if(0===u.ResponseCode){const m=u.ResponseObject;Array.isArray(m)?(m.forEach(g=>{g.group=!g.IsLeaf,this.mappedData.forEach(C=>{C.ProductId===g.Id?(g.TurnoverPercent=C.TurnoverPercent,g.Percent=C.Percent):(g.TurnoverPercent=null,g.Percent=null)})}),e.success({rowData:m,rowCount:m.length})):this.snackbarService.showError("Invalid data format: Entities is not an array")}else this.snackbarService.showError(u.Description)})}}),this.columnDefs=[{headerName:"Common.Id",headerValueGetter:this.localizeHeader.bind(this),field:"Id",hide:!0},{headerName:"Common.Name",headerValueGetter:this.localizeHeader.bind(this),field:"Name",editable:!0,onCellValueChanged:e=>this.onCellValueChanged(e),filter:"agTextColumnFilter",filterParams:{buttons:["apply","reset"],closeOnApply:!0,filterOptions:this.filterService.textOptions}},{headerName:"Common.ParentId",headerValueGetter:this.localizeHeader.bind(this),field:"ParentId"},{headerName:"Common.Percent",headerValueGetter:this.localizeHeader.bind(this),field:"Percent",editable:!0,onCellValueChanged:e=>this.onCellValueChanged(e),filter:"agTextColumnFilter",filterParams:{buttons:["apply","reset"],closeOnApply:!0,filterOptions:this.filterService.textOptions},cellEditor:"numericEditor"},{headerName:"Common.TurnoverPercent",headerValueGetter:this.localizeHeader.bind(this),field:"TurnoverPercent",editable:!0,onCellValueChanged:e=>this.onCellValueChanged(e),filter:"agTextColumnFilter",filterParams:{buttons:["apply","reset"],closeOnApply:!0,filterOptions:this.filterService.textOptions},cellEditor:"textEditor"},{headerName:"Common.State",headerValueGetter:this.localizeHeader.bind(this),field:"State",cellRenderer:e=>0===e.data.State?"Inactive":1===e.data.State?"Active":""}]}ngOnInit(){this.agentId=this.route.snapshot.queryParams.agentId,this.getCommissionPlan(),this.level=this.route.snapshot.queryParams.level}getCommissionPlan(){let r;if(this.agentId){let s=this.agentId.split(",");r=s[s.length-1]}else r=this.agentId;this.mainService.getCommissionPlan({AgentId:+r}).subscribe(s=>{0===s.ResponseCode?this.mappedData=s.ResponseObject:this.snackbarService.showError(s.Description)})}onGridReady(r){this.gridApi=r.api,this.gridApi.setServerSideDatasource(this.createServerSideDatasource())}onCellValueChanged(r){this.mainService.updateCommissionPlan({AgentID:this.agentId,ProductId:r.data.Id,TurnoverPercent:r.data.TurnoverPercent,Percent:r.data.Percent}).subscribe(e=>{0===e.ResponseCode||this.snackbarService.showError(e.Description)})}static#e=this.\u0275fac=function(s){return new(s||i)(a.GI1(a.zZn),a.GI1(D.i))};static#t=this.\u0275cmp=a.In1({type:i,selectors:[["app-agent-commission-plan"]],viewQuery:function(s,e){if(1&s&&a.CC$(t,5),2&s){let d;a.wto(d=a.Gqi())&&(e.agGrid=d.first)}},standalone:!0,features:[a.eg9,a.UHJ],decls:6,vars:16,consts:[[1,"container"],[1,"content-action"],[1,"title"],[1,"grid-content"],[1,"ag-theme-balham",3,"headerHeight","rowHeight","rowData","enableGroupEdit","autoGroupColumnDef","rowModelType","columnDefs","defaultColDef","treeData","animateRows","components","cacheBlockSize","isServerSideGroup","getServerSideGroupKey","ensureDomOrder","enableCellTextSelection","gridReady"],["agGrid",""]],template:function(s,e){1&s&&(a.I0R(0,"div",0)(1,"div",1),a.wR5(2,"div",2),a.C$Y(),a.I0R(3,"div",3)(4,"ag-grid-angular",4,5),a.qCj("gridReady",function(u){return e.onGridReady(u)}),a.C$Y()()()),2&s&&(a.yG2(4),a.E7m("headerHeight",e.headerHeight)("rowHeight",e.rowHeight)("rowData",e.rowData)("enableGroupEdit",!0)("autoGroupColumnDef",e.autoGroupColumnDef)("rowModelType",e.rowModelType)("columnDefs",e.columnDefs)("defaultColDef",e.defaultColDef)("treeData",!0)("animateRows",!0)("components",e.components)("cacheBlockSize",1e4)("isServerSideGroup",e.isServerSideGroup)("getServerSideGroupKey",e.getServerSideGroupKey)("ensureDomOrder",!0)("enableCellTextSelection",!0))},dependencies:[c.MD,h.Oc,h.U5,f.qQ,_.O0],styles:[".container[_ngcontent-%COMP%]{width:100%;height:100%;padding:0 10px;box-sizing:border-box;height:84%;overflow:hidden}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]{height:6%;display:flex;align-items:center;flex-wrap:wrap;height:7%}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]{font-size:20px;letter-spacing:.04em;color:#076192;flex:1;padding-left:10px}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]{text-decoration:none;color:#076192}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]   span[_ngcontent-%COMP%]{color:#000}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]{margin-left:8px;background-color:#239dff;padding:10px;border:1px solid #239dff;min-width:104px;color:#fff;border-radius:4px;font-size:16px;font-weight:500}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]     .mat-button-wrapper{color:#fff!important}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]{color:#076192!important;background-color:#a0c4d8;pointer-events:none}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn.disabled[_ngcontent-%COMP%]     .mat-button-wrapper{color:#076192!important}.container[_ngcontent-%COMP%]   .content-action[_ngcontent-%COMP%]   .mat-btn[_ngcontent-%COMP%]:hover{cursor:pointer;border:1px solid #076192;background:linear-gradient(#076192,#258ecd 50%,#0573ba)}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]{width:100%;height:77vh}.container[_ngcontent-%COMP%]   .grid-content[_ngcontent-%COMP%]     ag-grid-angular{width:100%;height:100%}"]})}return i})()}}]);