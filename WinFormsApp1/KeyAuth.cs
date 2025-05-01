using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace KeyAuthLib
{
    public class KeyAuth
    {
        private readonly string name;
        private readonly string ownerid;
        private readonly string secret;
        private readonly string version;
        private string sessionid;
        private string apiUrl = "https://hyperb57p-qipp.tryxcloud.cc/api/1.2/"; // Default URL
        private bool initialized;
        private static readonly HttpClient client = new HttpClient();

        public KeyAuth(string name, string ownerid, string secret, string version)
        {
            this.name = name;
            this.ownerid = ownerid;
            this.secret = secret;
            this.version = version;

            // Initialize with the latest URL in the background
            _ = UpdateApiUrlAsync();
        }

        private async Task UpdateApiUrlAsync()
        {
            try
            {
                string newUrl = await FetchLatestApiUrlAsync();
                if (!string.IsNullOrEmpty(newUrl))
                {
                    if (!newUrl.EndsWith("/api/1.2/"))
                    {
                        newUrl = newUrl.TrimEnd('/') + "/api/1.2/";
                    }
                    apiUrl = newUrl;
                }
            }
            catch
            {
                // Keep the default URL if fetching fails
            }
        }

        private async Task<string> FetchLatestApiUrlAsync()
        {
            HttpClientHandler handler = new HttpClientHandler();
            var cookieContainer = new CookieContainer();
            handler.CookieContainer = cookieContainer;
            handler.UseCookies = true;

            using (HttpClient tempClient = new HttpClient(handler))
            {
                tempClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                var uri = new Uri("http://www.keyauthpanel.thsite.top/urlshow.php");
                string html = await tempClient.GetStringAsync(uri);

                var (key, iv, enc) = ExtractAESVars(html);
                string cookieVal = DecryptAESCBC(enc, key, iv);

                cookieContainer.Add(uri, new Cookie("__test", cookieVal));

                string response2 = await tempClient.GetStringAsync("http://www.keyauthpanel.thsite.top/urlshow.php?i=1");

                if (response2.Contains("<script>"))
                {
                    var (key2, iv2, enc2) = ExtractAESVars(response2);
                    string cookieVal2 = DecryptAESCBC(enc2, key2, iv2);
                    cookieContainer.Add(uri, new Cookie("__test", cookieVal2));

                    response2 = await tempClient.GetStringAsync("http://www.keyauthpanel.thsite.top/urlshow.php?i=2");
                }

                return response2.Trim();
            }
        }

        private (string keyHex, string ivHex, string cipherHex) ExtractAESVars(string html)
        {
            var match = Regex.Match(html, @"toNumbers\(""([a-f0-9]{32})""\).*?toNumbers\(""([a-f0-9]{32})""\).*?toNumbers\(""([a-f0-9]{32})""\)", RegexOptions.Singleline);
            if (!match.Success)
                throw new Exception("AES variables not found in page.");
            return (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        }

        private string DecryptAESCBC(string cipherHex, string keyHex, string ivHex)
        {
            byte[] cipher = HexToBytes(cipherHex);
            byte[] key = HexToBytes(keyHex);
            byte[] iv = HexToBytes(ivHex);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decrypted = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                    return BitConverter.ToString(decrypted).Replace("-", "").ToLower();
                }
            }
        }

        private byte[] HexToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public async Task<AuthResult> InitializeAsync()
        {
            if (initialized) return new AuthResult { Success = true };

            // Ensure we have the latest URL before making the request
            await UpdateApiUrlAsync();

            var data = new Dictionary<string, string>
            {
                { "type", "init" },
                { "name", name },
                { "ownerid", ownerid },
                { "ver", version }
            };

            try
            {
                var response = await Request(data, true);
                if (response["success"].ToObject<bool>())
                {
                    sessionid = response["sessionid"].ToString();
                    initialized = true;
                    return new AuthResult { Success = true };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = response["message"]?.ToString() ?? "Initialization failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            if (!initialized)
            {
                var initResult = await InitializeAsync();
                if (!initResult.Success) return initResult;
            }

            var hwid = GetHWID();
            var data = new Dictionary<string, string>
            {
                { "type", "login" },
                { "username", username },
                { "pass", password },
                { "hwid", hwid },
                { "sessionid", sessionid },
                { "name", name },
                { "ownerid", ownerid }
            };

            try
            {
                var response = await Request(data);
                if (response["success"].ToObject<bool>())
                {
                    var ip = await GetIPAddress();
                    return new AuthResult
                    {
                        Success = true,
                        Username = username,
                        IPAddress = ip,
                        ExpiryDate = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd HH:mm:ss"),
                        Message = "Login successful"
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = response["message"]?.ToString() ?? "Login failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<AuthResult> RegisterAsync(string username, string password)
        {
            if (!initialized)
            {
                var initResult = await InitializeAsync();
                if (!initResult.Success) return initResult;
            }

            var hwid = GetHWID();
            var data = new Dictionary<string, string>
            {
                { "type", "register" },
                { "username", username },
                { "pass", password },
                { "hwid", hwid },
                { "sessionid", sessionid },
                { "name", name },
                { "ownerid", ownerid }
            };

            try
            {
                var response = await Request(data);
                return new AuthResult
                {
                    Success = response["success"].ToObject<bool>(),
                    Message = response["message"]?.ToString() ?? "Registration failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<AuthResult> ValidateLicenseAsync(string licenseKey)
        {
            if (!initialized)
            {
                var initResult = await InitializeAsync();
                if (!initResult.Success) return initResult;
            }

            var data = new Dictionary<string, string>
            {
                { "type", "validate_license" },
                { "license_key", licenseKey },
                { "sessionid", sessionid },
                { "name", name },
                { "ownerid", ownerid }
            };

            try
            {
                var response = await Request(data);
                return new AuthResult
                {
                    Success = response["success"].ToObject<bool>(),
                    Message = response["message"]?.ToString() ?? "License validation failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = ex.Message };
            }
        }

        private async Task<JObject> Request(Dictionary<string, string> data, bool firstCall = false)
        {
            if (!firstCall) data["enckey"] = GetEncryptionKey();

            var content = new FormUrlEncodedContent(data);
            var response = await client.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();
            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        private string GetEncryptionKey()
        {
            if (string.IsNullOrEmpty(sessionid))
                throw new Exception("Session ID not available");

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(
                Encoding.UTF8.GetBytes(secret + sessionid + name + ownerid));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static string GetHWID()
        {
            var hwid = string.Empty;

            try
            {
                // Processor ID
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                    hwid += obj["ProcessorId"]?.ToString();

                // MAC Address
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up);
                if (nic != null)
                    hwid += nic.GetPhysicalAddress().ToString();
            }
            catch { /* Hardware identification fallback */ }

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hwid));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static async Task<string> GetIPAddress()
        {
            try
            {
                var response = await client.GetStringAsync("https://api.ipify.org?format=json");
                return JObject.Parse(response)["ip"]?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }


}