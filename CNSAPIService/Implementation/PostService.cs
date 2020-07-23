using CNSAPIService.Interface;
using CNSAPIService.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CNSAPIService
{
    public class PostService : IPostService
    {
        private static readonly HttpClient client = new HttpClient();
        public PostService()
        {
            client.DefaultRequestHeaders.Add("ApiKey", "cIoG9yZ8TjgXYEoMxPvpLQmC6eziKoxj");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task<PersonPictureUploadResponse> UploadStudentPicture(IPersonPicturePayload payload)
        {

            var jsonpayload = new Dictionary<string, IPersonPicturePayload>() 
                                    { { "payload", new PersonPicturePayload()
                                                        {
                                                            Image = payload.Image, 
                                                            ImageExtension = payload.ImageExtension.Substring(1), 
                                                            StudentId = Convert.ToInt32(payload.StudentId), 
                                                            StudentPictureId = payload.StudentPictureId 
                                                        } 
                                    }};

             var response = await client.PostAsync("https://sisclientweb-test-100537.campusnexus.cloud/api/commands/Common/PersonPicture/uploadStudentPicture", new StringContent(JsonConvert.SerializeObject(jsonpayload), Encoding.UTF8, "application/json")).ConfigureAwait(false);
             var responseStream = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
             JObject jsonData = JObject.Parse(responseStream.ToString());
             return Newtonsoft.Json.JsonConvert.DeserializeObject<PersonPictureUploadResponse>(jsonData.ToString());
        }

        public async Task<PersonPictureValue> GetStudentPicture(IPersonPicturePayload payload)
        {
            var studentPictureId = await client.GetAsync("https://sisclientweb-test-100537.campusnexus.cloud/ds/campusnexus/PersonPictures?$filter=StudentId eq " + payload.StudentId);
            var spicture = await studentPictureId.Content.ReadAsStringAsync().ConfigureAwait(false);
            JObject spicturejsonData = JObject.Parse(spicture.ToString());
            var studentPictureDataObject =  Newtonsoft.Json.JsonConvert.DeserializeObject<GetPersonPictureOdata>(spicturejsonData.ToString());
            return studentPictureDataObject.Value.ToList().FirstOrDefault();
        }

        //public async Task<PersonPictureValue> DeleteStudentPicture(IPersonPicturePayload payload)
        //{
        //    var jsonpayload = new Dictionary<string, IPersonPicturePayload>()
        //                            { { "payload", new PersonPicturePayload()
        //                                                {
        //                                                    StudentId = Convert.ToInt32(payload.StudentId),
        //                                                     Id = payload.StudentPictureId
        //                                                }
        //                            }};

        //    var studentPictureId = await client.PostAsync("https://sisclientweb-test-100537.campusnexus.cloud/api/commands/Common/PersonPicture/delete");
        //    var spicture = await studentPictureId.Content.ReadAsStringAsync().ConfigureAwait(false);
        //    JObject spicturejsonData = JObject.Parse(spicture.ToString());
        //    var studentPictureDataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<GetPersonPictureOdata>(spicturejsonData.ToString());
        //    return studentPictureDataObject.Value.ToList().FirstOrDefault();
        //}
    }
}
