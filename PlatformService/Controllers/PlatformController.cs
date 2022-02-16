using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformController(
            IPlatformRepo repository,
            IMapper mapper , 
            ICommandDataClient commandDataClient,
            IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("--> Getting platforms");

            var platFormItems = _repository.GetAllPlatforms();
            return Ok( _mapper.Map<IEnumerable<PlatformReadDto>>(platFormItems));
        }

        [HttpGet("{id}", Name ="GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            Console.WriteLine("--> Gettin platform");
            var  platFormItem = _repository.GetPlatformById(id);
            if(platFormItem != null)
            {
                return Ok (_mapper.Map<PlatformReadDto>(platFormItem));
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platform)
        {
            var platFormModel = _mapper.Map<Platform>(platform);

            _repository.CreatePlatForm(platFormModel);
            _repository.SaveChanges();

            var platFormReadDto = _mapper.Map<PlatformReadDto>(platFormModel);
            
            var createdResource = new { Id = platFormReadDto.Id};
            var routeValues = new { id = createdResource.Id };
            
            //Send Sync Message
            try
            {
                await _commandDataClient.SendPlatformToCommand(platFormReadDto);
            }
            catch (System.Exception ex)
            {
                
                Console.WriteLine($"--> Could not sed synchrously: {ex.Message}");
            } 

            //Send Async Message
            try
            {

                var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platFormReadDto);
                platformPublishedDto.Event = "Platform_Published";

                _messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch (System.Exception ex)
            {
                
                Console.WriteLine($"--> Could not sed Asynchrously: {ex.Message}");
            }


            return CreatedAtRoute("GetPlatformById", routeValues, createdResource);
        }
    }
}