using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Common.Persistence.Repositories;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/entry")]
    [EnableCors("PublicApiPolicy")]
    public class RadioController : Controller {
        private readonly IEntryRepository _entryRepository;
        private readonly IMapper _mapper;

        public RadioController(IEntryRepository entryRepository,
            IMapper mapper) {
            _entryRepository = entryRepository;
            _mapper = mapper;
        }
    }

    [HttpGet("playlist")]
    public async Task<string> GetPlaylist() {
        // var items = _entryRepository.Get
        return null;
    }
}
