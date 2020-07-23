using CNSAPIService.Models;

namespace CNSAPIService.Interface
{
    public class PersonPictureUploadResponse
    {
        public bool HasError { get; set; }
        public bool HasFault { get; set; }
        public bool HasSecurityError { get; set; }
        public bool HasValidationError { get; set; }
        public bool HasValidationInformation { get; set; }
        public bool HasValidationWarning { get; set; }
        public bool HasWarning { get; set; }
        public Payload Payload { get; set; }
    }

    /// <summary>
    ///// 
    ///// </summary>
    //public class DeletePersonPicture : IPersonPicturePayload
    //{
    //    public string CreatedDateTime { get; set; }
    //    public bool IsStudent { get; set; }
    //    public string LastModifiedDateTime { get; set; }
    //    public int LastModifiedUserId { get; set; }
    //    public string RowVersion { get; set; }
    //    public int StaffId { get; set; }
    //    public string OriginalState { get; set; }
    //    public string SecureState { get; set; }
    //    public string ModifiedProperties { get; set; }
    //    public string OriginalValues { get; set; }
    //    public string EntityState { get; set; }
    //    public int StudentId { get; set; } 
    //    public string PictureImage { get; set; }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //public class GetPersonPicture: IPersonPicturePayload
    //{
    //    public int Id { get; set; }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //public class UploadStudentPicture : IPersonPicturePayload
    //{
    //    public int StudentPictureId { get; set; }
    //    public int StudentId { get; set; } //StudentId for DeletePersonPicture
    //    public string Image { get; set; }//PictureImage for deletepersonpicture
    //    public string ImageExtension { get; set; }
    //}
}
