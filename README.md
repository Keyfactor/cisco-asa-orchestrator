<h1 align="center" style="border-bottom: none">
    Cisco Asa Universal Orchestrator Extension
</h1>

<p align="center">
  <!-- Badges -->
<img src="https://img.shields.io/badge/integration_status-production-3D1973?style=flat-square" alt="Integration Status: production" />
<a href="https://github.com/Keyfactor/cisco-asa-orchestrator/releases"><img src="https://img.shields.io/github/v/release/Keyfactor/cisco-asa-orchestrator?style=flat-square" alt="Release" /></a>
<img src="https://img.shields.io/github/issues/Keyfactor/cisco-asa-orchestrator?style=flat-square" alt="Issues" />
<img src="https://img.shields.io/github/downloads/Keyfactor/cisco-asa-orchestrator/total?style=flat-square&label=downloads&color=28B905" alt="GitHub Downloads (all assets, all releases)" />
</p>

<p align="center">
  <!-- TOC -->
  <a href="#support">
    <b>Support</b>
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
  <a href="https://github.com/orgs/Keyfactor/repositories?q=orchestrator">
    <b>Related Integrations</b>
  </a>
</p>


## Overview

The Cisco Asa Universal Orchestrator extension is designed to manage Identity Certificates and TrustPoints on Cisco Asa devices. This extension is equipped to handle certificate bindings on the Remote Access VPN for the managed certificates. Please note that some of the actions depend on using the CLI through the API, which may return command line strings that could change with different versions of the Cisco Asa software, potentially affecting the inventory bindings functionality.

In the context of Keyfactor Command, defined Certificate Stores of the Cisco Asa Certificate Store Type represent containers for managing certificates on Cisco Asa devices. These stores facilitate operations such as inventorying existing certificates, adding new certificates, and removing certificates from the device. They can be defined to manage certificates within specific paths or configurations specific to Cisco Asa devices, making the overall process of certificate management streamlined and efficient.

## Compatibility

This integration is compatible with Keyfactor Universal Orchestrator version 10.2 and later.

## Support
The Cisco Asa Universal Orchestrator extension is supported by Keyfactor for Keyfactor customers. If you have a support issue, please open a support ticket with your Keyfactor representative. If you have a support issue, please open a support ticket via the Keyfactor Support Portal at https://support.keyfactor.com. 
 
> To report a problem or suggest a new feature, use the **[Issues](../../issues)** tab. If you want to contribute actual bug fixes or proposed enhancements, use the **[Pull requests](../../pulls)** tab.

## Installation
Before installing the Cisco Asa Universal Orchestrator extension, it's recommended to install [kfutil](https://github.com/Keyfactor/kfutil). Kfutil is a command-line tool that simplifies the process of creating store types, installing extensions, and instantiating certificate stores in Keyfactor Command.


1. Follow the [requirements section](docs/ciscoasa.md#requirements) to configure a Service Account and grant necessary API permissions.

    <details><summary>Requirements</summary>

    The configuration of the CiscoAsa Universal Orchestrator extension requires specific settings and considerations. Follow these steps to ensure proper setup and functionality:

    1. **Configure the Remote Platform (Cisco Asa) and API Access**:
       - Ensure that the Cisco Asa device is accessible over the network and that the API is enabled.
       - Verify that the device is running a compatible version of the Cisco Asa software.

    2. **Create a Service Account**:
       - Create an account on the Cisco Asa device with privilege level 15 access.
       - Note the username and password for this account, as they will be required when setting up the Certificate Store in Keyfactor Command.

    3. **Install the Keyfactor Universal Orchestrator Extension for CiscoAsa**:
       - Deploy the Cisco Asa Universal Orchestrator extension on a server with access to both Keyfactor Command and the Cisco Asa device.

    4. **Create the CiscoAsa Certificate Store Type**:
       - In Keyfactor Command, navigate to Certificate Store Types and create a new store type with the following basic and advanced settings:
         - **Name**: CiscoAsa
         - **Short Name**: CiscoAsa
         - **Supported Job Types**: Inventory, Add, Remove
         - **Needs Server**: Checked
         - **Blueprint Allowed**: Checked
         - **Store Path Type**: Freeform
         - **Supports Custom Alias**: Required
         - **Private Keys**: Required
         - **PFX Password Style**: Default or Custom

    5. **Create the CiscoAsa Certificate Store**:
       - In Keyfactor Command, navigate to Certificate Stores and create a new store using the previously defined CiscoAsa Certificate Store Type.
       - Provide the following information during store creation:
         - **Category**: CiscoAsa
         - **Client Machine**: Hostname or IP of the Cisco Asa device
         - **Store Path**: /Identity
         - **Orchestrator**: Select the orchestrator with CiscoAsa capability
         - **Commit To Disk**: True or False based on your requirements
         - **Server Username**: The service account username
         - **Server Password**: The service account password
         - **Use SSL**: Determine whether to use SSL for API connection

    By following these steps, you will ensure that the Cisco Asa device is properly configured for certificate management using the CiscoAsa Universal Orchestrator extension in Keyfactor Command.



    </details>

2. Create Certificate Store Types for the Cisco Asa Orchestrator extension. 

    * **Using kfutil**:

        ```shell
        # CiscoAsa
        kfutil store-types create CiscoAsa
        ```

    * **Manually**:
        * [CiscoAsa](docs/ciscoasa.md#certificate-store-type-configuration)

3. Install the Cisco Asa Universal Orchestrator extension.
    
    * **Using kfutil**: On the server that that hosts the Universal Orchestrator, run the following command:

        ```shell
        # Windows Server
        kfutil orchestrator extension -e cisco-asa-orchestrator@latest --out "C:\Program Files\Keyfactor\Keyfactor Orchestrator\extensions"

        # Linux
        kfutil orchestrator extension -e cisco-asa-orchestrator@latest --out "/opt/keyfactor/orchestrator/extensions"
        ```

    * **Manually**: Follow the [official Command documentation](https://software.keyfactor.com/Core-OnPrem/Current/Content/InstallingAgents/NetCoreOrchestrator/CustomExtensions.htm?Highlight=extensions) to install the latest [Cisco Asa Universal Orchestrator extension](https://github.com/Keyfactor/cisco-asa-orchestrator/releases/latest).

4. Create new certificate stores in Keyfactor Command for the Sample Universal Orchestrator extension.

    * [CiscoAsa](docs/ciscoasa.md#certificate-store-configuration)



## License

Apache License 2.0, see [LICENSE](LICENSE).

## Related Integrations

See all [Keyfactor Universal Orchestrator extensions](https://github.com/orgs/Keyfactor/repositories?q=orchestrator).