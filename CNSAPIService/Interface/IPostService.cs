using CNSAPIService.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace CNSAPIService.Interface
{
    public interface IPostService
    {
        Task<PersonPictureUploadResponse> UploadStudentPicture(IPersonPicturePayload payload);
        Task<PersonPictureValue> GetStudentPicture(IPersonPicturePayload payload);
    }
}
