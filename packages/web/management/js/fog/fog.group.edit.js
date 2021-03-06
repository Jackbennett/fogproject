$(function() {
	// Just hide the group info
	$('#hostNoGroup').hide();
	// Checkbox toggles
	$('.toggle-checkbox1').click(function() {
		$('input.toggle-host1:checkbox').attr('checked', ($(this).attr('checked') ? 'checked' : false));
	});
	$('.toggle-checkbox2').click(function() {
		$('input.toggle-host2:checkbox').attr('checked', ($(this).attr('checked') ? 'checked' : false));
	});
	$('.toggle-checkboxprint').click(function() {
		$('input.toggle-print:checkbox').attr('checked', ($(this).attr('checked') ? 'checked' : false));
	});
	$('.toggle-checkboxprintrm').click(function() {
		$('input.toggle-printrm:checkbox').attr('checked', ($(this).attr('checked') ? 'checked' : false));
	});
	$('.toggle-checkboxsnapin').click(function() {
		$('input.toggle-snapin:checkbox').attr('checked', ($(this).attr('checked') ? 'checked' : false));
	});
	$('.toggle-checkboxsnapinrm').click(function() {
		$('input.toggle-snapinrm:checkbox').attr('checked', ($(this).attr('checked') ? 'checked' : false));
	});
	// Show hide based on checked state.
	$('#hostNotInMe').hide();
	$('#hostNoGroup').hide();
	$('#hostMeShow').click(function() {
		$('#hostNotInMe').toggle();
	});
	$('#hostNoShow').click(function() {
		$('#hostNoGroup').toggle();
	});
});
