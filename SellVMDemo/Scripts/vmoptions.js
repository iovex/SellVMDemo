var resions = null;

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
	
	alert($("#regionsSelecor option:selected").text());
	
}
    