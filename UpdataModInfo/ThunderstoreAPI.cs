using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UpdataModInfo
{
    internal class ThunderstoreAPIMain
    {
        [Serializable]
        public class Version
        {
            [JsonProperty("name")]
            public string Name;

            [JsonProperty("full_name")]
            public string FullName;

            [JsonProperty("description")]
            public string Description;

            [JsonProperty("icon")]
            public Uri Icon;

            [JsonProperty("version_number")]
            public string VersionNumber;

            [JsonProperty("dependencies")]
            public object[] Dependencies;

            [JsonProperty("download_url")]
            public Uri DownloadUrl;

            [JsonProperty("downloads")]
            public long Downloads;

            [JsonProperty("date_created")]
            public DateTimeOffset DateCreated;

            [JsonProperty("website_url")]
            public Uri WebsiteUrl;

            [JsonProperty("is_active")]
            public bool IsActive;

            [JsonProperty("uuid4")]
            public Guid Uuid4;

            [JsonProperty("file_size")]
            public long FileSize;
        }

        [Serializable]
        public class Package
        {
            [JsonProperty("name")]
            public string Name;

            [JsonProperty("full_name")]
            public string FullName;

            [JsonProperty("owner")]
            public string Owner;

            [JsonProperty("package_url")]
            public string PackageUrl;

            [JsonProperty("donation_link")]
            public string DonationLink;

            [JsonProperty("date_created")]
            public string DateCreated;

            [JsonProperty("date_updated")]
            public string DateUpdated;

            [JsonProperty("uuid4")]
            public string Uuid4;

            [JsonProperty("rating_score")]
            public string RatingScore;

            [JsonProperty("is_pinned")]
            public string IsPinned;

            [JsonProperty("is_deprecated")]
            public string IsDeprecated;

            [JsonProperty("has_nsfw_content")]
            public string HasNsfwContent;

            [JsonProperty("categories")]
            public string[] Categories;

            [JsonProperty("versions")]
            public Version[] Versions;
        }

        public static class ThunderstoreAPI
        {
            public static async Task<Package[]> ReturnThunderstorePackages()
            {
                Package[] array;
                using (HttpClient httpClient = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), "https://thunderstore.io/c/dyson-sphere-program/api/v1/package/"))
                    {
                        request.Headers.TryAddWithoutValidation("accept", "application/json");
                        HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request);
                        HttpResponseMessage response = httpResponseMessage;
                        httpResponseMessage = null;
                        string text = await response.Content.ReadAsStringAsync();
                        string content = text;
                        text = null;
                        Package[] temp = JsonConvert.DeserializeObject<Package[]>(content);
                        array = temp;
                    }
                }
                return array;
            }

            public static async Task<Package> ReturnThunderstorePackageByName(string name)
            {
                Package package;
                using (HttpClient httpClient = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), "https://thunderstore.io/c/dyson-sphere-program/api/v1/package/"))
                    {
                        request.Headers.TryAddWithoutValidation("accept", "application/json");
                        HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request);
                        HttpResponseMessage response = httpResponseMessage;
                        httpResponseMessage = null;
                        string text = await response.Content.ReadAsStringAsync();
                        string content = text;
                        text = null;
                        Package[] temp = JsonConvert.DeserializeObject<Package[]>(content);
                        package = temp.First((Package x) => x.Name == name);
                    }
                }
                return package;
            }
        }
    }
}
