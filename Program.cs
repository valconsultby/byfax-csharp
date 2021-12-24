using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiSampleFramework
{
    static class Md5Generator
    {
        public static string FromFile(string path)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            string check = "";

            if (File.Exists(path) == false)
                return check;

            try
            {
                // Create a fileStream for the file.
                FileStream fileStream = new FileStream(path, FileMode.Open);

                // Be sure it's positioned to the beginning of the stream.
                fileStream.Position = 0;

                // Compute the hash of the fileStream.
                byte[] hashValue = md5.ComputeHash(fileStream);

                fileStream.Close();

                foreach (byte b in hashValue)
                    check += b.ToString("x2");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return check.ToLower();
        }
        public static string FromString(string content)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            string check = "";
            foreach (byte b in md5.ComputeHash(Encoding.UTF8.GetBytes(content)))
                check += b.ToString("x2");
            return check;
        }
    }
    class Program
    {
        private static readonly string _apiSecret = "YOUR-API-KEY";
        private static readonly string _apiKey = "YOUR-API-SECRET";

        // Replace with https://api.byfax.biz for production
        private static readonly string _apiUrl = "https://sandbox.byfax.biz";

        static void Main(string[] args)
        {
            GetCoverPagesList();

            UploadCoverPages();

            UploadDocument();

            SendFax();

            SendTiffFax();

            Console.ReadLine();
        }

        //Cover pages. List
        private static bool GetCoverPagesList()
        {
            // Create "cover" service object and fill base params
            cover.ApiServiceCover apiService = new cover.ApiServiceCover();
            apiService.Url = $"{_apiUrl}/soap/1.1/cover";
            apiService.Timeout = 2 * 60 * 1000;

            // Create and fill auth token object
            apiService.UsernameTokenValue = new cover.UsernameToken();
            apiService.UsernameTokenValue.Password = _apiSecret;
            apiService.UsernameTokenValue.Username = _apiKey;

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
        }

        //Cover pages. Adding
        private static bool UploadCoverPages()
        {
            // Create "cover" service object and fill base params
            cover.ApiServiceCover apiService = new cover.ApiServiceCover();
            apiService.Url = $"{_apiUrl}/soap/1.1/cover";
            apiService.Timeout = 2 * 60 * 1000;

            // Create and fill auth token object
            apiService.UsernameTokenValue = new cover.UsernameToken();
            apiService.UsernameTokenValue.Password = _apiSecret;
            apiService.UsernameTokenValue.Username = _apiKey;

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
        }

        //Preloading a document
        private static bool UploadDocument()
        {
            // Create "document" service object and fill base params
            document.ApiServiceDocument apiService = new document.ApiServiceDocument();
            apiService.Url = $"{_apiUrl}/soap/1.1/document";
            apiService.Timeout = 2 * 60 * 1000;

            // Create and fill auth token object
            apiService.UsernameTokenValue = new document.UsernameToken();
            apiService.UsernameTokenValue.Password = _apiSecret;
            apiService.UsernameTokenValue.Username = _apiKey;

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
        }

        //Sending a fax (common submission way)
        private static bool SendFax()
        {
            // Create "faxout" service object and fill base params
            faxout.ApiServiceFaxout apiService = new faxout.ApiServiceFaxout();
            apiService.Url = $"{_apiUrl}/soap/1.1/faxout";
            apiService.Timeout = 2 * 60 * 1000;

            // Create and fill auth token object
            apiService.UsernameTokenValue = new faxout.UsernameToken();
            apiService.UsernameTokenValue.Password = _apiSecret;
            apiService.UsernameTokenValue.Username = _apiKey;

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

        }

        //Sending a fax (prepared TIFF submission)
        private static bool SendTiffFax()
        {
            // Create "faxout" service object and fill base params
            faxout.ApiServiceFaxout apiService = new faxout.ApiServiceFaxout();
            apiService.Url = $"{_apiUrl}/soap/1.1/document";
            apiService.Timeout = 2 * 60 * 1000;

            // Create and fill auth token object
            apiService.UsernameTokenValue = new faxout.UsernameToken();
            apiService.UsernameTokenValue.Password = _apiSecret;
            apiService.UsernameTokenValue.Username = _apiKey;

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

            return true;
        }

    }
}
