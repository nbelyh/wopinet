# Introduction 

This is a .NET7/8 port of the Microsoft WOPI Framework sample (see links below)
Not affiliated with Microsoft in any way, beside taking the WOPIFramework repo as a base.

# Getting Started

The project contains the "core" project (library) for implementing a custom WOPI server,
providing basic interfaces (to be implemented) for storage and state management.

Contains a sample (test) implementation based on the above library.
The sample assumes Microsoft SQL Database for metadata (locks), and Azure Storage for files.

If configured correctly, should pass the WOPI Validator for read/edit.

The repo also contains a simple frontend anguar sample.

References:
- [WOPIFramework](https://github.com/apulliam/WOPIFramework) - using as a base (rewritten)
- [WopiHost](https://github.com/petrsvihlik/WopiHost) - not using, because of in-memory management for locks, and the lack of PROOF support
- [PnP-WOPI](https://github.com/OfficeDev/PnP-WOPI) - not using, too old

# Build and Test

TODO
