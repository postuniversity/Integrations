namespace CNSAPIService.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonPicturePayload : IPersonPicturePayload
    {
        //delete
        public string CreatedDateTime { get; set; }
        public bool IsStudent { get; set; }
        public string LastModifiedDateTime { get; set; }
        public int LastModifiedUserId { get; set; }
        public string RowVersion { get; set; }
        public int StaffId { get; set; }
        public string OriginalState { get; set; }
        public string SecureState { get; set; }
        public string ModifiedProperties { get; set; }
        public string OriginalValues { get; set; }
        public string EntityState { get; set; }
        public int StudentId { get; set; }
        //public string PictureImage { get; set; }

        //get
        public int Id { get; set; }
        //
        public int StudentPictureId { get; set; }
       // public int StudentId { get; set; } //StudentId for DeletePersonPicture
        public string Image { get; set; }//PictureImage for deletepersonpicture
        public string ImageExtension { get; set; }

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
