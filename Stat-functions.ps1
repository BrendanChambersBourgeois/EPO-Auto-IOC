#connect-ExchangeOnline -ShowProgress $true

function Get-MailTrafficReport-Percent{ 
      Param (
          [Parameter(Mandatory=$false)]
          # Date 'MM/dd/yyyy'
          [String]$StartDate = (Get-Date).addMonths(-1),
          [Parameter(Mandatory=$false)]
          # Date 'MM/dd/yyyy'
          [string]$EndDate = (Get-Date),
          # Type of query 
          [Parameter(Mandatory=$false)]
          [ValidateSet("TopSpamRecipient", "TopMailSender", "TopMailRecipient", "TopMalwareRecipient", "TopMalware", "MailTrafficReport")]
          [String]$Category = "TopSpamRecipient"
      )

$Report = 0

# MailTrafficReport needs to be seperated as it used diffrent parameters. 
if($Category -ne "MailTrafficReport"){
    $Report = Get-MailTrafficSummaryReport -Category $Category -StartDate $startdate -EndDate $enddate | 
    # Groups name feild and adds them togetherand creats a total count
    Group C1 | select Name, @{N='Message Count';E={($_.Group|Measure-Object -Property C2 -sum).Sum}}
}else {
    $Report = Get-MailTrafficReport -AggregateBy Summary -StartDate $startdate -EndDate $enddate | where {$_.Direction -eq 'Inbound'} | 
    # Groups name action and adds them together to create a Message Count total
    Where {$_.Action -ne ''} | Group action | select Name, @{N='Message Count';E={($_.Group|Measure-Object -Property messagecount -sum).Sum}}
}

# counts every value and adds them for % calculation then dones it in next command.
$Report | select @{Name=$Category;Expression={$_.Name}}, 'Message Count', @{L='Percent';E={"{0:N2}"-f (($_.'Message Count'/($Report | Measure 'Message Count' -sum).sum).ToString("P"))}} | 
sort -Property 'Message Count' -Descending | select -first 30
}

function Get-QuarantineMessage-Percent{ 
      Param (
          [Parameter(Mandatory=$false)]
          # Date 'MM/dd/yyyy'
          [String]$StartReceivedDate = (Get-Date).addMonths(-1)
      )

$Report = 0

# MailTrafficReport needs to be seperated as it used diffrent parameters. 

$Report = Get-QuarantineMessage -QuarantineTypes Phish, HighConfPhish -StartReceivedDate $StartReceivedDate -pagesize 1000 

# Group together and counting of top RecipientAddress
$Report = $Report | select -ExpandProperty RecipientAddress | group

# Display Top Phish and HighConfPhish in one table with %
$Report | 
select (
@{N="Top Phish Recipient";E={$_.Name}},
@{N="Message Count";E={$_.Count}},
@{L="Percent";E={"{0:N2}"-f (($_.Count/($Report | Measure count -sum).sum).ToString("P"))}}) | 
sort -Property "Message Count" -Descending | select -first 15
}

function Get-QuarantineMessage-send-Percent{ 
      Param (
          [Parameter(Mandatory=$false)]
          # Date 'MM/dd/yyyy'
          [String]$StartReceivedDate = (Get-Date).addMonths(-1)
      )

$Report = 0

# MailTrafficReport needs to be seperated as it used diffrent parameters. 

$Report = Get-QuarantineMessage -QuarantineTypes Phish, HighConfPhish -StartReceivedDate $StartReceivedDate -pagesize 1000 

# Group together and counting of top RecipientAddress
$Report = $Report | select -ExpandProperty senderAddress | group

# Display Top Phish and HighConfPhish in one table with %
$Report | 
select (
@{N="Top Phish Sender";E={$_.Name}},
@{N="Message Count";E={$_.Count}},
@{L="Percent";E={"{0:N2}"-f (($_.Count/($Report | Measure count -sum).sum).ToString("P"))}}) | 
sort -Property "Message Count" -Descending | select -first 15
}
