
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{

    [Route("api/c/platforms/{platformId}/[Controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"--> Hit the GetCommandsForPlatform: {platformId}");

            if(!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }
            var commandsItems = _repository.GetCommandsForPlatform(platformId);
            return Ok ( _mapper.Map<IEnumerable<CommandReadDto>>(commandsItems));
        }

        [HttpGet("{commandId}", Name ="GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            Console.WriteLine($"--> Hit the GetCommandsForPlatform: {platformId}");
            
            if(!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var commandItem = _repository.GetCommand(platformId,commandId);
            if(commandItem == null)
            {
                return NotFound();
            }


            return Ok (_mapper.Map<CommandReadDto>(commandItem));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, [FromBody]CommandCreateDto commandDto)
        {
            Console.WriteLine($"--> Hit the CreateCommandForPlatform: {platformId}");
            
            if(!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var command = _mapper.Map<Command>(commandDto);
            _repository.CreateCommand(platformId,command);
            _repository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(command); 
             var createdResource = new { Id = commandReadDto.Id};
            var routeValues = new { platformId = platformId, commandId = commandReadDto.Id };

             return CreatedAtRoute("GetCommandForPlatform", routeValues, createdResource);

        }
    }
}