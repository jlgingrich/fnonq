# fnonq

> **F**inance a**N**d **O**perations e**NQ**ueue

A `dotnet` tool that allows you to easily send messages to a [F&O Message Processor queue](https://learn.microsoft.com/en-us/dynamics365/supply-chain/message-processor/message-processor).

## Installation

1. Place `fnonw.1.X.X.nupkg` file in Downloads folder.
2. Run `dotnet tool install --global --add-source . fnonq`.
3. Try it out: `fnonq --help`.
4. ...Profit?
5. To uninstall, run `dotnet tool uninstall --global fnonq`

## Help

```txt
[Finance aNd Operations eNQueue] Easily add messages to a F&O Message Processor queue

USAGE: fnonq [--help] --company-id <string> --client-id <string> --client-secret <string> --legal-entity <string> --message-queue <string> --message-type <string> --url <string> [--mock]
             <path>

MESSAGE:

    <path>                path to JSON file containing message payload

OPTIONS:

    --company-id <string> Microsoft company id; a GUID
    --client-id <string>  OAuth2 client id; a UUID
    --client-secret <string>
                          OAuth2 client secret; a UUID
    --legal-entity <string>
                          F&O legal entity; a STRING
    --message-queue <string>
                          message queue name; a STRING
    --message-type <string>
                          message type; a STRING
    --url <string>        base url of the F&O instance; a URL
    --mock                if set, display the message queue request instead of sending it
    --help                display this list of options.
```

## Example Use

This Powershell script is annotated and split onto multiple lines for clarity.

```powershell
fnonq `
<# Company id for authentication according to MSFT #> `
--company-id '12345657-1234-1234-1234-1234567890ab' `
<# OAuth2 client id #> `
--client-id '12345657-1234-1234-1234-1234567890ab' `
<# OAuth2 client secret #> `
--client-secret '12345657-1234-1234-1234-1234567890ab' `
<# URL of target F&O environment #> `
--url 'https://your-environment.operations.dynamics.com' `
<# Target Legal entity id #> `
--legal-entity 'USMF' `
<# Message queue id #> `
--message-queue 'MESSAGE_QUEUE' `
<# Message type id #> `
--message-type 'MESSAGE_TYPE' `
 <# Add if you want to see the raw HTTP request it would've sent #> `
--mock `
<# Path to message payload #> `
message-to-send.json
```

## Building

1. Run `dotnet pack` in this directory.
2. Distribute the file `./bin/Release/fnonq.1.X.X.nupkg`.
3. See [installation instructions](#installation) above.
