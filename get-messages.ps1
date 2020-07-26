#connect-ExchangeOnline -ShowProgress $true
function Get-Messages
{ 
      Param (
          [Parameter(Mandatory=$false)]
          # Date 'MM/dd/yyyy'
          [String]$StartReceivedDate = (Get-Date).addMonths(-1)
      )
    $m = Get-QuarantineMessage -StartExpiresDate (Get-Date).Addhours(-24) -PageSize 1000 -QuarantineTypes malware, spam, phish
    #QuarantineMessage with html/htm - likly call phishing
    $callm = $m | Preview-QuarantineMessage | where {$_.attachment -like '*.html' -or $_.attachment -like '*.htm'}
    foreach($call in $callm)
    {
        $call_identity = $call | select -ExpandProperty Identity
        $tmp = Export-QuarantineMessage -Identity $call_identity
        $tmp.eml | ."$PSScriptRoot\EPO-Auto-IOC\EPO-Auto-IOC.exe"
    }
}