# GlobalSign CAPlugin Configuration

## Overview
The GlobalSign CAPlugin enables the Synchronization, Enrollment, and Revocation of TLS Certificates from the GlobalSign Certificate Center.

## Requirements

### Certificate Chain
To enroll for certificates, the Keyfactor Command server must trust the certificate chain. After creating your Root and/or Subordinate CA, ensure the certificate chain is imported into the AnyGateway and Command Server certificate store.

### API Allow List
The GlobalSign API filters requests based on IP addresses. Ensure the appropriate IP addresses are allowed to make requests to the GlobalSign API.

### Domain Point of Contact
This extension uses the contact information of the GCC Domain point of contact for certificate enrollment. These fields are required for submission and must be populated in the Domain's point of contact section, which can be found in the GlobalSign Portal under the **Manage Domains** page.

## Gateway Registration
GlobalSign supports the following Root certificates: [GlobalSign Root Certificates](https://support.globalsign.com/ca-certificates/root-certificates/globalsign-root-certificates).  
**Root_R3** is commonly used throughout MSSL. Define the root certificate you wish to use on the Gateway registration tab.  
Each additional Root will require a separate CA setup.

## Valid GlobalSign SAN Usage
GlobalSign supports specific combinations of SAN types with certain products. For example, a Private IP can only be used as a SAN with a `PV_INTRA` Certificate.  
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
