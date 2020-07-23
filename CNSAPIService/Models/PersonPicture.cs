//using System.Collections.Generic;
//using System.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace CNSAPIService.Models
{
    //    public class PersonPicture
    //    {

    //    }
    //    public class Create
    //    {

    //    }
    //    public class Get
    //    {

    //    }
    //    public class UploadStudentPicture
    //    {

    //    }

    //}


    public class GetPersonPictureOdata
    {
        [JsonProperty("@odata.context")]
        public string Metadata { get; set; }
        public IEnumerable<PersonPictureValue> Value { get; set; }
    }

    public class PersonPictureValue
    {
        public int Id { get; set; }
        public string CreatedDateTime { get; set; }
        public bool IsStudent { get; set; }
        public string LastModifiedDateTime { get; set; }
        public int LastModifiedUserId { get; set; }
        public string PictureImage { get; set; }
        public Nullable<int> StaffId { get; set; }
        public int StudentId { get; set; }
    }



    public class Payload
    {
        public Data Data { get; set; }
        public int Count { get; set; }
    }
    public class Data
    {
        public int StudentPictureId { get; set; }
        public int StudentId { get; set; }
        public string Image { get; set; }
        public string ImageExtension { get; set; }
        public int Id { get; set; }

        //delete response
        public string CreatedDateTime { get; set; }
        public bool IsStudent { get; set; }
        public string LastModifiedDateTime { get; set; }
        public int LastModifiedUserId { get; set; }
        public string PictureImage { get; set; }
        public string RowVersion { get; set; }

        public int StaffId { get; set; }
        //public int StudentId { get; set; }
        public string OriginalState { get; set; }
        public string SecureState { get; set; }
        public string ModifiedProperties { get; set; }
        public string OriginalValues { get; set; }
        public string EntityState { get; set; }
        //end of delete reponse

    }
}