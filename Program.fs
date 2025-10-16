open System

open Argu
open FsHttp
open Microsoft.Identity.Client
open System.Net

/// CLI argument declaration
type CliArguments =
    // Connection params
    | [<ExactlyOnce>] Company_Id of string // Derives TOKEN_URL
    | [<ExactlyOnce>] Client_Id of string
    | [<ExactlyOnce>] Client_Secret of string
    // Payload params
    | [<ExactlyOnce>] Legal_Entity of string
    | [<ExactlyOnce>] Message_Queue of string
    | [<ExactlyOnce>] Message_Type of string
    | [<ExactlyOnce>] Url of string // Derives SCOPE
    | [<Unique>] Mock
    | [<MainCommand; Mandatory>] Message of path: string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Legal_Entity _ -> "F&O legal entity; a STRING"
            | Message_Queue _ -> "message queue name; a STRING"
            | Message_Type _ -> "message type; a STRING"
            | Client_Id _ -> "OAuth2 client id; a UUID"
            | Client_Secret _ -> "OAuth2 client secret; a UUID"
            | Company_Id _ -> "Microsoft company id; a GUID"
            | Url _ -> "base url of the F&O instance; a URL"
            | Mock -> "if set, display the message queue request instead of sending it"
            | Message _ -> "path to JSON file containing message payload"

/// Sends a message to a F&O message processor queue
let sendMessage token baseUrl companyId messageQueue messageType payload =
    http {
        config_useBaseUrl baseUrl
        POST "/api/services/SysMessageServices/SysMessageService/sendMessage"

        AuthorizationBearer token

        body

        jsonSerialize {|
            _companyId = companyId
            _messageQueue = messageQueue
            _messageType = messageType
            _messageContent = payload
        |}
    }

// SCRIPT
[<EntryPoint>]
let main argv =
    // CLI interface
    let errorHandler =
        ProcessExiter(
            colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red
        )

    let parser =
        ArgumentParser.Create<CliArguments>(
            programName = "fnonq",
            helpTextMessage = "[Finance aNd Operations eNQueue] Easily add messages to a F&O Message Processor queue\n",
            errorHandler = errorHandler
        )

    let results = parser.Parse argv

    // Arguments
    let CLIENT_ID = results.GetResult Client_Id
    let CLIENT_SECRET = results.GetResult Client_Secret

    let AUTH_URL =
        $"https://login.microsoftonline.com/%s{results.GetResult Company_Id}/oauth2/v2.0/token"

    let SCOPE = results.GetResult Url + "//.default"
    let LEGAL_ENTITY = results.GetResult Legal_Entity
    let MESSAGE_QUEUE = results.GetResult Message_Queue
    let MESSAGE_TYPE = results.GetResult Message_Type
    let CONTENT = results.GetResult Message |> System.IO.File.ReadAllText
    let MOCK = results.Contains Mock
    let BASE_URL = results.GetResult Url

    // Application logic
    if MOCK then
        sendMessage "PLACEHOLDER" BASE_URL LEGAL_ENTITY MESSAGE_QUEUE MESSAGE_TYPE CONTENT
        |> Request.print
        |> printfn "%s"

        0
    else
        let app =
            ConfidentialClientApplicationBuilder
                .Create(CLIENT_ID)
                .WithClientSecret(CLIENT_SECRET)
                .WithAuthority(AUTH_URL)
                .Build()

        let TOKEN =
            app.AcquireTokenForClient([| SCOPE |]).ExecuteAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> _.AccessToken

        let response =
            sendMessage TOKEN BASE_URL LEGAL_ENTITY MESSAGE_QUEUE MESSAGE_TYPE CONTENT
            |> Request.send

        if response.statusCode <> HttpStatusCode.OK then
            printfn "%i %s" (int response.statusCode) response.reasonPhrase
            Response.toText response |> printfn "%s"
            1
        else
            printfn "Message queued successfully"
            0
