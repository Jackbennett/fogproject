<?php
require_once('../commons/base.inc.php');
if ($_SESSION["foglastreport"] != null)
{
	$report = unserialize($_SESSION["foglastreport"]);
	$report->setFileName($_REQUEST['filename']);
	if ($_REQUEST["type"] == "csv")
		$report->outputReport(ReportMaker::FOG_REPORT_CSV);
	else if ( $_REQUEST["type"] == "pdf" )
		$report->outputReport(ReportMaker::FOG_REPORT_PDF);
	else if ($_REQUEST["type"] == "host")
		$report->outputReport(ReportMaker::FOG_EXPORT_HOST);
	else if ($_REQUEST["type"] == "sql")
		$report->outputReport(ReportMaker::FOG_BACKUP_SQL);
}
else if (isset($_REQUEST['export']))
{
	$report = new ReportMaker();
	$report->outputReport(ReportMaker::FOG_BACKUP_SQL);
}
