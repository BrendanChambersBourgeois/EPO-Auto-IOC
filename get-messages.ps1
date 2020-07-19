#connect-ExchangeOnline -ShowProgress $true

function Get-Messages{ 
      Param (
          [Parameter(Mandatory=$false)]
          # Date 'MM/dd/yyyy'
          [String]$StartReceivedDate = (Get-Date).addMonths(-1)
      )
$m = {}
$callm = {}
#QuarantineMessage
$m = Get-QuarantineMessage -StartReceivedDate (Get-Date).AddDays(-30) -PageSize 100 -QuarantineTypes malware
#QuarantineMessage with html/htm - likly call phishing
$callm = $m | Preview-QuarantineMessage | where {$_.attachment -like '*.html' -or $_.attachment -like '*.htm'}
foreach($call in $callm){
$call_identity = $call | select -ExpandProperty Identity
$call_file_path = $call_identity -replace "\\", "-"
$pwd = Get-Location
$e = Export-QuarantineMessage -Identity $call_identity; $bytes = [Convert]::FromBase64String($e.eml); [IO.File]::WriteAllBytes("$PSScriptRoot\$call_file_path.eml", $bytes)
write-host $call_file_path 'saved'
}
}