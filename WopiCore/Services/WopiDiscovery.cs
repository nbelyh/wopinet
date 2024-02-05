using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using WopiCore.Models;
using System.IO;

namespace WopiCore.Services
{
    public class WopiDiscovery
    {
        private readonly IWopiConfiguration wopiConfig;
        private readonly IMemoryCache memoryCache;

        public WopiDiscovery(IWopiConfiguration wopiConfig, IMemoryCache memoryCache)
        {
            this.wopiConfig = wopiConfig;
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// Contains all valid URL placeholders for different WOPI actions
        /// </summary>
        public class WopiUrlPlaceholders
        {
            public static List<string> Placeholders = new List<string>() { BUSINESS_USER,
            DC_LLCC, DISABLE_ASYNC, DISABLE_CHAT, DISABLE_BROADCAST,
            EMBDDED, FULLSCREEN, PERFSTATS, RECORDING, THEME_ID, UI_LLCC,
            VALIDATOR_TEST_CATEGORY, HOST_SESSION_ID, ACTIVITY_NAVIGATION_ID,
            SESSION_CONTEXT, WOPI_SOURCE
        };
            public const string BUSINESS_USER = "<IsLicensedUser=BUSINESS_USER&>";
            public const string DC_LLCC = "<rs=DC_LLCC&>";
            public const string DISABLE_ASYNC = "<na=DISABLE_ASYNC&>";
            public const string DISABLE_CHAT = "<dchat=DISABLE_CHAT&>";
            public const string DISABLE_BROADCAST = "<vp=DISABLE_BROADCAST&>";
            public const string EMBDDED = "<e=EMBEDDED&>";
            public const string FULLSCREEN = "<fs=FULLSCREEN&>";
            public const string PERFSTATS = "<showpagestats=PERFSTATS&>";
            public const string RECORDING = "<rec=RECORDING&>";
            public const string THEME_ID = "<thm=THEME_ID&>";
            public const string UI_LLCC = "<ui=UI_LLCC&>";
            public const string VALIDATOR_TEST_CATEGORY = "<testcategory=VALIDATOR_TEST_CATEGORY>";
            public const string HOST_SESSION_ID = "<hid=HOST_SESSION_ID&>";
            public const string ACTIVITY_NAVIGATION_ID = "<actnavid=ACTIVITY_NAVIGATION_ID&>";
            public const string SESSION_CONTEXT = "<sc=SESSION_CONTEXT&>";
            public const string WOPI_SOURCE = "<wopisrc=WOPI_SOURCE&>";

            /// <summary>
            /// Sets a specific WOPI URL placeholder with the correct value
            /// Most of these are hard-coded in this WOPI implementation
            /// </summary>
            public static string GetPlaceholderValue(string placeholder)
            {
                var ph = placeholder.Substring(1, placeholder.IndexOf("="));
                switch (placeholder)
                {
                    case BUSINESS_USER:
                        return ph + "1";
                    case DC_LLCC:
                    case UI_LLCC:
                        return ph + "en-US";
                    case DISABLE_ASYNC:
                    case DISABLE_BROADCAST:
                    case EMBDDED:
                    case FULLSCREEN:
                    case RECORDING:
                    case THEME_ID:
                        // These are all broadcast related actions
                        return ph + "true";
                    case DISABLE_CHAT:
                        return ph + "false";
                    case PERFSTATS:
                        return ""; // No documentation
                    case VALIDATOR_TEST_CATEGORY:
                        return ph + "OfficeOnline"; //This value can be set to All, OfficeOnline or OfficeNativeClient to activate tests specific to Office Online and Office for iOS. If omitted, the default value is All.  
                    case HOST_SESSION_ID:
                        return "";
                    case ACTIVITY_NAVIGATION_ID:
                        return "";
                    case SESSION_CONTEXT:
                        return "";
                    case WOPI_SOURCE: // append it anyways at the end
                        return "";
                    default:
                        return "";

                }
            }
        }



        //WOPI protocol constants
        public const string WOPI_BASE_PATH = @"/wopi/";
        public const string WOPI_CHILDREN_PATH = @"/children";
        public const string WOPI_CONTENTS_PATH = @"/contents";
        public const string WOPI_FILES_PATH = @"files/";
        public const string WOPI_FOLDERS_PATH = @"folders/";

        /// <summary>
        /// Gets the discovery information from WOPI discovery and caches it appropriately
        /// </summary>
        public async Task<List<WopiAction>> GetActions(IAuthInfo auth)
        {
            List<WopiAction> actions = new List<WopiAction>();

            if (!memoryCache.TryGetValue("DiscoData", out actions))
            {
                await Refresh(auth);
                if (!memoryCache.TryGetValue("DiscoData", out actions))
                {
                    throw new Exception("Could not retrieve WOPI discovery data");
                }
            }
            return actions;
        }

        public async Task Refresh(IAuthInfo auth)
        {

            // Use the Wopi Discovery endpoint to get the data
            HttpClient client = new HttpClient();
            var discoveryUrl = wopiConfig.GetWopiDiscovery(auth);
            using (HttpResponseMessage response = await client.GetAsync(discoveryUrl))
            {
                if (response.IsSuccessStatusCode)
                {
                    var actions = new List<WopiAction>();

                    // Read the xml string from the response
                    var xmlString = await response.Content.ReadAsStringAsync();

                    // Parse the xml string into Xml
                    var discoXml = XDocument.Parse(xmlString);

                    // Convert the discovery xml into list of WopiApp
                    var xapps = discoXml.XPathSelectElements("//net-zone[@name='external-https']/app");
                    foreach (var xapp in xapps)
                    {
                        // Parse the actions for the app
                        var xactions = xapp.Descendants("action");
                        foreach (var xaction in xactions)
                        {
                            actions.Add(new WopiAction()
                            {
                                app = xapp.Attribute("name").Value,
                                favIconUrl = xapp.Attribute("favIconUrl").Value,
                                checkLicense = Convert.ToBoolean(xapp.Attribute("checkLicense").Value),
                                name = xaction.Attribute("name").Value,
                                ext = xaction.Attribute("ext") != null ? xaction.Attribute("ext").Value : string.Empty,
                                progid = xaction.Attribute("progid") != null ? xaction.Attribute("progid").Value : string.Empty,
                                isDefault = xaction.Attribute("default") != null ? true : false,
                                urlsrc = xaction.Attribute("urlsrc").Value,
                                requires = xaction.Attribute("requires") != null ? xaction.Attribute("requires").Value : string.Empty
                            });
                        }

                        // Cache the discovey data for an hour
                        memoryCache.Set("DiscoData", actions, DateTimeOffset.Now.AddHours(1));
                    }

                    // Convert the discovery xml into list of WopiApp
                    var proof = discoXml.Descendants("proof-key").FirstOrDefault();
                    var wopiProof = new WopiProof()
                    {
                        value = proof.Attribute("value").Value,
                        modulus = proof.Attribute("modulus").Value,
                        exponent = proof.Attribute("exponent").Value,
                        oldvalue = proof.Attribute("oldvalue").Value,
                        oldmodulus = proof.Attribute("oldmodulus").Value,
                        oldexponent = proof.Attribute("oldexponent").Value
                    };

                    // Add to cache for 20min
                    memoryCache.Set("WopiProof", wopiProof, DateTimeOffset.Now.AddMinutes(15));
                }
            }

        }

        /// <summary>
        /// Forms the correct action url for the file and host
        /// </summary>
        public async Task<string> GetActionUrl(string action, string fileName, string fileId, IAuthInfo auth)
        {
            var host = wopiConfig.GetWopiBackend(auth);

            // Initialize the urlsrc
            var actions = await GetActions(auth);
            var ext = Path.GetExtension(fileName).TrimStart('.');
            var found = actions.FirstOrDefault(i => i.name == action && i.ext == ext);
            if (found == null)
                return null;

            var urlsrc = found.urlsrc;

            // Look through the action placeholders
            var phCnt = 0;
            foreach (var p in WopiUrlPlaceholders.Placeholders)
            {
                if (urlsrc.Contains(p))
                {
                    // Replace the placeholder value accordingly
                    var ph = WopiUrlPlaceholders.GetPlaceholderValue(p);
                    if (!string.IsNullOrEmpty(ph))
                    {
                        urlsrc = urlsrc.Replace(p, ph + "&");
                        phCnt++;
                    }
                    else
                        urlsrc = urlsrc.Replace(p, ph);
                }
            }

            // Add the WOPISrc to the end of the request
            // urlsrc += ((phCnt > 0) ? "" : "?") + String.Format("WOPISrc=https://{0}/wopi/files/{1}", "bnk-vs-2022.swedencentral.cloudapp.azure.com", fileId);
            urlsrc += (phCnt > 0 ? "" : "?") + $"WOPISrc={host}/wopi/files/{fileId}";
            return urlsrc;
        }


        internal async Task<WopiProof> getWopiProof(IAuthInfo auth)
        {
            var wopiProof = new WopiProof();
            // Check cache for this data
            if (!memoryCache.TryGetValue("WopiProof", out wopiProof))
            {
                await Refresh(auth);
                if (!memoryCache.TryGetValue("WopiProof", out wopiProof))
                {
                    throw new Exception("Could not retrieve WOPI proof data");
                }
            }
            return wopiProof;
        }

        /// <summary>
        /// Validates the WOPI Proof on an incoming WOPI request
        /// </summary>
        public async Task<bool> Validate(WopiRequest wopiRequest, IAuthInfo auth)
        {
            var backendUrl = wopiConfig.GetWopiBackend(auth);
            var hostUrl = Regex.Replace(wopiRequest.RequestUri.OriginalString, @"http://localhost:\d+", backendUrl).Replace(":443", "");

            var timeStamp = long.Parse(wopiRequest.Timestamp);
            var timeStampDateTime = new DateTime(timeStamp, DateTimeKind.Utc);
            if ((DateTime.UtcNow - timeStampDateTime).TotalMinutes > 20)
                return false;

            // Make sure the request has the correct headers
            if (wopiRequest.Proof == null ||
                wopiRequest.Timestamp == null)
                return false;

            // Set the requested proof values
            var requestProof = wopiRequest.Proof;
            var requestProofOld = string.Empty;
            if (wopiRequest.ProofOld != null)
                requestProofOld = wopiRequest.ProofOld;

            // Get the WOPI proof info from discovery
            var discoProof = await getWopiProof(auth);

            // Encode the values into bytes
            var accessTokenBytes = Encoding.UTF8.GetBytes(wopiRequest.AccessToken);
            var hostUrlBytes = Encoding.UTF8.GetBytes(hostUrl.ToUpperInvariant());
            var timeStampBytes = BitConverter.GetBytes(Convert.ToInt64(wopiRequest.Timestamp)).Reverse().ToArray();

            // Build expected proof
            List<byte> expected = new List<byte>(
                4 + accessTokenBytes.Length +
                4 + hostUrlBytes.Length +
                4 + timeStampBytes.Length);

            // Add the values to the expected variable
            expected.AddRange(BitConverter.GetBytes(accessTokenBytes.Length).Reverse().ToArray());
            expected.AddRange(accessTokenBytes);
            expected.AddRange(BitConverter.GetBytes(hostUrlBytes.Length).Reverse().ToArray());
            expected.AddRange(hostUrlBytes);
            expected.AddRange(BitConverter.GetBytes(timeStampBytes.Length).Reverse().ToArray());
            expected.AddRange(timeStampBytes);
            byte[] expectedBytes = expected.ToArray();

            return verifyProof(expectedBytes, requestProof, discoProof.value) ||
                verifyProof(expectedBytes, requestProof, discoProof.oldvalue) ||
                verifyProof(expectedBytes, requestProofOld, discoProof.value);
        }

        /// <summary>
        /// Verifies the proof against a specified key
        /// </summary>
        private bool verifyProof(byte[] expectedProof, string proofFromRequest, string proofFromDiscovery)
        {
            using (RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider())
            {
                try
                {
                    rsaProvider.ImportCspBlob(Convert.FromBase64String(proofFromDiscovery));
                    return rsaProvider.VerifyData(expectedProof, SHA256.Create(), Convert.FromBase64String(proofFromRequest));
                }
                catch (FormatException)
                {
                    return false;
                }
                catch (CryptographicException)
                {
                    return false;
                }
            }
        }
    }
}
