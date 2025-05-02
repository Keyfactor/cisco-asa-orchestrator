## Overview

The Cisco Asa Orchestrator Manages Only Identity Certificates and TrustPoints on the Cisco Asa Device.  It manages bindings on the Remote Access VPN for those certificates.

**Note:** Some of the functionality uses the CLI through the API which returns command line strings.  This may be fragile especially inventory bindings if the CLI return changes between versions of the product.

## Test Cases

<details>
<summary>Cisco Asa</summary>

Case Number|Case Name|Enrollment Params|Expected Results|Passed|Screenshot
----|------------------------|------------------------------------|--------------|----------------|-------------------------
1|Inventory|N/A|Identity certificates only will be inventoried on the the device and appear in the Cisco Asa Store in Keyfactor Command|True|![](Images/TC1-Inventory.gif)
2|New Cert Enrollment To Identiy Certs with No Bindings|**Alias:** NoBindingCertTC2 **Interfaces Comma Separated:**|Cert will be Installed to Identity Certificates with Trustpoint name of NoBindingCertTC2|True|![](Images/TC2-EnrollNoBindings.gif)
3|New Cert Enrollment To Identiy Certs with Bindings|**Alias:** TC3EnrollWithBindings **Interfaces Comma Separated:** inside,dmz|Cert will be Installed to Identity Certificates with Trustpoint name of TC3EnrollWithBindings and bound to inside and dmz interfaces|True|![](Images/TC3-EnrollWithBindings.gif)
4|Renew Cert To Identiy Certs with No Bindings|**Alias:** NoBindingCertTC2 **Interfaces Comma Separated:**|Cert will Renewed to Identity Certificates with Trustpoint name of NoBindingCertTC2doublepipeSomeDateInt this is *not* a replace to prevent downtime.|True|![](Images/TC4-RenewNoBindings.gif)
5|Renew Cert To Identiy Certs with Bindings|**Alias:** TC3EnrollWithBindings **Interfaces Comma Separated:** inside,dmz|Cert will Renewed to Identity Certificates with Trustpoint name of TC3EnrollWithBindingsdoublepipeSomeDateInt this is *not* a replace to prevent downtime.  It will also be bound to dmz and inside interfaces.|True|![](Images/TC5-RenewNoBindings.gif)
6|Attempted Replace without overwrite flag To Identiy Certs with Bindings|**Alias:** TC3EnrollWithBindings **Interfaces Comma Separated:** inside,dmz|Job will fail and the result will indicate to use the overwrite flag|True|![](Images/TC6-SameNameNoOverwriteFlag.gif)
7|Remove Cert With Bindings|**Alias:** TC3EnrollWithBindingsdoublepipe587638726 **Interfaces Comma Separated:** inside,dmz|Identity Certifite will be deleted and bindings removed.|True|![](Images/TC7-RemoveCertWithBindings.gif)
8|Commit To Disk Test|**Alias:** CommitToDiskTC **Interfaces Comma Separated:**|After Enrollment the startup config and running config in Cisco Asa Should be the same|True|![](Images/TC8-CommitToDiskTest.gif)
9|Attempted Replace with overwrite flag To Identiy Certs with Bindings|**Alias:** TC3EnrollWithBindings **Interfaces Comma Separated:** inside,dmz|Since overwrite flag was used, it will add and rebind a new certificate named TC3EnrollWithBindingsdoublepipeSomeDateInt this is *not* a replace to prevent downtime|True|![](Images/TC9-SameNameWithOverwriteFlag.gif)

</details>
