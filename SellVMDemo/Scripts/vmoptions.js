var CLIENT_TENANT = "e4c9ab4e-bd27-40d5-8459-230ba2a757fb" ;
var CLIENT_SUBSCRIPTION = "e5b0fcfa-e859-43f3-8d84-5e5fe29f4c68";

function getRegions() {
	
	if($("#regionsSelecor").children().length == 1){
		
		$.ajax({
			type:"get",
            url:"/vmoptions/regions",
			async:true,
            success: function (result) {
                for (index in result) {
                    $("#regionsSelecor").append('<option value=' + result[index].Name + '>' + result[index].Name+'</option>');              
                }
			}
		});	
    }        
}

function regionSelect(){
	
	var region = $("#regionsSelecor option:selected").text();
	
	$.ajax({
			type:"get",
            url:"/vmoptions/avaliable",
			async:true,
            success: function (result) {
                for (index in result) {
                    $("#regionsSelecor").append('<option value=' + result[index].Name + '>' + result[index].Name+'</option>');              
                }
			}
		});	
	
}
    