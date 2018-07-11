var url =new URL(window.location.href)

var CLIENT_TENANT = url.searchParams.get("CLIENT_TENANT") ; 
var CLIENT_SUBSCRIPTION = url.searchParams.get("CLIENT_SUBSCRIPTION")

var sizeList = null;
var osList = null;
var selectedVmSize = null ;
var dataDisks = 0 ;

var vmParams = {
	osType:null,
	popOsImage:null,
	dataDisks:null,
	dataDisksDetails:null,
	vmSzie:null,
	region:null
};


function getRegions() {
	
	if($("#regionsSelecor").children().length == 1){	
		$.ajax({
			type:"get",
            url:"/vmoptions/regions",
			async:true,
            success: function (result) {
                for (index in result) {
                	var region = result[index].Name;
                	
                	   if( region.indexOf("china")<0 && region.indexOf("gov")<0 && region.indexOf("germany")<0 && region.indexOf("usdod") ) 
                           $("#regionsSelecor").append('<option value=' + region + '>' + region +'</option>');              
                }
			}
		});	
    }        
}

function regionSelect(){
	
	var client_tenant = CLIENT_TENANT ; 
	var client_subscription = CLIENT_SUBSCRIPTION;
	
	var region = $("#regionsSelecor option:selected").text();
	$("#sizeSelecor").children().remove();
	$("#sizeSelecor").append('<option > Loading </option>');
	$.ajax({  
			type:"get",
            url:"/vmoptions/avaliable/"+ client_tenant +"/" + client_subscription + "/" + region,
			async:true,
            success: function (result) {
            	sizeList = result;
                for (index in result) {
                    $("#sizeSelecor").append('<option value=' + index + '>' + result[index].Name+'</option>');              
                }
                $("#sizeSelecor").children()[0].remove();
                $("#sizeSelecor").append('<option value="" disabled selected>Select VM Size</option>')
			}
	});	
	
}
    
function getSize(){
	
	var index = $("#sizeSelecor option:selected").val();
	selectedVmSize = sizeList[index];
	
	$("#cores").text(selectedVmSize.Cores);
	$("#memory").text( selectedVmSize.MemoryInMB/1024 + "GB");
	$("#maxdiskcount").text(selectedVmSize.MaxDataDiskCount);
	$("#dataDisksLeft").text(selectedVmSize.MaxDataDiskCount);
	$("#vmSizeInfo").show();
	$("#diskAdder").show();
	$("#diskList").empty()
	
}

function getPopularOsImage(){
	
	if(osList == null){
		$.ajax({
			type:"get",
			url:"/vmoptions/popularos",
			async:false,
			success:function(result){
				osList = result;
			}
		});
	}
	
	$("#osImageSelector").children().remove();
	$("#osImageSelector").append('<option value="" disabled selected>Select OS image</option>')
	
	if($("#osType option:selected").text() == 'windows'){
		var winList = osList[0];
		for (index in winList) 
                    $("#osImageSelector").append('<option value=' + winList[index].Value + '>' + winList[index].Key +'</option>');              
	}else{
		var linList = osList[1];
		for (index in linList) 
                    $("#osImageSelector").append('<option value=' + linList[index].Value + '>' + linList[index].Key +'</option>');                   
	}	
}


function addDisks(){
	
	var diskItem = "<div ><input type='range' min='10' max='1000' value='10' oninput='change(this)'/> <span id='diskSize'>10GB</span> <button value='delete' onclick='deleteDisk(this)'>delete</button></div>"
	
	$("#diskList").append(diskItem);
	
	dataDisks = $("#diskList").children().length;
	
	var diskLeft = selectedVmSize.MaxDataDiskCount - dataDisks;
	
	$("#dataDisksLeft").text(diskLeft);
	
	if(diskLeft == 0 )$("#diskAdder").hide();
}

function change(obj){
	obj.parentElement.childNodes[2].innerHTML=obj.value + "GB";
}


function deleteDisk(obj){
	obj.parentNode.remove();
	dataDisks = $("#diskList").children().length;
	var diskLeft = selectedVmSize.MaxDataDiskCount - dataDisks;
	if(diskLeft >0) $("#diskAdder").show();
	$("#dataDisksLeft").text(diskLeft);
}

function CheckAndPurchase(obj){
	
	var clientID = CLIENT_TENANT;
	var clientsub = CLIENT_SUBSCRIPTION;
	
	if(
		$("#regionsSelecor").val() &&
		$("#osType").val() &&
		$("#osImageSelector").val() &&
		selectedVmSize 
	){
		vmParams.region = $("#regionsSelecor").val();
		vmParams.osType=$("#osType").val();
		vmParams.popOsImage = $("#osImageSelector").val();
		vmParams.vmSzie = selectedVmSize.Name;
		vmParams.dataDisks = dataDisks;
		if(vmParams.dataDisks > 0) vmParams.dataDisksDetails = getDataDiskDetails();
		vmParams.dataDisksDetails = getDataDiskDetails();
		
		obj.innerHTML="request sent , it will take about 15 mins , pls wait  "
		obj.disabled="disabled";
		
		$.ajax({
			type:"post",
			url:"/vmoptions/create/"+ clientID + "/" +  clientsub,
			data:vmParams,
			async:true,
			success:function(result){
				$("#result").text(result);
			},
			error: function(){
				$("#result").text("Error");
			}
		});
		
		
	}else{
		alert("pls check your vm options");
	}
}


function getDataDiskDetails(){
	
	var details = [];
	var listSzie = document.getElementById("diskList").childElementCount;
	var disklist = document.getElementById("diskList").children;
	
 	for(var i=0 ; i<listSzie; i++){
		var disk = {
			size:null,
			id:null
		}
		
		disk.size = disklist[i].firstChild.value;
		
		disk.id = "disk_" + i;
		details.push(disk);
	}
	
	return details;
}


function getType(obj){
	
	$("#timeSelector").show();
	
	var timetext = obj.value == 1 ?  "month(es)" : "year(s)"
	
	$("#timetext").text(timetext);
	
}
