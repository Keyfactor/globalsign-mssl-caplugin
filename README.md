<h1 align="center" style="border-bottom: none">
    GlobalSign MSSL AnyCA Gateway REST Plugin
</h1>

<p align="center">
  <!-- Badges -->
<img src="https://img.shields.io/badge/integration_status-production-3D1973?style=flat-square" alt="Integration Status: production" />
<a href="https://github.com/Keyfactor/globalsign-mssl-caplugin/releases"><img src="https://img.shields.io/github/v/release/Keyfactor/globalsign-mssl-caplugin?style=flat-square" alt="Release" /></a>
<img src="https://img.shields.io/github/issues/Keyfactor/globalsign-mssl-caplugin?style=flat-square" alt="Issues" />
<img src="https://img.shields.io/github/downloads/Keyfactor/globalsign-mssl-caplugin/total?style=flat-square&label=downloads&color=28B905" alt="GitHub Downloads (all assets, all releases)" />
</p>

<p align="center">
  <!-- TOC -->
  <a href="#support">
    <b>Support</b>
  </a> 
  ·
  <a href="#requirements">
    <b>Requirements</b>
  </a>
  ·
  <a href="#installation">
    <b>Installation</b>
  </a>
  ·
  <a href="#license">
    <b>License</b>
  </a>
  ·
  <a href="https://github.com/orgs/Keyfactor/repositories?q=anycagateway">
    <b>Related Integrations</b>
  </a>
</p>


The GlobalSign CAPlugin enables the Synchronization, Enrollment, and Revocation of TLS Certificates from the GlobalSign Certificate Center.

## Compatibility

The GlobalSign MSSL AnyCA Gateway REST plugin is compatible with the Keyfactor AnyCA Gateway REST 25.2.0 and later.

## Support
The GlobalSign MSSL AnyCA Gateway REST plugin is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket with your Keyfactor representative. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com. 

> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Requirements

### Certificate Chain
To enroll for certificates, the Keyfactor Command server must trust the certificate chain. After creating your Root and/or Subordinate CA, ensure the certificate chain is imported into the AnyGateway and Command Server certificate store.

### API Allow List
The GlobalSign API filters requests based on IP addresses. Ensure the appropriate IP addresses are allowed to make requests to the GlobalSign API.

### Domain Point of Contact
This extension uses the contact information of the GCC Domain point of contact for certificate enrollment. These fields are required for submission and must be populated in the Domain's point of contact section, which can be found in the GlobalSign Portal under the **Manage Domains** page.

## Installation

1. Install the AnyCA Gateway REST per the [official Keyfactor documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/InstallIntroduction.htm).

2. On the server hosting the AnyCA Gateway REST, download and unzip the latest [GlobalSign MSSL AnyCA Gateway REST plugin](https://github.com/Keyfactor/globalsign-mssl-caplugin/releases/latest) from GitHub.

3. Copy the unzipped directory (usually called `net6.0` or `net8.0`) to the Extensions directory:


    ```shell
    Depending on your AnyCA Gateway REST version, copy the unzipped directory to one of the following locations:
    Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net6.0\Extensions
    Program Files\Keyfactor\AnyCA Gateway\AnyGatewayREST\net8.0\Extensions
    ```

    > The directory containing the GlobalSign MSSL AnyCA Gateway REST plugin DLLs (`net6.0` or `net8.0`) can be named anything, as long as it is unique within the `Extensions` directory.

4. Restart the AnyCA Gateway REST service.

5. Navigate to the AnyCA Gateway REST portal and verify that the Gateway recognizes the GlobalSign MSSL plugin by hovering over the ⓘ symbol to the right of the Gateway on the top left of the portal.

## Configuration

1. Follow the [official AnyCA Gateway REST documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCA-Gateway.htm) to define a new Certificate Authority, and use the notes below to configure the **Gateway Registration** and **CA Connection** tabs:

    * **Gateway Registration**

        GlobalSign supports the following Root certificates: [GlobalSign Root Certificates](https://support.globalsign.com/ca-certificates/root-certificates/globalsign-root-certificates).  
        **Root_R3** is commonly used throughout MSSL. Define the root certificate you wish to use on the Gateway registration tab.  
        Each additional Root will require a separate CA setup.

    * **CA Connection**

        Populate using the configuration fields collected in the [requirements](#requirements) section.

        * **GlobalSignUsername** - GlobalSign MSSL API Username 
        * **GlobalSignPassword** - GlobalSign MSSL API Password 
        * **DateFormatString** - Date format string. Default is yyyy-MM-ddTHH:mm:ss.fffZ 
        * **OrderAPIProdURL** - MSSL Order Prod API URL. Default is https://system.globalsign.com/kb/ws/v2/ManagedSSLService 
        * **OrderAPITestURL** - MSSL Order Test API URL. Default is https://test-gcc.globalsign.com/kb/ws/v2/ManagedSSLService 
        * **QueryAPIProdURL** - MSSL Query Prod API URL. Default is https://system.globalsign.com/kb/ws/v1/GASService 
        * **QueryAPITestURL** - MSSL Query Test API URL. Default is https://test-gcc.globalsign.com/kb/ws/v1/GASService 
        * **TestAPI** - Enable the use of the test GlobalSign API endpoints. Default is false. 
        * **DelayTime** - This is the number of seconds between retries when attempting to download a certificate. Default is 150. 
        * **RetryCount** - This is the number of times the AnyGateway will attempt to pickup an new certificate before reporting an error. Default is 5. 
        * **SyncIntervalDays** - OPTIONAL: Required if SyncStartDate is used. Specifies how to page the certificate sync. Should be a value such that no interval of that length contains > 500 certificate enrollments. 
        * **SyncStartDate** - If provided, full syncs will start at the specified date. 
        * **SyncProducts** - OPTIONAL: If provided as a comma-separated list of product IDs, will limit the certificate sync to only certificates of those products. If blank or not provided, will sync all certs. 
        * **Enabled** - Flag to Enable or Disable gateway functionality. Disabling is primarily used to allow creation of the CA prior to configuration information being available. 

2. Define [Certificate Profiles](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCP-Gateway.htm) and [Certificate Templates](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCA-Gateway.htm) for the Certificate Authority as required. One Certificate Profile must be defined per Certificate Template. It's recommended that each Certificate Profile be named after the Product ID. The GlobalSign MSSL plugin supports the following product IDs:

    * **PEV_SHA2**
    * **PEV**
    * **PV**
    * **PV_SHA2**
    * **PV_INTRA**
    * **PV_INTRA_SHA2**
    * **PV_INTRA_ECCP256**
    * **PV_CLOUD**
    * **PV_CLOUD_ECC2**

3. Follow the [official Keyfactor documentation](https://software.keyfactor.com/Guides/AnyCAGatewayREST/Content/AnyCAGatewayREST/AddCA-Keyfactor.htm) to add each defined Certificate Authority to Keyfactor Command and import the newly defined Certificate Templates.

4. In Keyfactor Command (v12.3+), for each imported Certificate Template, follow the [official documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/ReferenceGuide/Configuring%20Template%20Options.htm) to define enrollment fields for each of the following parameters:

    * **CertificateValidityInYears** - Number of years the certificate will be valid for 
    * **SlotSize** - Maximum number of SANs that a certificate may have - valid values are [FIVE, TEN, FIFTEEN, TWENTY, THIRTY, FOURTY, FIFTY, ONE_HUNDRED] 
    * **RootCAType** - The certificate's root CA - Depending on certificate expiration date, SHA_1 not be allowed. Will default to SHA_2 if expiration date exceeds sha1 allowed date. Options are GlobalSign R certs. 
    * **MSSLProfileId** - OPTIONAL: If specified, enrollments will use that profile ID for domain lookups. If not provided, domain lookup will be done based on the Common Name or first DNS SAN. Useful if your GlobalSign account has multiple domain objects with the same domain string, or subdomains (e.g. sub.test.com vs test.com). 


## Valid GlobalSign SAN Usage
GlobalSign supports specific combinations of SAN types with certain GlobalSign products. For example, a Private IP can only be used as a SAN with a `PV_INTRA` Certificate.  
Please refer to the GlobalSign documentation for more information on SAN usage:  [GlobalSign MSSL API User Guide (Section 2.2.5)](https://www.globalsign.com/en/repository/globalsign-mssl-api-user-guide.pdf)

## Enrollment Fields

### Required Enrollment Fields
The following fields are required for enrollment on all certificate templates:
- **ContactName**: Set Data Type to 'string' when creating the field. The name of the contact person for the certificate. This is required by the GlobalSign API.

### PV_INTRA Specific Enrollment Fields
The following fields are available for use in the enrollment of `PV_INTRA` Certificates:
- **PrivateDomain**: Set Data Type to 'string' when creating the field. Set to `true` if enrolling a certificate for a private domain (e.g., `.local`, `.lab`, etc.).
  - **If PrivateDomain is set to `true`, the following fields must also be specified:**
    - **RequesterEmail**: Set Data Type to 'string' when creating the field. The contact email address for the enrollment. Required by the GlobalSign API.
    - **RequesterTel**: Set Data Type to 'string' when creating the field. The contact telephone number for the enrollment. Required by the GlobalSign API.
- **InternalIP**: Set Data Type to 'string' when creating the field. Set to `true` if an IP SAN attached during a `PV_INTRA` certificate enrollment is a private IP address (e.g., `10.x.x.x`, `192.168.x.x`, etc.).


## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Any CA Gateways (REST)](https://github.com/orgs/Keyfactor/repositories?q=anycagateway).