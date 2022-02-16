using AutoMapper;
using Grpc.Core;
using PlatformService.Data;

namespace PlatformService.SyncDataServices.Grpc
{
    public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
    {
        private readonly IPlatformRepo _repostory;
        private readonly IMapper _mapper;

        public GrpcPlatformService(IPlatformRepo repostory, IMapper mapper)
        {
            _repostory = repostory;
            _mapper = mapper;
        }

        public override Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context)
        {
            var response = new PlatformResponse();
            var platforms = _repostory.GetAllPlatforms();

            foreach (var platform in platforms)
            {
                response.Platform.Add( _mapper.Map<GrpcPlatformModel>(platform) );
            }

            return  Task.FromResult(response);
        }
    }
}