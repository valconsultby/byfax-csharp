PRODUCT
-------------------

byFax is an online faxing platform that allows you to send and receive faxes without fax machines or any other devices directly in your browser or through the API integration presented here.
More information about the platform and its abilities can be found on the <a href="https://byfax.biz">byFax</a> website (https://byfax.biz).


REQUIREMENTS
------------

The minimum requirement for this project is .NET framework 3.0. Sample project was created for .NET framework 4.8


SERVICES
------------

To get started with the API, you need to create a byFax application and get the api-key and api-secret to authorize your application to the API.
As you implement your solution, you have a fully functional test environment at https://sandbox.byfax.biz
When your solution is complete, the base url for services changes to the production environment to https://api.byfax.biz
See the services list below.

- cover - cover page management. <a href="https://api.byfax.biz/soap/1.1/cover" targe='__blank'>[Detailed description and WSDL link]</a>
- document - fax documents cache management. <a href="https://api.byfax.biz/soap/1.1/document" targe='__blank'>[Detailed description and WSDL link]</a>
- faxout - sending fax and monitoring delivery status and downloading faxes as PDF files. <a href="https://api.byfax.biz/faxout" targe='__blank'>[Detailed description and WSDL link]</a>
- faxin/message - obtain list of received faxes and downloading as PDF files. <a href="https://api.byfax.biz/soap/1.1/faxin/message" targe='__blank'>[Detailed description and WSDL link]</a>
- faxin/inventory - receiving data about assigned virtual fax-numbers. <a href="https://api.byfax.biz/soap/1.1/faxin/inventory" targe='__blank'>[Detailed description and WSDL link]</a>


SAMPLES
------------

## Authorization
The application is authorized to the API using a special SOAP header that contains api-key and api-secret. The header is represented by a UsernameToken object.

The authorization header is passed in every request, the connection to the API has no sessions and no additional tokens.

```cs
// Replace ApiKey and ApiSecret with data from your application settings
string apiSecret = "YOUR-API-KEY";
string apiKey = "YOUR-API-SECRET";

// Replace with https://api.byfax.biz for production
string apiUrl = "https://sandbox.byfax.biz";

// Create and fill auth token object
apiService.UsernameTokenValue = new cover.UsernameToken();
apiService.UsernameTokenValue.Password = apiSecret;
apiService.UsernameTokenValue.Username = apiKey;

// Create service object and fill base params. Example on Document service
document.ApiServiceDocument apiService = new document.ApiServiceDocument();
apiService.Url = $"{apiUrl}/soap/1.1/document";
apiService.Timeout = 2 * 60 * 1000;
```

## Cover pages. List
Cover Pages are available for more personalized faxing in byFax. The system has already been loaded with a basic set of cover pages, which are available via API as well as in the byFax customer portal.

Both portal users and API developers in their applications have the ability to add own custom cover pages. The cover page is a DocX file with predefined placeholders that are replaced with the sender and recipient data during the sending process.

```cs
// Replace ApiKey and ApiSecret with data from your application settings
string apiSecret = "YOUR-API-KEY";
string apiKey = "YOUR-API-SECRET";

// Replace with https://api.byfax.biz for production
string apiUrl = "https://sandbox.byfax.biz";

// Create and fill auth token object
apiService.UsernameTokenValue = new cover.UsernameToken();
apiService.UsernameTokenValue.Password = apiSecret;
apiService.UsernameTokenValue.Username = apiKey;

// Create service object and fill base params
cover.ApiServiceCover apiService = new cover.ApiServiceCover();
apiService.Url = $"{apiUrl}/soap/1.1/cover";
apiService.Timeout = 2 * 60 * 1000;

// call "listCovers" function and obtain response
cover.ApiResponseListCovers response = apiService.listCovers();

// Check response state code for success and return false if failed
if (response.stateCode == cover.StateCodes.SUCCESS)
{
    foreach (var item in response.items)
    {
        Console.WriteLine($"{item.coverTitle} - {item.coverRef}");
    }
    Console.WriteLine();
    return true;
}
else
{
    return false;
}
```      

## Cover pages. Add new
To add a cover page, you should upload a DocX file to the system and setup its name.

```cs
// Replace ApiKey and ApiSecret with data from your application settings
string apiSecret = "YOUR-API-KEY";
string apiKey = "YOUR-API-SECRET";

// Replace with https://api.byfax.biz for production
string apiUrl = "https://sandbox.byfax.biz";

// Create and fill auth token object
apiService.UsernameTokenValue = new cover.UsernameToken();
apiService.UsernameTokenValue.Password = apiSecret;
apiService.UsernameTokenValue.Username = apiKey;

// Create service object and fill base params
cover.ApiServiceCover apiService = new cover.ApiServiceCover();
apiService.Url = $"{apiUrl}/soap/1.1/cover";
apiService.Timeout = 2 * 60 * 1000;

string path = "C:\\cover.docx";

// Create and fill FaxFile object
cover.FaxFile faxFile = new cover.FaxFile();

faxFile.fileCheck = Md5Generator.FromFile(path);
faxFile.fileName = System.IO.Path.GetFileName(path);
faxFile.fileSize = new System.IO.FileInfo(path).Length;
// Important to set that "fileSize" property is specified
faxFile.fileSizeSpecified = true;
// Read file data without encoding to Base64
faxFile.fileData = System.IO.File.ReadAllBytes(path);

// Call Api function "addCover" to upload a new cover page
cover.ApiResponseCoverUpload response = apiService.addCover("Test cover title", faxFile);

// Check response state code for success and return false if failed
Console.WriteLine(response.stateCode);
if (response.stateCode != cover.StateCodes.SUCCESS)
    return false;

// Store newly created cover page refID
string coverReference = response.coverRef;
Console.WriteLine();
return true;
```

## Preloading a document
In case the same file must be sent several times or to many recipients, the system provides the ability to upload a document and save it for further reuse.

```cs
// Replace ApiKey and ApiSecret with data from your application settings
string apiSecret = "YOUR-API-KEY";
string apiKey = "YOUR-API-SECRET";

// Replace with https://api.byfax.biz for production
string apiUrl = "https://sandbox.byfax.biz";

// Create and fill auth token object
apiService.UsernameTokenValue = new cover.UsernameToken();
apiService.UsernameTokenValue.Password = apiSecret;
apiService.UsernameTokenValue.Username = apiKey;

// Create service object and fill base params.
document.ApiServiceDocument apiService = new document.ApiServiceDocument();
apiService.Url = $"{apiUrl}/soap/1.1/document";
apiService.Timeout = 2 * 60 * 1000;

string path = "C:\\file.pdf";

// Create and fill FaxFile object
document.FaxFile faxFile = new document.FaxFile();

faxFile.fileCheck = Md5Generator.FromFile(path);
faxFile.fileName = System.IO.Path.GetFileName(path);
faxFile.fileSize = new System.IO.FileInfo(path).Length;
// Important to set that "fileSize" property is specified
faxFile.fileSizeSpecified = true;
// Read file data without encoding to Base64
faxFile.fileData = System.IO.File.ReadAllBytes(path);

document.FaxDocument faxDocument = new document.FaxDocument() { documentFile = faxFile };

// Call Api function "uploadDocument" to upload a document to cache
var response = apiService.uploadDocument(faxDocument);

Console.WriteLine(response.stateCode);
Console.WriteLine();
if (response.stateCode != document.StateCodes.SUCCESS)
    return false;
else
    return true;
```

## Sending a fax (common submission way)
The system provides many options to pass to send fax request - loading documents directly within the request, using previously uploaded documents, using a cover page, submit fax in high or standard quality, submit fax in text or photo mode, submit fax for one or more documents in the request, submit fax for one or more recipients, setting your own fax header format, setting the number of retries in case the number is busy, etc. Below there is an example of using the most common options.

```cs
// Replace ApiKey and ApiSecret with data from your application settings
string apiSecret = "YOUR-API-KEY";
string apiKey = "YOUR-API-SECRET";

// Replace with https://api.byfax.biz for production
string apiUrl = "https://sandbox.byfax.biz";

// Create and fill auth token object
apiService.UsernameTokenValue = new cover.UsernameToken();
apiService.UsernameTokenValue.Password = apiSecret;
apiService.UsernameTokenValue.Username = apiKey;

// Create service object and fill base params.
faxout.ApiServiceFaxout apiService = new faxout.ApiServiceFaxout();
apiService.Url = $"{apiUrl}/soap/1.1/faxout";
apiService.Timeout = 2 * 60 * 1000;

// Crate and fill fax submission request data
faxout.ApiRequestFaxjobSubmit submitRequest = new faxout.ApiRequestFaxjobSubmit();

// Submission broadcast refID.
// Unique within your API account
// Should be unique for each submission
// Uncomment this line to set at your side otherwise API will generate.
// submitRequest.broadcastRef = Md5Generator.FromString("my-broadcast-ref" + DateTime.Now.Millisecond.ToString());

// Fax header format template
submitRequest.Header = "<DateTime> <Timezone>|From: <From> To: <To>|Page <CurPage>of <CurPages>";

// Number of reties in case of Busy or NoAnswer
submitRequest.busyRetry = 3;

// Send quality. Available options are STANDARD or FINE. See the enumeration
submitRequest.sendQuality = faxout.FaxQuality.FINE;

// Send mode. Available options are TEXT or PHOTO. See the enumeration
submitRequest.sendMode = faxout.FaxMode.TEXT;

// Sender identification. At least one of properties is required
submitRequest.Sender = new faxout.FaxContact();
submitRequest.Sender.Name = "My sender name";
submitRequest.Sender.Company = "My sender company";
submitRequest.Sender.Number = "+375 99 111111111";

List<faxout.FaxRecipient> listRecipients = new List<faxout.FaxRecipient>();

// Recipient object. Number and Name/Company are required.
faxout.FaxRecipient recipient = new faxout.FaxRecipient();
recipient.Name = "Recipient name"; // Replace with your recipient name or leave empty
recipient.Company = "Recipient company"; // Replace with your recipient company or leave empty
recipient.Number = "+375 99 111111122"; // If recipient name and company are empty, only recipient number will be set to fax-header

// Unique message reference-ID.
// Uncomment this line to set at your side otherwise API will generate.
// recipient.messageRef = Md5Generator.FromString("my-message-ref" + DateTime.Now.Millisecond.ToString()); 

// Push recipient object to array and create another if it is a batch job
listRecipients.Add(recipient);

// Save recipients array into the request data
submitRequest.Recipients = listRecipients.ToArray();

List<faxout.FaxDocument> listDocuments = new List<faxout.FaxDocument>();

string path = "C:\\file.pdf";

// Document object with a file to upload within exact fax submission.
faxout.FaxFile faxFile = new faxout.FaxFile();
faxFile.fileCheck = Md5Generator.FromFile(path);
faxFile.fileName = System.IO.Path.GetFileName(path);
faxFile.fileSize = new System.IO.FileInfo(path).Length;
faxFile.fileSizeSpecified = true;
faxFile.fileData = System.IO.File.ReadAllBytes(path);

faxout.FaxDocument submitDoc = new faxout.FaxDocument();
submitDoc.documentFile = faxFile;

// Push document object into the array. Add another one if needed.
listDocuments.Add(submitDoc);

submitDoc = new faxout.FaxDocument();
submitDoc.documentRef = "documentRef";

// Push document object into the array.
listDocuments.Add(submitDoc);

// Save documents array into the request data
submitRequest.Documents = listDocuments.ToArray();

// Call "Submit" API function to push fax into the queue
faxout.ApiResponseFaxjobSubmit submitResponse = apiService.Submit(submitRequest);

// Check response state code for success and return false if failed
Console.WriteLine("Submit " + submitResponse.stateCode);
if (submitResponse.stateCode != faxout.StateCodes.SUCCESS)
    return false;

// Fill list of recipients to check status for
List<string> recipientsCheckList = new List<string>();
foreach (var item in submitResponse.reportRecipients)
{
    recipientsCheckList.Add(item.messageRef);
}

// Create and fill check status request object
faxout.ApiRequestFaxjobListMessages checkRequest = new faxout.ApiRequestFaxjobListMessages();

// Add and fill pagination data
checkRequest.pagination = new faxout.ListPagination();
checkRequest.pagination.pageNumber = 0;
checkRequest.pagination.pageSize = 10;
checkRequest.messageRefs = recipientsCheckList.ToArray();

// Call "listRecipients" API function to obtain recipients status
faxout.ApiResponseFaxjobMessages checkResponse = apiService.listRecipients(checkRequest);

Console.WriteLine("listRecipients " + checkResponse.stateCode);

// Check response state code for success and return false if failed
if (checkResponse.stateCode != faxout.StateCodes.SUCCESS)
    return false;

// Obtain and store recipient status
foreach (var item in checkResponse.items)
{
    Console.WriteLine($"{item.messageRef} {item.jobState}");
}
Console.WriteLine();
return true;
```

## Sending a fax (prepared TIFF submission)
This method was specifically designed to send a prepared TIFF file to a single recipient. The method is used only if the TIFF file is prepared on the application`s side and it must be sent without going through the byFax document preparation system. Using this method application should pass only the following data, sender details (Sender object), recipient details (Recipient object), the unique identifier of the container (broadcastRef parameter) and the prepared TIFF file (document object). The full text of the fax header could also be passed to be placed at the top of the page. If the header is already placed to all pages of the document, then the header parameter is passed as empty string. Here is an example of using this function.

```cs
// Replace ApiKey and ApiSecret with data from your application settings
string apiSecret = "YOUR-API-KEY";
string apiKey = "YOUR-API-SECRET";

// Replace with https://api.byfax.biz for production
string apiUrl = "https://sandbox.byfax.biz";

// Create and fill auth token object
apiService.UsernameTokenValue = new cover.UsernameToken();
apiService.UsernameTokenValue.Password = apiSecret;
apiService.UsernameTokenValue.Username = apiKey;

// Create service object and fill base params.
faxout.ApiServiceFaxout apiService = new faxout.ApiServiceFaxout();
apiService.Url = $"{apiUrl}/soap/1.1/faxout";
apiService.Timeout = 2 * 60 * 1000;

// Crate and fill fax submission request data
faxout.ApiRequestFaxjobMessage submitRequest = new faxout.ApiRequestFaxjobMessage();

// Submission broadcast refID.
// Unique within your API account
// Should be unique for each submission
// Uncomment this line to set at your side otherwise API will generate.
// submitRequest.broadcastRef = Md5Generator.FromString("my-broadcast-ref" + DateTime.Now.Millisecond.ToString());

// Fax header format template
submitRequest.Header = "<DateTime> <Timezone>|From: <From> To: <To>|Page <CurPage>of <CurPages>";

// Number of reties in case of Busy or NoAnswer
submitRequest.busyRetry = 3;

// Sender identification. At least one of properties is required
submitRequest.Sender = new faxout.FaxContact();
submitRequest.Sender.Name = "My sender name";
submitRequest.Sender.Company = "My sender company";
submitRequest.Sender.Number = "+375 99 111111111";

// Recipient object. Number and Name/Company are required.
submitRequest.Recipient = new faxout.FaxRecipient();
submitRequest.Recipient.Name = "Recipient name"; // Replace with your recipient name or leave empty
submitRequest.Recipient.Company = "Recipient company"; // Replace with your recipient company or leave empty
submitRequest.Recipient.Number = "+375 99 111111122"; // If recipient name and company are empty, only recipient number will be set to fax-header

// Unique message reference-ID.
// Uncomment this line to set at your side otherwise API will generate.
// submitRequest.Recipient.messageRef = Md5Generator.FromString("my-message-ref" + DateTime.Now.Millisecond.ToString()); 

string path = "C:\\file.tiff";

// Document object with a file to upload within exact fax submission.
faxout.FaxFile faxFile = new faxout.FaxFile();
faxFile.fileCheck = Md5Generator.FromFile(path);
faxFile.fileName = System.IO.Path.GetFileName(path);
faxFile.fileSize = new System.IO.FileInfo(path).Length;
faxFile.fileSizeSpecified = true;
faxFile.fileData = System.IO.File.ReadAllBytes(path);

submitRequest.Document = new faxout.FaxDocument() { documentFile = faxFile };

// Call "SubmitMessage" API function to push fax into the queue
var messageResponse = apiService.SubmitMessage(submitRequest);

// Fill list of recipients to check status for
List<string> recipientsCheckList = new List<string>();
foreach (var item in messageResponse.reportRecipients)
{
    recipientsCheckList.Add(item.messageRef);
}

// Create and fill check status request object
faxout.ApiRequestFaxjobListMessages checkRequest = new faxout.ApiRequestFaxjobListMessages();

// Add and fill pagination data
checkRequest.pagination = new faxout.ListPagination();
checkRequest.pagination.pageNumber = 0;
checkRequest.pagination.pageSize = 10;
checkRequest.messageRefs = recipientsCheckList.ToArray();

// Call "listRecipients" API function to obtain recipients status
faxout.ApiResponseFaxjobMessages checkResponse = apiService.listRecipients(checkRequest);

Console.WriteLine("listRecipients " + checkResponse.stateCode);

// Check response state code for success and return false if failed
if (checkResponse.stateCode != faxout.StateCodes.SUCCESS)
    return false;

// Obtain and store recipient status
foreach (var item in checkResponse.items)
{
    Console.WriteLine($"{item.messageRef} {item.jobState}");
}

Console.WriteLine();
```


Still have questions?
------------

If you still have any questions, or the samples above are not informative enough, you are able get more detailed information about API functions, objects and enumerations in the detailed description of each service (links can be found above), you can also contact us via helpdesk or JivoSite. We are always glad to hear suggestions and ideas for expanding or improving both the byFax API and our entire product.

At the moment, only the basic functionality of the byFax portal is available in our open API, if you need to expand the capabilities or add fundamentally new functions, we are always happy to discuss.

If you are a Java, Ruby, Go or other programming language developer and would like to help improving the byFax API SDK, we will be glad to have your help developing SDKs in other languages.
