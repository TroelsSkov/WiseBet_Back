using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.Models;
namespace WiseBet.backend.Controllers
{
   public abstract class BaseController<T> : ControllerBase where T : IDto
   {
      protected readonly BaseRepository<T, Guid> repository;

      public BaseController(BaseRepository<T, Guid> repo)
      {
         repository = repo;
      }
   }
}
